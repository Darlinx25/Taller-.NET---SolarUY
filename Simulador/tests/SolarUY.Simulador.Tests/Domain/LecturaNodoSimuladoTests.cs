using SolarUY.Simulador.Domain;
using Xunit;

namespace SolarUY.Simulador.Tests.Domain;

public class LecturaNodoSimuladoTests
{
    private static readonly Guid NodoId = Guid.NewGuid();
    private static readonly DateTime FechaHoraUtc = new(2026, 11, 5, 16, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan CincoSegundos = TimeSpan.FromSeconds(5);

    private static LecturaPanelSimulado Panel(decimal potenciaW, string codigo = "PAN-001") =>
        new(Guid.NewGuid(), codigo, potenciaW, EstadoPanelSimulado.Operativo, FechaHoraUtc);

    private static LecturaNodoSimulado Crear(
        IReadOnlyList<LecturaPanelSimulado> paneles,
        decimal energiaAnteriorKWh = 0m,
        decimal? potenciaMaximaKW = null) =>
        LecturaNodoSimulado.Crear(NodoId, "N-01", paneles, energiaAnteriorKWh, potenciaMaximaKW, CincoSegundos, FechaHoraUtc);

    [Fact]
    public void Crear_LecturaNodo_SumaLaPotenciaDeSusPaneles()
    {
        var lectura = Crear(new[] { Panel(400m, "A"), Panel(300.5m, "B") });

        Assert.Equal(NodoId, lectura.NodoId);
        Assert.Equal("N-01", lectura.CodigoNodo);
        Assert.Equal(700.5m, lectura.PotenciaGeneradaW);
        Assert.Equal(2, lectura.Paneles.Count);
        Assert.Equal(FechaHoraUtc, lectura.FechaHora);
    }

    [Fact]
    public void Crear_LecturaNodo_SinPaneles_InformaCeroWattsYCeroEnergia()
    {
        var lectura = Crear(Array.Empty<LecturaPanelSimulado>());

        Assert.Equal(0m, lectura.PotenciaGeneradaW);
        Assert.Equal(0m, lectura.EnergiaAcumuladaKWh);
        Assert.Empty(lectura.Paneles);
    }

    [Fact]
    public void Crear_LecturaNodo_ConLimiteDelInversor_NoSuperaLaPotenciaNominal()
    {
        var lectura = Crear(new[] { Panel(600m, "A"), Panel(600m, "B") }, potenciaMaximaKW: 1m);

        Assert.Equal(1000m, lectura.PotenciaGeneradaW);
    }

    [Fact]
    public void Crear_LecturaNodo_ConLimiteMayorALaSuma_NoLaModifica()
    {
        var lectura = Crear(new[] { Panel(600m) }, potenciaMaximaKW: 5m);

        Assert.Equal(600m, lectura.PotenciaGeneradaW);
    }

    [Fact]
    public void Crear_LecturaNodo_SinLimiteDelInversor_NoAplicaLimite()
    {
        var lectura = Crear(new[] { Panel(600m, "A"), Panel(600m, "B") }, potenciaMaximaKW: null);

        Assert.Equal(1200m, lectura.PotenciaGeneradaW);
    }

    [Fact]
    public void Crear_LecturaNodo_ConLimiteDeCeroKW_InformaCeroWatts()
    {
        var lectura = Crear(new[] { Panel(600m) }, potenciaMaximaKW: 0m);

        Assert.Equal(0m, lectura.PotenciaGeneradaW);
    }

    [Fact]
    public void Crear_LecturaNodo_AcumulaLaEnergiaDelIntervalo()
    {
        var lectura = Crear(new[] { Panel(1000m) }, energiaAnteriorKWh: 2m);

        Assert.Equal(2m + 1m / 720m, lectura.EnergiaAcumuladaKWh, 10);
    }

    [Fact]
    public void Crear_LecturaNodo_NoRedondeaLaEnergia_ParaQueUnNodoChicoSigaAcumulando()
    {
        var energia = 0m;
        for (var tick = 1; tick <= 3; tick++)
        {
            var lectura = Crear(new[] { Panel(300m) }, energia);

            Assert.True(lectura.EnergiaAcumuladaKWh > energia);
            energia = lectura.EnergiaAcumuladaKWh;
        }

        Assert.Equal(3 * 0.3m * 5m / 3600m, energia, 10);
    }

    [Fact]
    public void Crear_LecturaNodo_ConIdVacio_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            LecturaNodoSimulado.Crear(Guid.Empty, "N-01", Array.Empty<LecturaPanelSimulado>(), 0m, null, CincoSegundos, FechaHoraUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_LecturaNodo_ConCodigoVacio_LanzaExcepcion(string? codigo)
    {
        Assert.Throws<ArgumentException>(() =>
            LecturaNodoSimulado.Crear(NodoId, codigo!, Array.Empty<LecturaPanelSimulado>(), 0m, null, CincoSegundos, FechaHoraUtc));
    }

    [Fact]
    public void Crear_LecturaNodo_ConPanelesNulos_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() =>
            LecturaNodoSimulado.Crear(NodoId, "N-01", null!, 0m, null, CincoSegundos, FechaHoraUtc));
    }

    [Fact]
    public void Crear_LecturaNodo_ConEnergiaAnteriorNegativa_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Crear(Array.Empty<LecturaPanelSimulado>(), energiaAnteriorKWh: -0.1m));
    }

    [Fact]
    public void Crear_LecturaNodo_ConPotenciaMaximaNegativa_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Crear(Array.Empty<LecturaPanelSimulado>(), potenciaMaximaKW: -1m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Crear_LecturaNodo_ConIntervaloNoPositivo_LanzaExcepcion(int segundos)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            LecturaNodoSimulado.Crear(
                NodoId, "N-01", Array.Empty<LecturaPanelSimulado>(), 0m, null, TimeSpan.FromSeconds(segundos), FechaHoraUtc));
    }

    [Fact]
    public void Crear_LecturaNodo_ConFechaHoraPorDefecto_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            LecturaNodoSimulado.Crear(NodoId, "N-01", Array.Empty<LecturaPanelSimulado>(), 0m, null, CincoSegundos, default));
    }
}
