using Microsoft.EntityFrameworkCore;
using SolarUY.Domain.Entities;

namespace SolarUY.Infrastructure.Persistence;

/// <summary>
/// Contexto de datos de SolarUY.
/// </summary>
public class SolarUYDbContext : DbContext
{
    /// <summary>
    /// Crea el contexto de datos.
    /// </summary>
    /// <param name="options">Opciones de configuración del contexto.</param>
    public SolarUYDbContext(DbContextOptions<SolarUYDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Parque solares registrados en el sistema.
    /// </summary>
    public DbSet<ParqueSolar> ParquesSolares => Set<ParqueSolar>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SolarUYDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
