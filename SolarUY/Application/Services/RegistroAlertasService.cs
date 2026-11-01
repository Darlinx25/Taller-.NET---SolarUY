using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SolarUY.Application.Dtos;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Repositories;

namespace SolarUY.Application.Services;

/// <summary>
/// Detecta los fallos de paneles en cada lectura válida de telemetría: abre una alerta cuando un panel
/// pasa a <c>EnFalla</c> y la cierra cuando se recupera o deja de aparecer en la lectura.
/// </summary>
public class RegistroAlertasService : IRegistroAlertasService
{
    private const string EstadoEnFalla = "EnFalla";

    private readonly IAlertaPanelRepository _alertaPanelRepository;
    private readonly IAlertasNotifier _notificador;
    private readonly ILogger<RegistroAlertasService> _logger;

    public RegistroAlertasService(
        IAlertaPanelRepository alertaPanelRepository,
        IAlertasNotifier notificador,
        ILogger<RegistroAlertasService> logger)
    {
        _alertaPanelRepository = alertaPanelRepository
            ?? throw new ArgumentNullException(nameof(alertaPanelRepository));
        _notificador = notificador ?? throw new ArgumentNullException(nameof(notificador));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RegistrarLecturaAsync(
        TelemetriaDto lectura,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lectura);

        if (lectura.Parques is null || lectura.Parques.Count == 0
            || lectura.FechaHora is null || lectura.FechaHora == default(DateTime))
        {
            return;
        }

        var activasPorPanel = await ObtenerActivasPorPanelAsync(cancellationToken).ConfigureAwait(false);
        var panelesLeidos = new HashSet<Guid>();
        var abiertas = 0;
        var cerradas = 0;

        foreach (var parque in lectura.Parques)
        {
            if (parque?.Nodos is null)
                continue;

            foreach (var nodo in parque.Nodos)
            {
                if (nodo?.Paneles is null)
                    continue;

                foreach (var panel in nodo.Paneles)
                {
                    if (panel is null)
                        continue;

                    panelesLeidos.Add(panel.PanelId);
                    activasPorPanel.TryGetValue(panel.PanelId, out var activa);

                    if (EstaEnFalla(panel))
                    {
                        if (activa is not null)
                            continue;

                        var nueva = await AbrirSiCorrespondeAsync(parque, nodo, panel, cancellationToken)
                            .ConfigureAwait(false);
                        if (nueva is null)
                            continue;

                        activasPorPanel[panel.PanelId] = nueva;
                        abiertas++;
                    }
                    else if (activa is not null && activa.EstaActiva && activa.Cerrar(panel.FechaHora))
                    {
                        cerradas++;
                    }
                }
            }
        }

        cerradas += CerrarAusentes(activasPorPanel, panelesLeidos, lectura.FechaHora.Value);

        if (abiertas == 0 && cerradas == 0)
            return;

        await _alertaPanelRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Alertas de paneles actualizadas con la lectura {FechaHora:o}: {Abiertas} abierta(s), {Cerradas} cerrada(s).",
            lectura.FechaHora,
            abiertas,
            cerradas);

        await NotificarAsync(cancellationToken).ConfigureAwait(false);
    }

    private static bool EstaEnFalla(TelemetriaPanelDto panel)
    {
        return string.Equals(panel.Estado, EstadoEnFalla, StringComparison.Ordinal);
    }

    private async Task<Dictionary<Guid, AlertaPanel>> ObtenerActivasPorPanelAsync(
        CancellationToken cancellationToken)
    {
        var activas = await _alertaPanelRepository
            .ObtenerActivasAsync(cancellationToken)
            .ConfigureAwait(false);

        var activasPorPanel = new Dictionary<Guid, AlertaPanel>();
        foreach (var alerta in activas)
        {
            // Vienen de la más reciente a la más antigua: si hubiera más de una por panel, prevalece la más reciente.
            activasPorPanel.TryAdd(alerta.PanelId, alerta);
        }

        return activasPorPanel;
    }

    private async Task<AlertaPanel?> AbrirSiCorrespondeAsync(
        TelemetriaParqueSolarDto parque,
        TelemetriaNodoDto nodo,
        TelemetriaPanelDto panel,
        CancellationToken cancellationToken)
    {
        var ultima = await _alertaPanelRepository
            .ObtenerUltimaPorPanelAsync(panel.PanelId, cancellationToken)
            .ConfigureAwait(false);

        if (!AlertaPanel.DebeAbrirse(ultima, panel.FechaHora))
            return null;

        var alerta = AlertaPanel.Abrir(
            parque.ParqueSolarId,
            parque.NombreParque,
            nodo.NodoId,
            nodo.CodigoNodo,
            panel.PanelId,
            panel.CodigoSerie,
            panel.FechaHora);

        await _alertaPanelRepository.AgregarAsync(alerta, cancellationToken).ConfigureAwait(false);
        return alerta;
    }

    private static int CerrarAusentes(
        Dictionary<Guid, AlertaPanel> activasPorPanel,
        HashSet<Guid> panelesLeidos,
        DateTime fechaHoraLectura)
    {
        var cerradas = 0;

        foreach (var (panelId, alerta) in activasPorPanel)
        {
            if (panelesLeidos.Contains(panelId) || !alerta.EstaActiva)
                continue;

            if (alerta.Cerrar(fechaHoraLectura))
                cerradas++;
        }

        return cerradas;
    }

    private async Task NotificarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _notificador.NotificarAlertasActualizadasAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No se pudo notificar la actualización de alertas.");
        }
    }
}
