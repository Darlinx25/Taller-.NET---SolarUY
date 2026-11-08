using SolarUY.Simulador.Domain;
using Xunit;

namespace SolarUY.Simulador.Tests.Domain;

public class GeneradorGeneracionSolarTests
{
    private static DateTime Hora(int hora, int minuto = 0, int segundo = 0) =>
        new(2026, 11, 5, hora, minuto, segundo);

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(6, 59, 59)]
    [InlineData(7, 0, 0)]
    [InlineData(19, 0, 0)]
    [InlineData(23, 59, 59)]
    public void CalcularFactorSolar_FueraDeLaVentana_DevuelveCero(int hora, int minuto, int segundo)
    {
        var factor = GeneradorGeneracionSolar.CalcularFactorSolar(Hora(hora, minuto, segundo));

        Assert.Equal(0m, factor);
    }

    [Fact]
    public void CalcularFactorSolar_ALas13_DevuelvePico()
    {
        var factor = GeneradorGeneracionSolar.CalcularFactorSolar(Hora(13));

        Assert.Equal(1m, factor);
    }

    [Fact]
    public void CalcularFactorSolar_HorasSimetricas_DevuelveElMismoFactor()
    {
        var manana = GeneradorGeneracionSolar.CalcularFactorSolar(Hora(9));
        var tarde = GeneradorGeneracionSolar.CalcularFactorSolar(Hora(17));

        Assert.Equal(manana, tarde);
        Assert.Equal(0.5m, manana);
    }

    [Fact]
    public void CalcularFactorSolar_UsaHoraFraccionaria()
    {
        var factor1330 = GeneradorGeneracionSolar.CalcularFactorSolar(Hora(13, 30));
        var factor1500 = GeneradorGeneracionSolar.CalcularFactorSolar(Hora(15));

        Assert.True(factor1330 < 1m);
        Assert.True(factor1330 > factor1500);
    }

    [Fact]
    public void CalcularFactorSolar_RedondeaA4Decimales()
    {
        var factor = GeneradorGeneracionSolar.CalcularFactorSolar(Hora(10));

        Assert.Equal(0.7071m, factor);
    }

    [Fact]
    public void CalcularFactorSolar_TodoElDia_EstaEntreCeroYUno()
    {
        for (var minuto = 0; minuto < 24 * 60; minuto++)
        {
            var factor = GeneradorGeneracionSolar.CalcularFactorSolar(Hora(minuto / 60, minuto % 60));

            Assert.InRange(factor, 0m, 1m);
        }
    }

    [Fact]
    public void CalcularPotenciaGenerada_PanelEnFalla_DevuelveCero()
    {
        var potencia = GeneradorGeneracionSolar.CalcularPotenciaGenerada(
            400m, Hora(13), 0m, EstadoPanelSimulado.EnFalla);

        Assert.Equal(0m, potencia);
    }

    [Fact]
    public void CalcularPotenciaGenerada_FueraDeLaVentana_DevuelveCero()
    {
        var potencia = GeneradorGeneracionSolar.CalcularPotenciaGenerada(
            400m, Hora(3), 0m, EstadoPanelSimulado.Operativo);

        Assert.Equal(0m, potencia);
    }

    [Fact]
    public void CalcularPotenciaGenerada_ALas13SinVariacion_DevuelveLaPotenciaRegistrada()
    {
        var potencia = GeneradorGeneracionSolar.CalcularPotenciaGenerada(
            400m, Hora(13), 0m, EstadoPanelSimulado.Operativo);

        Assert.Equal(400m, potencia);
    }

    [Fact]
    public void CalcularPotenciaGenerada_VariacionPositivaAlPico_NoSuperaLaPotenciaRegistrada()
    {
        var potencia = GeneradorGeneracionSolar.CalcularPotenciaGenerada(
            400m, Hora(13), 0.10m, EstadoPanelSimulado.Operativo);

        Assert.Equal(400m, potencia);
    }

    [Fact]
    public void CalcularPotenciaGenerada_VariacionNegativaAlPico_ReduceLaPotencia()
    {
        var potencia = GeneradorGeneracionSolar.CalcularPotenciaGenerada(
            400m, Hora(13), -0.10m, EstadoPanelSimulado.Operativo);

        Assert.Equal(360m, potencia);
    }

    [Fact]
    public void CalcularPotenciaGenerada_RedondeaA2Decimales()
    {
        var potencia = GeneradorGeneracionSolar.CalcularPotenciaGenerada(
            333.33m, Hora(10), 0m, EstadoPanelSimulado.Operativo);

        Assert.Equal(235.70m, potencia);
        Assert.Equal(potencia, Math.Round(potencia, 2));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.5)]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1)]
    public void CalcularPotenciaGenerada_TodoElDia_NuncaEsNegativaNiSuperaLaPotenciaRegistrada(double variacion)
    {
        for (var minuto = 0; minuto < 24 * 60; minuto += 5)
        {
            var potencia = GeneradorGeneracionSolar.CalcularPotenciaGenerada(
                550.5m, Hora(minuto / 60, minuto % 60), (decimal)variacion, EstadoPanelSimulado.Operativo);

            Assert.InRange(potencia, 0m, 550.5m);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void CalcularPotenciaGenerada_PotenciaNoPositiva_LanzaExcepcion(double potenciaWp)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GeneradorGeneracionSolar.CalcularPotenciaGenerada(
                (decimal)potenciaWp, Hora(13), 0m, EstadoPanelSimulado.Operativo));
    }

    [Theory]
    [InlineData(-1.01)]
    [InlineData(1.01)]
    public void CalcularPotenciaGenerada_VariacionFueraDeRango_LanzaExcepcion(double variacion)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GeneradorGeneracionSolar.CalcularPotenciaGenerada(
                400m, Hora(13), (decimal)variacion, EstadoPanelSimulado.Operativo));
    }
}
