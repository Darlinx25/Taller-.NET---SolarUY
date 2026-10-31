using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Enums;
using SolarUY.Domain.Repositories;
using SolarUY.Domain.ValueObjects;
using SolarUY.Infrastructure.Persistence;

namespace SolarUY.Infrastructure.Repositories;

public class HistoricoRepository : IHistoricoRepository
{
    private readonly SolarUYDbContext _contexto;

    public HistoricoRepository(SolarUYDbContext contexto)
    {
        _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
    }

    public async Task<IReadOnlyList<ResumenDiarioParque>> ObtenerPorFechaAsync(
        DateOnly fechaLocal,
        CancellationToken cancellationToken = default)
    {
        return await _contexto.ResumenesDiariosParques
            .Include(parque => parque.Nodos)
                .ThenInclude(nodo => nodo.Paneles)
            .Where(parque => parque.FechaLocal == fechaLocal)
            .OrderBy(parque => parque.NombreParque)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ResumenDiarioGeneracion>> ObtenerResumenesAsync(
        FiltroHistorico filtro,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        return filtro.TipoEntidad switch
        {
            TipoEntidadHistorico.Parque => await ObtenerParquesAsync(filtro, cancellationToken)
                .ConfigureAwait(false),
            TipoEntidadHistorico.Nodo => await ObtenerNodosAsync(filtro, cancellationToken)
                .ConfigureAwait(false),
            TipoEntidadHistorico.Panel => await ObtenerPanelesAsync(filtro, cancellationToken)
                .ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(filtro), "El tipo de entidad no es válido.")
        };
    }

    public async Task AgregarAsync(
        ResumenDiarioParque resumen,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resumen);
        await _contexto.ResumenesDiariosParques
            .AddAsync(resumen, cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _contexto.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ResumenDiarioGeneracion>> ObtenerParquesAsync(
        FiltroHistorico filtro,
        CancellationToken cancellationToken)
    {
        var consulta = _contexto.ResumenesDiariosParques
            .AsNoTracking()
            .Where(resumen => resumen.FechaLocal >= filtro.Desde && resumen.FechaLocal <= filtro.Hasta);

        if (filtro.EntidadId.HasValue)
            consulta = consulta.Where(resumen => resumen.ParqueSolarId == filtro.EntidadId.Value);

        var datos = await consulta
            .OrderBy(resumen => resumen.FechaLocal)
            .Select(resumen => new
            {
                resumen.ParqueSolarId,
                resumen.NombreParque,
                resumen.FechaLocal,
                resumen.PotenciaAcumuladaW,
                resumen.CantidadLecturas,
                resumen.EnergiaKWh,
                resumen.PotenciaMaximaW,
                resumen.Fallas,
                resumen.UltimoEstado
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return datos
            .Select(dato => new ResumenDiarioGeneracion(
                dato.ParqueSolarId,
                dato.NombreParque,
                dato.FechaLocal,
                dato.PotenciaAcumuladaW,
                dato.CantidadLecturas,
                dato.EnergiaKWh,
                dato.PotenciaMaximaW,
                dato.Fallas,
                dato.UltimoEstado))
            .ToList();
    }

    private async Task<IReadOnlyList<ResumenDiarioGeneracion>> ObtenerNodosAsync(
        FiltroHistorico filtro,
        CancellationToken cancellationToken)
    {
        var consulta = _contexto.ResumenesDiariosNodos
            .AsNoTracking()
            .Where(resumen => resumen.FechaLocal >= filtro.Desde && resumen.FechaLocal <= filtro.Hasta);

        if (filtro.EntidadId.HasValue)
            consulta = consulta.Where(resumen => resumen.NodoId == filtro.EntidadId.Value);

        var datos = await consulta
            .OrderBy(resumen => resumen.FechaLocal)
            .Select(resumen => new
            {
                resumen.NodoId,
                resumen.CodigoNodo,
                resumen.FechaLocal,
                resumen.PotenciaAcumuladaW,
                resumen.CantidadLecturas,
                resumen.EnergiaKWh,
                resumen.PotenciaMaximaW,
                resumen.Fallas,
                resumen.UltimoEstado
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return datos
            .Select(dato => new ResumenDiarioGeneracion(
                dato.NodoId,
                dato.CodigoNodo,
                dato.FechaLocal,
                dato.PotenciaAcumuladaW,
                dato.CantidadLecturas,
                dato.EnergiaKWh,
                dato.PotenciaMaximaW,
                dato.Fallas,
                dato.UltimoEstado))
            .ToList();
    }

    private async Task<IReadOnlyList<ResumenDiarioGeneracion>> ObtenerPanelesAsync(
        FiltroHistorico filtro,
        CancellationToken cancellationToken)
    {
        var consulta = _contexto.ResumenesDiariosPaneles
            .AsNoTracking()
            .Where(resumen => resumen.FechaLocal >= filtro.Desde && resumen.FechaLocal <= filtro.Hasta);

        if (filtro.EntidadId.HasValue)
            consulta = consulta.Where(resumen => resumen.PanelId == filtro.EntidadId.Value);

        var datos = await consulta
            .OrderBy(resumen => resumen.FechaLocal)
            .Select(resumen => new
            {
                resumen.PanelId,
                resumen.CodigoSerie,
                resumen.FechaLocal,
                resumen.PotenciaAcumuladaW,
                resumen.CantidadLecturas,
                resumen.EnergiaKWh,
                resumen.PotenciaMaximaW,
                resumen.Fallas,
                resumen.UltimoEstado
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return datos
            .Select(dato => new ResumenDiarioGeneracion(
                dato.PanelId,
                dato.CodigoSerie,
                dato.FechaLocal,
                dato.PotenciaAcumuladaW,
                dato.CantidadLecturas,
                dato.EnergiaKWh,
                dato.PotenciaMaximaW,
                dato.Fallas,
                dato.UltimoEstado))
            .ToList();
    }
}
