using System;
using SolarUY.Domain.ValueObjects;
using Xunit;

namespace SolarUY.Tests.Domain;

public class CalculadoraEnergiaTests
{
    private static readonly DateTime Fecha = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Calcular_SinLecturaAnterior_DevuelveCero()
    {
        var energia = CalculadoraEnergia.CalcularEnergiaKWh(1000m, Fecha, null, 15);

        Assert.Equal(0m, energia);
    }

    [Fact]
    public void Calcular_IntervaloDeCincoSegundos_IntegraLaEnergia()
    {
        var energia = CalculadoraEnergia.CalcularEnergiaKWh(
            1000m,
            Fecha.AddSeconds(5),
            Fecha,
            15);

        Assert.Equal(5m / 3600m, energia);
    }

    [Fact]
    public void Calcular_IntervaloMayorALaBrecha_UsaElTope()
    {
        var energia = CalculadoraEnergia.CalcularEnergiaKWh(
            1000m,
            Fecha.AddSeconds(60),
            Fecha,
            15);

        Assert.Equal(15m / 3600m, energia);
    }

    [Fact]
    public void Calcular_IntervaloNoPositivo_DevuelveCero()
    {
        Assert.Equal(
            0m,
            CalculadoraEnergia.CalcularEnergiaKWh(1000m, Fecha, Fecha, 15));

        Assert.Equal(
            0m,
            CalculadoraEnergia.CalcularEnergiaKWh(1000m, Fecha, Fecha.AddSeconds(1), 15));
    }

    [Fact]
    public void Calcular_PotenciaNegativa_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CalculadoraEnergia.CalcularEnergiaKWh(-1m, Fecha, null, 15));
    }

    [Fact]
    public void Calcular_FechaActualVacia_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(
            () => CalculadoraEnergia.CalcularEnergiaKWh(1000m, default, null, 15));
    }

    [Fact]
    public void Calcular_FechaAnteriorVacia_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(
            () => CalculadoraEnergia.CalcularEnergiaKWh(1000m, Fecha, (DateTime?)default(DateTime), 15));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Calcular_BrechaNoPositiva_LanzaExcepcion(int brecha)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CalculadoraEnergia.CalcularEnergiaKWh(1000m, Fecha, null, brecha));
    }
}
