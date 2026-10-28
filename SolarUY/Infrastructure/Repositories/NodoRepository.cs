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

public class NodoRepository : INodoRepository
{
    private readonly SolarUYDbContext _contexto;

    public NodoRepository(SolarUYDbContext contexto)
    {
        _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
    }

    public async Task AgregarAsync(Nodo nodo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nodo);
        await _contexto.Nodos.AddAsync(nodo, cancellationToken).ConfigureAwait(false);
    }

    public Task<Nodo?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _contexto.Nodos
            .Include(nodo => nodo.Inversor)
            .Include(nodo => nodo.ParqueSolar)
            .FirstOrDefaultAsync(nodo => nodo.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Nodo>> ObtenerPorParqueAsync(Guid parqueSolarId, CancellationToken cancellationToken = default)
    {
        return await _contexto.Nodos
            .Include(nodo => nodo.Inversor)
            .Where(nodo => nodo.ParqueSolarId == parqueSolarId)
            .OrderBy(nodo => nodo.Codigo)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<bool> ExisteCodigoAsync(string codigo, Guid parqueSolarId, Guid? nodoIdExcluido = null, CancellationToken cancellationToken = default)
    {
        return _contexto.Nodos
            .AnyAsync(
                nodo => nodo.ParqueSolarId == parqueSolarId
                    && nodo.Codigo == codigo
                    && (nodoIdExcluido == null || nodo.Id != nodoIdExcluido),
                cancellationToken);
    }

    public void Eliminar(Nodo nodo)
    {
        ArgumentNullException.ThrowIfNull(nodo);
        _contexto.Nodos.Remove(nodo);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _contexto.SaveChangesAsync(cancellationToken);
    }
}
