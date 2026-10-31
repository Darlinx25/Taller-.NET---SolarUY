using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;
using SolarUY.Domain.Repositories;

namespace SolarUY.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IParqueSolarRepository _parqueSolarRepository;
    private readonly ITelemetriaService _telemetriaService;

    public DashboardService(
        IParqueSolarRepository parqueSolarRepository,
        ITelemetriaService telemetriaService)
    {
        _parqueSolarRepository = parqueSolarRepository ?? throw new ArgumentNullException(nameof(parqueSolarRepository));
        _telemetriaService = telemetriaService ?? throw new ArgumentNullException(nameof(telemetriaService));
    }

    public async Task<DashboardTelemetriaViewDto> ObtenerDashboardAsync(CancellationToken cancellationToken = default)
    {
        var parques = await _parqueSolarRepository.ObtenerTodosAsync(cancellationToken).ConfigureAwait(false);
        var lectura = await _telemetriaService.ObtenerUltimaLecturaAsync(cancellationToken).ConfigureAwait(false);

        if (lectura is null || lectura.Parques is null || lectura.Parques.Count == 0)
        {
            var vacio = new DashboardTelemetriaViewDto
            {
                FechaHora = null,
                Parques = Array.Empty<DashboardParqueDto>(),
                TieneDatos = false,
                MensajeSinDatos = "Sin telemetría disponible"
            };

            return vacio;
        }

        var parquesPorId = parques.ToDictionary(p => p.Id);
        var listaParques = new List<DashboardParqueDto>();

        foreach (var parqueTelemetria in lectura.Parques)
        {
            parquesPorId.TryGetValue(parqueTelemetria.ParqueSolarId, out var parqueDominio);

            var nodos = new List<DashboardNodoDto>();
            if (parqueTelemetria.Nodos != null)
            {
                foreach (var nodoTelemetria in parqueTelemetria.Nodos)
                {
                    var paneles = new List<DashboardPanelDto>();
                    if (nodoTelemetria.Paneles != null)
                    {
                        foreach (var panelTelemetria in nodoTelemetria.Paneles)
                        {
                            var estado = string.IsNullOrWhiteSpace(panelTelemetria.Estado) ? "Desconocido" : panelTelemetria.Estado;
                            var esFallo = string.Equals(estado, "Fallo", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(estado, "Error", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(estado, "FALLA", StringComparison.OrdinalIgnoreCase);

                            paneles.Add(new DashboardPanelDto
                            {
                                Id = panelTelemetria.PanelId,
                                CodigoSerie = panelTelemetria.CodigoSerie,
                                Marca = string.Empty,
                                Modelo = string.Empty,
                                PotenciaWp = 0,
                                Generacion = panelTelemetria.PotenciaGeneradaW,
                                EstadoSalud = estado,
                                EsFallo = esFallo
                            });
                        }
                    }

                    nodos.Add(new DashboardNodoDto
                    {
                        Id = nodoTelemetria.NodoId,
                        Codigo = nodoTelemetria.CodigoNodo,
                        Generacion = nodoTelemetria.PotenciaGeneradaW,
                        Paneles = paneles
                    });
                }
            }

            var generacionTotal = nodos.Sum(n => n.Generacion);
            listaParques.Add(new DashboardParqueDto
            {
                Id = parqueTelemetria.ParqueSolarId,
                Nombre = parqueDominio?.Nombre ?? parqueTelemetria.NombreParque,
                Ubicacion = parqueDominio?.Ubicacion ?? string.Empty,
                GeneracionTotal = generacionTotal,
                Nodos = nodos
            });
        }

        return new DashboardTelemetriaViewDto
        {
            FechaHora = lectura.FechaHora,
            Parques = listaParques,
            TieneDatos = true,
            MensajeSinDatos = "Sin telemetría disponible"
        };
    }
}
