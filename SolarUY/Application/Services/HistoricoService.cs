using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SolarUY.Application.Dtos;
using SolarUY.Domain.Enums;
using SolarUY.Domain.Repositories;
using SolarUY.Domain.ValueObjects;

namespace SolarUY.Application.Services;

/// <summary>
/// Casos de uso de consulta de los históricos de generación.
/// </summary>
public class HistoricoService : IHistoricoService
{
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IParqueSolarRepository _parqueSolarRepository;
    private readonly INodoRepository _nodoRepository;
    private readonly IPanelRepository _panelRepository;
    private readonly ConfiguracionHistorico _configuracion;
    private readonly TimeProvider _timeProvider;

    public HistoricoService(
        IHistoricoRepository historicoRepository,
        IParqueSolarRepository parqueSolarRepository,
        INodoRepository nodoRepository,
        IPanelRepository panelRepository,
        IOptions<ConfiguracionHistorico> opciones,
        TimeProvider timeProvider)
    {
        _historicoRepository = historicoRepository
            ?? throw new ArgumentNullException(nameof(historicoRepository));
        _parqueSolarRepository = parqueSolarRepository
            ?? throw new ArgumentNullException(nameof(parqueSolarRepository));
        _nodoRepository = nodoRepository
            ?? throw new ArgumentNullException(nameof(nodoRepository));
        _panelRepository = panelRepository
            ?? throw new ArgumentNullException(nameof(panelRepository));
        ArgumentNullException.ThrowIfNull(opciones);
        _configuracion = opciones.Value;
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public FiltroHistoricoDto CrearFiltroPredeterminado()
    {
        var hoy = ObtenerFechaLocalActual();

        return new FiltroHistoricoDto
        {
            TipoEntidad = TipoEntidadHistorico.Parque,
            EntidadId = null,
            Desde = hoy.AddDays(-29),
            Hasta = hoy,
            Agrupacion = AgrupacionHistorico.Dia
        };
    }

    public async Task<OpcionesHistoricoDto> ObtenerOpcionesAsync(
        CancellationToken cancellationToken = default)
    {
        var parques = await _parqueSolarRepository
            .ObtenerTodosAsync(cancellationToken)
            .ConfigureAwait(false);

        var opcionesParques = new List<OpcionEntidadDto>(parques.Count);
        var opcionesNodos = new List<OpcionEntidadDto>();
        var opcionesPaneles = new List<OpcionEntidadDto>();

        foreach (var parque in parques)
        {
            opcionesParques.Add(new OpcionEntidadDto
            {
                Id = parque.Id,
                Nombre = parque.Nombre
            });

            var nodos = await _nodoRepository
                .ObtenerPorParqueAsync(parque.Id, cancellationToken)
                .ConfigureAwait(false);

            foreach (var nodo in nodos)
            {
                opcionesNodos.Add(new OpcionEntidadDto
                {
                    Id = nodo.Id,
                    Nombre = $"{parque.Nombre} / {nodo.Codigo}"
                });

                var paneles = await _panelRepository
                    .ObtenerPorNodoAsync(nodo.Id, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var panel in paneles)
                {
                    opcionesPaneles.Add(new OpcionEntidadDto
                    {
                        Id = panel.Id,
                        Nombre = $"{parque.Nombre} / {nodo.Codigo} / {panel.CodigoSerie}"
                    });
                }
            }
        }

        return new OpcionesHistoricoDto
        {
            Parques = opcionesParques,
            Nodos = opcionesNodos,
            Paneles = opcionesPaneles
        };
    }

    public async Task<HistoricoGeneracionDto> ConsultarAsync(
        FiltroHistoricoDto filtro,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var filtroValidado = FiltroHistorico.Crear(
            filtro.TipoEntidad,
            filtro.EntidadId,
            filtro.Desde ?? default,
            filtro.Hasta ?? default,
            filtro.Agrupacion);

        var resumenes = await _historicoRepository
            .ObtenerResumenesAsync(filtroValidado, cancellationToken)
            .ConfigureAwait(false);

        if (resumenes.Count == 0)
            return new HistoricoGeneracionDto();

        var filas = ObtenerFilas(resumenes, filtro.Agrupacion);
        var puntos = ObtenerPuntos(resumenes, filtro.Agrupacion);
        var totalLecturas = resumenes.Sum(resumen => resumen.CantidadLecturas);
        var potenciaTotal = resumenes.Sum(resumen => resumen.PotenciaAcumuladaW);

        return new HistoricoGeneracionDto
        {
            Resumenes = filas,
            PuntosGrafico = puntos,
            EnergiaTotalKWh = resumenes.Sum(resumen => resumen.EnergiaKWh),
            PotenciaPromedioW = totalLecturas == 0 ? 0m : potenciaTotal / totalLecturas,
            PotenciaMaximaW = resumenes.Max(resumen => resumen.PotenciaMaximaW),
            Fallas = resumenes.Sum(resumen => resumen.Fallas)
        };
    }

    private DateOnly ObtenerFechaLocalActual()
    {
        var fechaHoraUtc = _timeProvider.GetUtcNow().UtcDateTime;
        return DateOnly.FromDateTime(fechaHoraUtc.AddHours(_configuracion.DesfaseHorarioUruguayHoras));
    }

    private static List<ResumenHistoricoDto> ObtenerFilas(
        IReadOnlyList<ResumenDiarioGeneracion> resumenes,
        AgrupacionHistorico agrupacion)
    {
        var filas = new List<ResumenHistoricoDto>();

        foreach (var grupoEntidad in resumenes.GroupBy(resumen => resumen.EntidadId))
        {
            foreach (var grupoPeriodo in grupoEntidad.GroupBy(resumen => ObtenerPeriodo(resumen.FechaLocal, agrupacion)))
            {
                var lecturas = grupoPeriodo.Sum(resumen => resumen.CantidadLecturas);
                var potenciaTotal = grupoPeriodo.Sum(resumen => resumen.PotenciaAcumuladaW);
                var ultimo = grupoPeriodo.OrderByDescending(resumen => resumen.FechaLocal).First();

                filas.Add(new ResumenHistoricoDto
                {
                    EntidadId = ultimo.EntidadId,
                    EntidadNombre = ultimo.NombreEntidad,
                    Periodo = grupoPeriodo.Key,
                    EnergiaKWh = grupoPeriodo.Sum(resumen => resumen.EnergiaKWh),
                    PotenciaPromedioW = lecturas == 0 ? 0m : potenciaTotal / lecturas,
                    PotenciaMaximaW = grupoPeriodo.Max(resumen => resumen.PotenciaMaximaW),
                    Fallas = grupoPeriodo.Sum(resumen => resumen.Fallas),
                    UltimoEstado = ultimo.UltimoEstado
                });
            }
        }

        return filas
            .OrderBy(fila => fila.Periodo, StringComparer.Ordinal)
            .ThenBy(fila => fila.EntidadNombre, StringComparer.Ordinal)
            .ToList();
    }

    private static List<PuntoGraficoHistoricoDto> ObtenerPuntos(
        IReadOnlyList<ResumenDiarioGeneracion> resumenes,
        AgrupacionHistorico agrupacion)
    {
        return resumenes
            .GroupBy(resumen => ObtenerPeriodo(resumen.FechaLocal, agrupacion))
            .Select(grupo => new PuntoGraficoHistoricoDto
            {
                Periodo = grupo.Key,
                EnergiaKWh = grupo.Sum(resumen => resumen.EnergiaKWh)
            })
            .OrderBy(punto => punto.Periodo, StringComparer.Ordinal)
            .ToList();
    }

    private static string ObtenerPeriodo(DateOnly fecha, AgrupacionHistorico agrupacion)
    {
        return agrupacion switch
        {
            AgrupacionHistorico.Dia => fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            AgrupacionHistorico.Mes => fecha.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            AgrupacionHistorico.Anio => fecha.ToString("yyyy", CultureInfo.InvariantCulture),
            _ => throw new ArgumentOutOfRangeException(nameof(agrupacion), "La agrupación no es válida.")
        };
    }
}
