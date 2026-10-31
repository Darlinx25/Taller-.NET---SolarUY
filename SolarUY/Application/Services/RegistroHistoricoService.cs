using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarUY.Application.Dtos;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Enums;
using SolarUY.Domain.Repositories;

namespace SolarUY.Application.Services;

public interface IRegistroHistoricoService
{
    Task RegistrarLecturaAsync(TelemetriaDto lectura, CancellationToken cancellationToken = default);
}

/// <summary>
/// Actualiza los resúmenes diarios de generación con cada lectura válida de telemetría.
/// </summary>
public class RegistroHistoricoService : IRegistroHistoricoService
{
    private const string EstadoOperativo = "Operativo";
    private const string EstadoEnFalla = "EnFalla";

    private readonly IHistoricoRepository _historicoRepository;
    private readonly ConfiguracionHistorico _configuracion;
    private readonly ILogger<RegistroHistoricoService> _logger;

    public RegistroHistoricoService(
        IHistoricoRepository historicoRepository,
        IOptions<ConfiguracionHistorico> opciones,
        ILogger<RegistroHistoricoService> logger)
    {
        _historicoRepository = historicoRepository
            ?? throw new ArgumentNullException(nameof(historicoRepository));
        ArgumentNullException.ThrowIfNull(opciones);
        _configuracion = opciones.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RegistrarLecturaAsync(
        TelemetriaDto lectura,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lectura);

        if (lectura.Parques is null || lectura.Parques.Count == 0)
            return;

        if (lectura.FechaHora is null || lectura.FechaHora == default)
            throw new ArgumentException("La fecha y hora de la lectura es obligatoria.", nameof(lectura));

        var fechaLocal = ObtenerFechaLocal(lectura.FechaHora.Value);
        var resumenes = await _historicoRepository
            .ObtenerPorFechaAsync(fechaLocal, cancellationToken)
            .ConfigureAwait(false);

        var parques = resumenes.ToDictionary(resumen => resumen.ParqueSolarId);
        var nodos = resumenes
            .SelectMany(resumen => resumen.Nodos)
            .ToDictionary(resumen => resumen.NodoId);
        var paneles = resumenes
            .SelectMany(resumen => resumen.Nodos)
            .SelectMany(resumen => resumen.Paneles)
            .ToDictionary(resumen => resumen.PanelId);

        var hayCambios = false;

        foreach (var parque in lectura.Parques)
        {
            if (parque is null)
                continue;

            if (!parques.TryGetValue(parque.ParqueSolarId, out var resumenParque))
            {
                resumenParque = ResumenDiarioParque.Crear(
                    parque.ParqueSolarId,
                    parque.NombreParque,
                    fechaLocal);

                await _historicoRepository
                    .AgregarAsync(resumenParque, cancellationToken)
                    .ConfigureAwait(false);

                parques[parque.ParqueSolarId] = resumenParque;
            }

            var estadoParque = ObtenerEstadoConsolidado(
                parque.Nodos.SelectMany(nodo => nodo.Paneles));
            hayCambios |= RegistrarLectura(
                resumenParque.RegistrarLectura(
                    parque.FechaHora,
                    parque.PotenciaGeneradaW,
                    estadoParque,
                    _configuracion.BrechaMaximaSegundos),
                resumenParque.Id,
                parque.ParqueSolarId,
                parque.FechaHora);

            foreach (var nodo in parque.Nodos)
            {
                if (nodo is null)
                    continue;

                if (!nodos.TryGetValue(nodo.NodoId, out var resumenNodo))
                {
                    resumenNodo = ResumenDiarioNodo.Crear(
                        resumenParque.Id,
                        nodo.NodoId,
                        nodo.CodigoNodo,
                        fechaLocal);

                    resumenParque.AgregarNodo(resumenNodo);
                    nodos[nodo.NodoId] = resumenNodo;
                }

                var estadoNodo = ObtenerEstadoConsolidado(nodo.Paneles);
                hayCambios |= RegistrarLectura(
                    resumenNodo.RegistrarLectura(
                        nodo.FechaHora,
                        nodo.PotenciaGeneradaW,
                        estadoNodo,
                        _configuracion.BrechaMaximaSegundos),
                    resumenNodo.Id,
                    nodo.NodoId,
                    nodo.FechaHora);

                foreach (var panel in nodo.Paneles)
                {
                    if (panel is null)
                        continue;

                    if (!paneles.TryGetValue(panel.PanelId, out var resumenPanel))
                    {
                        resumenPanel = ResumenDiarioPanel.Crear(
                            resumenNodo.Id,
                            panel.PanelId,
                            panel.CodigoSerie,
                            fechaLocal);

                        resumenNodo.AgregarPanel(resumenPanel);
                        paneles[panel.PanelId] = resumenPanel;
                    }

                    hayCambios |= RegistrarLectura(
                        resumenPanel.RegistrarLectura(
                            panel.FechaHora,
                            panel.PotenciaGeneradaW,
                            ConvertirEstado(panel.Estado),
                            _configuracion.BrechaMaximaSegundos),
                        resumenPanel.Id,
                        panel.PanelId,
                        panel.FechaHora);
                }
            }
        }

        if (hayCambios)
        {
            await _historicoRepository
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private DateOnly ObtenerFechaLocal(DateTime fechaHoraUtc)
    {
        var fechaHoraLocal = fechaHoraUtc.AddHours(_configuracion.DesfaseHorarioUruguayHoras);
        return DateOnly.FromDateTime(fechaHoraLocal);
    }

    private static EstadoPanel ObtenerEstadoConsolidado(IEnumerable<TelemetriaPanelDto> paneles)
    {
        return paneles.Any(panel => panel.Estado == EstadoEnFalla)
            ? EstadoPanel.EnFalla
            : EstadoPanel.Operativo;
    }

    private static EstadoPanel ConvertirEstado(string estado)
    {
        return estado switch
        {
            EstadoOperativo => EstadoPanel.Operativo,
            EstadoEnFalla => EstadoPanel.EnFalla,
            _ => throw new ArgumentOutOfRangeException(
                nameof(estado),
                "El estado del panel no es válido.")
        };
    }

    private bool RegistrarLectura(
        bool registrada,
        Guid resumenId,
        Guid entidadId,
        DateTime fechaHora)
    {
        if (!registrada && _logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Lectura histórica ignorada por duplicada o fuera de orden. Resumen: {ResumenId}, entidad: {EntidadId}, fecha: {FechaHora:o}.",
                resumenId,
                entidadId,
                fechaHora);
        }

        return registrada;
    }
}
