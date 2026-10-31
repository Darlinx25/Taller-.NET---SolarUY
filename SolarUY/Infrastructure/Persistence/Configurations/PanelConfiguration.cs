using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarUY.Domain.Entities;

namespace SolarUY.Infrastructure.Persistence.Configurations;

public class PanelConfiguration : IEntityTypeConfiguration<Panel>
{
    public void Configure(EntityTypeBuilder<Panel> builder)
    {
        builder.ToTable("Paneles");

        builder.HasKey(panel => panel.Id);
        builder.Property(panel => panel.Id).ValueGeneratedNever();

        builder.Property(panel => panel.CodigoSerie)
            .IsRequired()
            .HasMaxLength(Panel.CodigoSerieMaxLength);

        builder.Property(panel => panel.Marca)
            .IsRequired()
            .HasMaxLength(Panel.MarcaMaxLength);

        builder.Property(panel => panel.Modelo)
            .IsRequired()
            .HasMaxLength(Panel.ModeloMaxLength);

        builder.Property(panel => panel.PotenciaWp)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(panel => panel.FechaInstalacion);

        builder.Property(panel => panel.Observaciones)
            .HasMaxLength(Panel.ObservacionesMaxLength);

        builder.Property(panel => panel.FechaCreacion)
            .IsRequired();

        builder.Property(panel => panel.Eliminado)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(panel => panel.FechaEliminacion);

        builder.Property(panel => panel.NodoId)
            .IsRequired();

        builder.HasQueryFilter(panel =>
            !panel.Eliminado
            && panel.Nodo != null
            && !panel.Nodo.Eliminado
            && panel.Nodo.ParqueSolar != null
            && !panel.Nodo.ParqueSolar.Eliminado);

        builder.HasOne(panel => panel.Nodo)
            .WithMany()
            .HasForeignKey(panel => panel.NodoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(panel => new { panel.NodoId, panel.CodigoSerie })
            .IsUnique()
            .HasDatabaseName("IX_Paneles_NodoId_CodigoSerie");
    }
}
