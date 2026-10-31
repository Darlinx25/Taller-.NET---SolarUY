using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Repositories;
using SolarUY.Infrastructure.Persistence;

namespace SolarUY.Infrastructure.Repositories;

public class PanelRepository : IPanelRepository
{
    private readonly SolarUYDbContext _contexto;

    public PanelRepository(SolarUYDbContext contexto)
    {
        _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
    }

    public async Task AgregarAsync(Panel panel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(panel);
        await _contexto.Paneles.AddAsync(panel, cancellationToken).ConfigureAwait(false);
    }

    public Task<Panel?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _contexto.Paneles
            .Include(panel => panel.Nodo)
                .ThenInclude(nodo => nodo!.ParqueSolar)
            .FirstOrDefaultAsync(panel => panel.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Panel>> ObtenerPorNodoAsync(
        Guid nodoId,
        CancellationToken cancellationToken = default)
    {
        return await _contexto.Paneles
            .AsNoTracking()
            .Where(panel => panel.NodoId == nodoId)
            .OrderBy(panel => panel.CodigoSerie)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<bool> ExisteCodigoAsync(
        string codigo,
        Guid nodoId,
        Guid? panelIdExcluido = null,
        CancellationToken cancellationToken = default)
    {
        return _contexto.Paneles
            .IgnoreQueryFilters()
            .AnyAsync(
                panel => panel.NodoId == nodoId
                    && panel.CodigoSerie == codigo
                    && (panelIdExcluido == null || panel.Id != panelIdExcluido),
                cancellationToken);
    }

    public void Eliminar(Panel panel)
    {
        ArgumentNullException.ThrowIfNull(panel);
        panel.Eliminar();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _contexto.SaveChangesAsync(cancellationToken);
    }
}
