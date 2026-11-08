using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Repositories;

namespace SolarUY.Application.Services;

public class DashboardService : IDashboardService
{
    private const string EstadoSinTelemetria = "Sin telemetría";
    private const string EstadoDesconocido = "Desconocido";
    private const string EstadoEnFalla = "EnFalla";

    private readonly IParqueSolarRepository _parqueSolarRepository;
    private readonly INodoRepository _nodoRepository;
    private readonly IPanelRepository _panelRepository;
    private readonly ITelemetriaService _telemetriaService;
    private readonly IControlSimulacionService _controlSimulacionService;

    public DashboardService(
        IParqueSolarRepository parqueSolarRepository,
        INodoRepository nodoRepository,
        IPanelRepository panelRepository,
        ITelemetriaService telemetriaService,
        IControlSimulacionService controlSimulacionService)
    {
        _controlSimulacionService = controlSimulacionService
            ?? throw new ArgumentNullException(nameof(controlSimulacionService));
        _parqueSolarRepository = parqueSolarRepository ?? throw new ArgumentNullException(nameof(parqueSolarRepository));
        _nodoRepository = nodoRepository ?? throw new ArgumentNullException(nameof(nodoRepository));
        _panelRepository = panelRepository ?? throw new ArgumentNullException(nameof(panelRepository));
        _telemetriaService = telemetriaService ?? throw new ArgumentNullException(nameof(telemetriaService));
    }

    public async Task<DashboardTelemetriaViewDto> ObtenerDashboardAsync(CancellationToken cancellationToken = default)
    {
        var parques = await _parqueSolarRepository.ObtenerTodosAsync(cancellationToken).ConfigureAwait(false);
        var lectura = await _telemetriaService.ObtenerUltimaLecturaAsync(cancellationToken).ConfigureAwait(false);
        var control = await _controlSimulacionService.ObtenerAsync(cancellationToken).ConfigureAwait(false);

        var parquesTelemetria = new Dictionary<Guid, TelemetriaParqueSolarDto>();
        var nodosTelemetria = new Dictionary<Guid, TelemetriaNodoDto>();
        var panelesTelemetria = new Dictionary<Guid, TelemetriaPanelDto>();

        if (lectura?.Parques is not null)
        {
            foreach (var parqueTelemetria in lectura.Parques)
            {
                parquesTelemetria[parqueTelemetria.ParqueSolarId] = parqueTelemetria;

                if (parqueTelemetria.Nodos is null)
                    continue;

                foreach (var nodoTelemetria in parqueTelemetria.Nodos)
                {
                    nodosTelemetria[nodoTelemetria.NodoId] = nodoTelemetria;

                    if (nodoTelemetria.Paneles is null)
                        continue;

                    foreach (var panelTelemetria in nodoTelemetria.Paneles)
                    {
                        panelesTelemetria[panelTelemetria.PanelId] = panelTelemetria;
                    }
                }
            }
        }

        var listaParques = new List<DashboardParqueDto>();

        foreach (var parque in parques)
        {
            var nodos = new List<DashboardNodoDto>();
            var nodosDominio = await _nodoRepository.ObtenerPorParqueAsync(parque.Id, cancellationToken).ConfigureAwait(false);

            foreach (var nodo in nodosDominio)
            {
                var panelesDominio = await _panelRepository.ObtenerPorNodoAsync(nodo.Id, cancellationToken).ConfigureAwait(false);
                var paneles = panelesDominio
                    .Select(panel => MapearPanel(panel, ObtenerValor(panelesTelemetria, panel.Id)))
                    .ToList();

                nodosTelemetria.TryGetValue(nodo.Id, out var nodoTelemetria);
                var generacionNodo = nodoTelemetria?.PotenciaGeneradaW ?? paneles.Sum(panel => panel.Generacion);

                nodos.Add(new DashboardNodoDto
                {
                    Id = nodo.Id,
                    Codigo = nodo.Codigo,
                    Generacion = generacionNodo,
                    Paneles = paneles
                });
            }

            parquesTelemetria.TryGetValue(parque.Id, out var parqueTelemetria);
            var generacionParque = parqueTelemetria?.PotenciaGeneradaW ?? nodos.Sum(nodo => nodo.Generacion);

            listaParques.Add(new DashboardParqueDto
            {
                Id = parque.Id,
                Nombre = parque.Nombre,
                Ubicacion = parque.Ubicacion,
                GeneracionTotal = generacionParque,
                Nodos = nodos
            });
        }

        return new DashboardTelemetriaViewDto
        {
            FechaHora = lectura?.FechaHora,
            Parques = listaParques,
            TieneDatos = lectura is not null && lectura.Parques.Count > 0,
            SimulacionForzadaActiva = control.SimulacionForzada,
            MensajeSinDatos = "Sin telemetría disponible"
        };
    }

    private static DashboardPanelDto MapearPanel(Panel panel, TelemetriaPanelDto? telemetria)
    {
        if (telemetria is null)
        {
            return new DashboardPanelDto
            {
                Id = panel.Id,
                CodigoSerie = panel.CodigoSerie,
                Marca = panel.Marca,
                Modelo = panel.Modelo,
                PotenciaWp = panel.PotenciaWp,
                Generacion = 0,
                EstadoSalud = EstadoSinTelemetria,
                EsFallo = false
            };
        }

        var estado = string.IsNullOrWhiteSpace(telemetria.Estado) ? EstadoDesconocido : telemetria.Estado;

        return new DashboardPanelDto
        {
            Id = panel.Id,
            CodigoSerie = panel.CodigoSerie,
            Marca = panel.Marca,
            Modelo = panel.Modelo,
            PotenciaWp = panel.PotenciaWp,
            Generacion = telemetria.PotenciaGeneradaW,
            EstadoSalud = estado,
            EsFallo = EsEstadoDeFalla(estado)
        };
    }

    private static TValue? ObtenerValor<TValue>(Dictionary<Guid, TValue> valores, Guid id)
        where TValue : class
    {
        return valores.TryGetValue(id, out var valor) ? valor : null;
    }

    private static bool EsEstadoDeFalla(string estado)
    {
        return string.Equals(estado, EstadoEnFalla, StringComparison.OrdinalIgnoreCase);
    }
}
