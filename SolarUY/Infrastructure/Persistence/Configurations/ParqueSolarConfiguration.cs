using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarUY.Domain.Entities;

namespace SolarUY.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración del mapeo de la entidad <see cref="ParqueSolar"/>.
/// </summary>
public class ParqueSolarConfiguration : IEntityTypeConfiguration<ParqueSolar>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ParqueSolar> builder)
    {
        builder.ToTable("ParquesSolares");

        builder.HasKey(parque => parque.Id);
        builder.Property(parque => parque.Id).ValueGeneratedNever();

        builder.Property(parque => parque.Nombre)
            .IsRequired()
            .HasMaxLength(ParqueSolar.NombreMaxLength);

        builder.Property(parque => parque.Ubicacion)
            .IsRequired()
            .HasMaxLength(ParqueSolar.UbicacionMaxLength);

        builder.Property(parque => parque.CapacidadEstimadaMW)
            .HasPrecision(18, 2);

        builder.Property(parque => parque.FechaCreacion)
            .IsRequired();

        builder.HasIndex(parque => parque.Nombre)
            .IsUnique()
            .HasDatabaseName("IX_ParquesSolares_Nombre");
    }
}
