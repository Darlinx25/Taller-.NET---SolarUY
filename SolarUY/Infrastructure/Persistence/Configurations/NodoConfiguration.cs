using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarUY.Domain.Entities;

namespace SolarUY.Infrastructure.Persistence.Configurations;

public class NodoConfiguration : IEntityTypeConfiguration<Nodo>
{
    public void Configure(EntityTypeBuilder<Nodo> builder)
    {
        builder.ToTable("Nodos");

        builder.HasKey(nodo => nodo.Id);
        builder.Property(nodo => nodo.Id).ValueGeneratedNever();

        builder.Property(nodo => nodo.Codigo)
            .IsRequired()
            .HasMaxLength(Nodo.CodigoMaxLength);

        builder.Property(nodo => nodo.Descripcion)
            .HasMaxLength(Nodo.DescripcionMaxLength);

        builder.Property(nodo => nodo.Estado)
            .IsRequired()
            .HasMaxLength(Nodo.EstadoMaxLength);

        builder.Property(nodo => nodo.FechaCreacion)
            .IsRequired();

        builder.Property(nodo => nodo.Eliminado)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(nodo => nodo.FechaEliminacion);

        builder.Property(nodo => nodo.ParqueSolarId)
            .IsRequired();

        builder.HasQueryFilter(nodo =>
            !nodo.Eliminado
            && nodo.ParqueSolar != null
            && !nodo.ParqueSolar.Eliminado);

        builder.HasOne(nodo => nodo.ParqueSolar)
            .WithMany()
            .HasForeignKey(nodo => nodo.ParqueSolarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(nodo => nodo.Inversor)
            .WithOne(inversor => inversor.Nodo)
            .HasForeignKey<Inversor>(inversor => inversor.NodoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(nodo => new { nodo.ParqueSolarId, nodo.Codigo })
            .IsUnique()
            .HasDatabaseName("IX_Nodos_ParqueSolarId_Codigo");
    }
}
