using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarUY.Domain.Entities;

namespace SolarUY.Infrastructure.Persistence.Configurations;

public class AlertaPanelConfiguration : IEntityTypeConfiguration<AlertaPanel>
{
    public void Configure(EntityTypeBuilder<AlertaPanel> builder)
    {
        builder.ToTable("AlertasPanel");

        builder.HasKey(alerta => alerta.Id);
        builder.Property(alerta => alerta.Id).ValueGeneratedNever();

        builder.Property(alerta => alerta.ParqueSolarId).IsRequired();
        builder.Property(alerta => alerta.NodoId).IsRequired();
        builder.Property(alerta => alerta.PanelId).IsRequired();

        builder.Property(alerta => alerta.NombreParque)
            .IsRequired()
            .HasMaxLength(AlertaPanel.NombreParqueMaxLength);

        builder.Property(alerta => alerta.CodigoNodo)
            .IsRequired()
            .HasMaxLength(AlertaPanel.CodigoNodoMaxLength);

        builder.Property(alerta => alerta.CodigoSerie)
            .IsRequired()
            .HasMaxLength(AlertaPanel.CodigoSerieMaxLength);

        builder.Property(alerta => alerta.FechaInicioUtc).IsRequired();
        builder.Property(alerta => alerta.FechaFinUtc);

        builder.HasOne<ParqueSolar>()
            .WithMany()
            .HasForeignKey(alerta => alerta.ParqueSolarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Nodo>()
            .WithMany()
            .HasForeignKey(alerta => alerta.NodoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Panel>()
            .WithMany()
            .HasForeignKey(alerta => alerta.PanelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(alerta => new { alerta.PanelId, alerta.FechaInicioUtc })
            .HasDatabaseName("IX_AlertasPanel_PanelId_FechaInicioUtc");

        builder.HasIndex(alerta => alerta.FechaFinUtc)
            .HasDatabaseName("IX_AlertasPanel_FechaFinUtc");
    }
}
