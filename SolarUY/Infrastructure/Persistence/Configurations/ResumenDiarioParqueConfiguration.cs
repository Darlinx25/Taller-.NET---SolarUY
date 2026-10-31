using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarUY.Domain.Entities;
using SolarUY.Infrastructure.Persistence.Converters;

namespace SolarUY.Infrastructure.Persistence.Configurations;

public class ResumenDiarioParqueConfiguration : IEntityTypeConfiguration<ResumenDiarioParque>
{
    public void Configure(EntityTypeBuilder<ResumenDiarioParque> builder)
    {
        builder.ToTable("ResumenesDiariosParques");

        builder.HasKey(resumen => resumen.Id);
        builder.Property(resumen => resumen.Id).ValueGeneratedNever();

        builder.Property(resumen => resumen.ParqueSolarId).IsRequired();

        builder.Property(resumen => resumen.NombreParque)
            .IsRequired()
            .HasMaxLength(ResumenDiarioParque.NombreParqueMaxLength);

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

        builder.HasOne<ParqueSolar>()
            .WithMany()
            .HasForeignKey(resumen => resumen.ParqueSolarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(resumen => new { resumen.ParqueSolarId, resumen.FechaLocal })
            .IsUnique()
            .HasDatabaseName("IX_ResumenesDiariosParques_ParqueSolarId_FechaLocal");
    }
}
