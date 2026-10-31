using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolarUY.Domain.Entities;
using SolarUY.Infrastructure.Persistence.Converters;

namespace SolarUY.Infrastructure.Persistence.Configurations;

public class ResumenDiarioNodoConfiguration : IEntityTypeConfiguration<ResumenDiarioNodo>
{
    public void Configure(EntityTypeBuilder<ResumenDiarioNodo> builder)
    {
        builder.ToTable("ResumenesDiariosNodos");

        builder.HasKey(resumen => resumen.Id);
        builder.Property(resumen => resumen.Id).ValueGeneratedNever();

        builder.Property(resumen => resumen.ResumenDiarioParqueId).IsRequired();
        builder.Property(resumen => resumen.NodoId).IsRequired();

        builder.Property(resumen => resumen.CodigoNodo)
            .IsRequired()
            .HasMaxLength(ResumenDiarioNodo.CodigoNodoMaxLength);

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

        builder.HasOne(resumen => resumen.ResumenDiarioParque)
            .WithMany(parque => parque.Nodos)
            .HasForeignKey(resumen => resumen.ResumenDiarioParqueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Nodo>()
            .WithMany()
            .HasForeignKey(resumen => resumen.NodoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(resumen => new { resumen.NodoId, resumen.FechaLocal })
            .IsUnique()
            .HasDatabaseName("IX_ResumenesDiariosNodos_NodoId_FechaLocal");
    }
}
