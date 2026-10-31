using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarUY.Domain.Entities;

namespace SolarUY.Infrastructure.Persistence.Configurations;

public class InversorConfiguration : IEntityTypeConfiguration<Inversor>
{
    public void Configure(EntityTypeBuilder<Inversor> builder)
    {
        builder.ToTable("Inversores");

        builder.HasKey(inversor => inversor.Id);
        builder.Property(inversor => inversor.Id).ValueGeneratedNever();

        builder.Property(inversor => inversor.Marca)
            .IsRequired()
            .HasMaxLength(Inversor.MarcaMaxLength);

        builder.Property(inversor => inversor.Modelo)
            .IsRequired()
            .HasMaxLength(Inversor.ModeloMaxLength);

        builder.Property(inversor => inversor.NumeroSerie)
            .IsRequired()
            .HasMaxLength(Inversor.NumeroSerieMaxLength);

        builder.Property(inversor => inversor.PotenciaNominalkW)
            .HasPrecision(18, 2);

        builder.Property(inversor => inversor.FechaCreacion)
            .IsRequired();

        builder.Property(inversor => inversor.Eliminado)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(inversor => inversor.FechaEliminacion);

        builder.Property(inversor => inversor.NodoId);

        builder.HasQueryFilter(inversor =>
            !inversor.Eliminado
            && (inversor.Nodo == null
                || (!inversor.Nodo.Eliminado
                    && inversor.Nodo.ParqueSolar != null
                    && !inversor.Nodo.ParqueSolar.Eliminado)));

        builder.HasOne(inversor => inversor.Nodo)
            .WithOne(nodo => nodo.Inversor)
            .HasForeignKey<Inversor>(inversor => inversor.NodoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
