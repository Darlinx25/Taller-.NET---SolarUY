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

/// <summary>
/// Implementación del repositorio de parques solares sobre Entity Framework Core.
/// </summary>
public class ParqueSolarRepository : IParqueSolarRepository
{
    private readonly SolarUYDbContext _contexto;

    /// <summary>
    /// Crea el repositorio de parques solares.
    /// </summary>
    /// <param name="contexto">Contexto de datos de la aplicación.</param>
    public ParqueSolarRepository(SolarUYDbContext contexto)
    {
        _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
    }

    /// <inheritdoc />
    public Task<bool> ExisteNombreAsync(
        string nombre,
        Guid? parqueIdExcluido = null,
        CancellationToken cancellationToken = default)
    {
        return _contexto.ParquesSolares
            .IgnoreQueryFilters()
            .AnyAsync(
                parque => parque.Nombre == nombre
                    && (parqueIdExcluido == null || parque.Id != parqueIdExcluido),
                cancellationToken);
    }

    /// <inheritdoc />
    public Task<ParqueSolar?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _contexto.ParquesSolares
            .FirstOrDefaultAsync(parque => parque.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public void Eliminar(ParqueSolar parque)
    {
        ArgumentNullException.ThrowIfNull(parque);

        parque.Eliminar();
    }

    /// <inheritdoc />
    public async Task AgregarAsync(ParqueSolar parque, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parque);

        await _contexto.ParquesSolares.AddAsync(parque, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ParqueSolar>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _contexto.ParquesSolares
            .AsNoTracking()
            .OrderBy(parque => parque.Nombre)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _contexto.SaveChangesAsync(cancellationToken);
    }
}
