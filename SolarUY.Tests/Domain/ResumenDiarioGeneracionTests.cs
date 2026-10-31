using System;
using SolarUY.Domain.Enums;
using SolarUY.Domain.ValueObjects;
using Xunit;

namespace SolarUY.Tests.Domain;

public class ResumenDiarioGeneracionTests
{
    private static readonly Guid EntidadId = Guid.NewGuid();
    private static readonly DateOnly Fecha = new(2026, 1, 15);

    [Fact]
    public void PotenciaPromedio_SeCalculaConLasLecturas()
    {
        var resumen = new ResumenDiarioGeneracion(
            EntidadId,
            "Parque Norte",
            Fecha,
            1000m,
            2,
            0.5m,
            700m,
            1,
            EstadoPanel.EnFalla);

        Assert.Equal(500m, resumen.PotenciaPromedioW);
        Assert.Equal("Parque Norte", resumen.NombreEntidad);
        Assert.Equal(EstadoPanel.EnFalla, resumen.UltimoEstado);
    }

    [Fact]
    public void PotenciaPromedio_SinLecturas_EsCero()
    {
        var resumen = new ResumenDiarioGeneracion(
            EntidadId,
            "Parque Norte",
            Fecha,
            0m,
            0,
            0m,
            0m,
            0,
            EstadoPanel.Operativo);

        Assert.Equal(0m, resumen.PotenciaPromedioW);
    }

    [Fact]
    public void Constructor_DatosInvalidos_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(
            () => new ResumenDiarioGeneracion(
                Guid.Empty,
                "Parque",
                Fecha,
                0m,
                0,
                0m,
                0m,
                0,
                EstadoPanel.Operativo));

        Assert.Throws<ArgumentException>(
            () => new ResumenDiarioGeneracion(
                EntidadId,
                " ",
                Fecha,
                0m,
                0,
                0m,
                0m,
                0,
                EstadoPanel.Operativo));

        Assert.Throws<ArgumentException>(
            () => new ResumenDiarioGeneracion(
                EntidadId,
                "Parque",
                default,
                0m,
                0,
                0m,
                0m,
                0,
                EstadoPanel.Operativo));
    }

    [Fact]
    public void Constructor_ValoresNegativos_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ResumenDiarioGeneracion(
                EntidadId,
                "Parque",
                Fecha,
                -1m,
                1,
                0m,
                0m,
                0,
                EstadoPanel.Operativo));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ResumenDiarioGeneracion(
                EntidadId,
                "Parque",
                Fecha,
                100m,
                1,
                0m,
                101m,
                0,
                EstadoPanel.Operativo));
    }
}
