using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarUY.Domain.Entities;

namespace SolarUY.Infrastructure.Persistence.Configurations;

public class TareaMantenimientoConfiguration : IEntityTypeConfiguration<TareaMantenimiento>
{
    public void Configure(EntityTypeBuilder<TareaMantenimiento> builder)
    {
        builder.ToTable("TareasMantenimiento");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.PanelId)
            .IsRequired();

        builder.Property(t => t.TipoTarea)
            .IsRequired()
            .HasMaxLength(TareaMantenimiento.TipoTareaMaxLength);

        builder.Property(t => t.Descripcion)
            .IsRequired()
            .HasMaxLength(TareaMantenimiento.DescripcionMaxLength);

        builder.Property(t => t.AsignadoA)
            .IsRequired()
            .HasMaxLength(TareaMantenimiento.AsignadoAMaxLength);

        builder.Property(t => t.FechaCreacion)
            .IsRequired();

        builder.Property(t => t.FechaAsignacion)
            .IsRequired();

        builder.Property(t => t.FechaVencimiento)
            .IsRequired();

        builder.Property(t => t.FechaInicio)
            .IsRequired();

        builder.Property(t => t.FechaFinalizacion)
            .IsRequired();

        builder.Property(t => t.Estado)
            .IsRequired()
            .HasMaxLength(TareaMantenimiento.EstadoMaxLength);

        builder.Property(t => t.QuienAsigna)
            .IsRequired()
            .HasMaxLength(TareaMantenimiento.QuienAsignaMaxLength);

        builder.Property(t => t.QuienCompleta)
            .HasMaxLength(TareaMantenimiento.QuienCompletaMaxLength);

        builder.Property(t => t.Observaciones)
            .HasMaxLength(TareaMantenimiento.ObservacionesMaxLength);

        builder.HasOne(t => t.Panel)
            .WithMany()
            .HasForeignKey(t => t.PanelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.PanelId);
    }
}
