using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarUY.Domain.Entities;
using SolarUY.Infrastructure.Persistence.Converters;

namespace SolarUY.Infrastructure.Persistence.Configurations;

public class ResumenDiarioPanelConfiguration : IEntityTypeConfiguration<ResumenDiarioPanel>
{
    public void Configure(EntityTypeBuilder<ResumenDiarioPanel> builder)
    {
        builder.ToTable("ResumenesDiariosPaneles");

        builder.HasKey(resumen => resumen.Id);
        builder.Property(resumen => resumen.Id).ValueGeneratedNever();

        builder.Property(resumen => resumen.ResumenDiarioNodoId).IsRequired();
        builder.Property(resumen => resumen.PanelId).IsRequired();

        builder.Property(resumen => resumen.CodigoSerie)
            .IsRequired()
            .HasMaxLength(ResumenDiarioPanel.CodigoSerieMaxLength);

        builder.Property(resumen => resumen.FechaLocal)
            .HasConversion<DateOnlyToDateTimeConverter>()
            .HasColumnType("date")
            .IsRequired();

        builder.Property(resumen => resumen.PotenciaAcumuladaW)
            .HasPrecision(18, 2);

        builder.Property(resumen => resumen.CantidadLecturas)
            .IsRequired();

        builder.Property(resumen => resumen.EnergiaKWh)
            .HasPrecision(18, 6);

        builder.Property(resumen => resumen.PotenciaMaximaW)
            .HasPrecision(18, 2);

        builder.Property(resumen => resumen.Fallas)
            .IsRequired();

        builder.Property(resumen => resumen.UltimoEstado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(resumen => resumen.UltimaFechaHoraUtc);

        builder.HasOne(resumen => resumen.ResumenDiarioNodo)
            .WithMany(nodo => nodo.Paneles)
            .HasForeignKey(resumen => resumen.ResumenDiarioNodoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Panel>()
            .WithMany()
            .HasForeignKey(resumen => resumen.PanelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(resumen => new { resumen.PanelId, resumen.FechaLocal })
            .IsUnique()
            .HasDatabaseName("IX_ResumenesDiariosPaneles_PanelId_FechaLocal");
    }
}
