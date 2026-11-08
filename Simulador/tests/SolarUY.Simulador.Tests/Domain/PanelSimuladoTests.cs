using SolarUY.Simulador.Domain;
using Xunit;

namespace SolarUY.Simulador.Tests.Domain;

public class PanelSimuladoTests
{
    private static readonly Guid PanelId = Guid.NewGuid();
    private static readonly Guid NodoId = Guid.NewGuid();
    private static readonly DateTime HoraDelPico = new(2026, 11, 5, 13, 0, 0);
    private static readonly DateTime FechaHoraUtc = new(2026, 11, 5, 16, 0, 0, DateTimeKind.Utc);

    private static PanelSimulado CrearPanel(decimal potenciaWp = 400m) =>
        new(PanelId, NodoId, "PAN-001", potenciaWp);

    private static PanelSimulado CrearPanelEnFalla()
    {
        var panel = CrearPanel();
        panel.ActualizarEstado(1m, 0.4m, 0m);
        Assert.Equal(EstadoPanelSimulado.EnFalla, panel.Estado);
        return panel;
    }

    [Fact]
    public void Crear_PanelSimulado_ConDatosValidos_IniciaOperativo()
    {
        var panel = new PanelSimulado(PanelId, NodoId, "  PAN-001  ", 400m);

        Assert.Equal(PanelId, panel.PanelId);
        Assert.Equal(NodoId, panel.NodoId);
        Assert.Equal("PAN-001", panel.CodigoSerie);
        Assert.Equal(400m, panel.PotenciaWp);
        Assert.Equal(EstadoPanelSimulado.Operativo, panel.Estado);
    }

    [Fact]
    public void Crear_PanelSimulado_ConPanelIdVacio_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => new PanelSimulado(Guid.Empty, NodoId, "PAN-001", 400m));
    }

    [Fact]
    public void Crear_PanelSimulado_ConNodoIdVacio_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => new PanelSimulado(PanelId, Guid.Empty, "PAN-001", 400m));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_PanelSimulado_ConCodigoSerieVacio_LanzaExcepcion(string? codigoSerie)
    {
        Assert.Throws<ArgumentException>(() => new PanelSimulado(PanelId, NodoId, codigoSerie!, 400m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Crear_PanelSimulado_ConPotenciaNoPositiva_LanzaExcepcion(double potenciaWp)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PanelSimulado(PanelId, NodoId, "PAN-001", (decimal)potenciaWp));
    }

    [Fact]
    public void ActualizarEstado_PanelOperativoConValorMenorALaProbabilidadDeFalla_PasaAEnFalla()
    {
        var panel = CrearPanel();

        panel.ActualizarEstado(0.20m, 0.40m, 0.19m);

        Assert.Equal(EstadoPanelSimulado.EnFalla, panel.Estado);
    }

    [Theory]
    [InlineData(0.20, 0.20)]
    [InlineData(0.20, 0.9)]
    [InlineData(0, 0)]
    public void ActualizarEstado_PanelOperativoConValorNoMenorALaProbabilidadDeFalla_SeMantieneOperativo(
        double probabilidadFalla,
        double valorAleatorio)
    {
        var panel = CrearPanel();

        panel.ActualizarEstado((decimal)probabilidadFalla, 0.40m, (decimal)valorAleatorio);

        Assert.Equal(EstadoPanelSimulado.Operativo, panel.Estado);
    }

    [Fact]
    public void ActualizarEstado_PanelEnFallaConValorMenorALaProbabilidadDeRecuperacion_SeRecupera()
    {
        var panel = CrearPanelEnFalla();

        panel.ActualizarEstado(0.20m, 0.40m, 0.39m);

        Assert.Equal(EstadoPanelSimulado.Operativo, panel.Estado);
    }

    [Theory]
    [InlineData(0.40)]
    [InlineData(0.9)]
    public void ActualizarEstado_PanelEnFallaConValorNoMenorALaProbabilidadDeRecuperacion_SigueEnFalla(
        double valorAleatorio)
    {
        var panel = CrearPanelEnFalla();

        panel.ActualizarEstado(0.20m, 0.40m, (decimal)valorAleatorio);

        Assert.Equal(EstadoPanelSimulado.EnFalla, panel.Estado);
    }

    [Fact]
    public void ActualizarEstado_ConRecuperacionCero_ElPanelNuncaSeRecupera()
    {
        var panel = CrearPanelEnFalla();

        for (var tick = 0; tick < 50; tick++)
        {
            panel.ActualizarEstado(0m, 0m, 0m);
            Assert.Equal(EstadoPanelSimulado.EnFalla, panel.Estado);
        }
    }

    [Fact]
    public void ActualizarEstado_ConRecuperacionUno_ElPanelSeRecuperaEnLaLecturaSiguiente()
    {
        var panel = CrearPanelEnFalla();

        panel.ActualizarEstado(0m, 1m, 0.999m);

        Assert.Equal(EstadoPanelSimulado.Operativo, panel.Estado);
    }

    [Fact]
    public void ActualizarEstado_PanelQueSeRecupera_NoVuelveAFallarEnEsaMismaLectura()
    {
        var panel = CrearPanel();
        var estados = new List<EstadoPanelSimulado>();

        // Falla y recuperación seguras: el valor aleatorio 0 cumple ambas condiciones en cada tick.
        for (var tick = 0; tick < 3; tick++)
        {
            panel.ActualizarEstado(1m, 1m, 0m);
            estados.Add(panel.Estado);
        }

        Assert.Equal(
            new[]
            {
                EstadoPanelSimulado.EnFalla,
                EstadoPanelSimulado.Operativo,
                EstadoPanelSimulado.EnFalla
            },
            estados);
    }

    [Theory]
    [InlineData(-0.1, 0.4, 0.5)]
    [InlineData(1.1, 0.4, 0.5)]
    [InlineData(0.2, -0.1, 0.5)]
    [InlineData(0.2, 1.1, 0.5)]
    [InlineData(0.2, 0.4, -0.1)]
    [InlineData(0.2, 0.4, 1.1)]
    public void ActualizarEstado_ConParametrosFueraDeRango_LanzaExcepcion(
        double probabilidadFalla,
        double probabilidadRecuperacion,
        double valorAleatorio)
    {
        var panel = CrearPanel();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            panel.ActualizarEstado(
                (decimal)probabilidadFalla,
                (decimal)probabilidadRecuperacion,
                (decimal)valorAleatorio));
    }

    [Fact]
    public void ForzarFalla_PanelOperativo_QuedaEnFalla()
    {
        var panel = CrearPanel();

        panel.ForzarFalla();

        Assert.Equal(EstadoPanelSimulado.EnFalla, panel.Estado);
    }

    [Fact]
    public void ForzarFalla_PanelYaEnFalla_SigueEnFalla()
    {
        var panel = CrearPanelEnFalla();

        panel.ForzarFalla();

        Assert.Equal(EstadoPanelSimulado.EnFalla, panel.Estado);
    }

    [Fact]
    public void ForzarFalla_ElPanelSeRecuperaConLaMismaProbabilidadQueUnaFallaAleatoria()
    {
        var sigueEnFalla = CrearPanel();
        sigueEnFalla.ForzarFalla();
        sigueEnFalla.ActualizarEstado(0m, 0.40m, 0.9m);

        var seRecupera = CrearPanel();
        seRecupera.ForzarFalla();
        seRecupera.ActualizarEstado(0m, 0.40m, 0.3m);

        Assert.Equal(EstadoPanelSimulado.EnFalla, sigueEnFalla.Estado);
        Assert.Equal(EstadoPanelSimulado.Operativo, seRecupera.Estado);
    }

    [Fact]
    public void ForzarFalla_PanelAlPico_InformaCeroWatts()
    {
        var panel = CrearPanel(400m);
        panel.ForzarFalla();

        var lectura = panel.GenerarLectura(HoraDelPico, 0m, FechaHoraUtc);

        Assert.Equal(0m, lectura.PotenciaGeneradaW);
        Assert.Equal(EstadoPanelSimulado.EnFalla, lectura.Estado);
    }

    [Fact]
    public void GenerarLectura_PanelOperativoAlPico_InformaLaPotenciaRegistrada()
    {
        var panel = CrearPanel(400m);

        var lectura = panel.GenerarLectura(HoraDelPico, 0m, FechaHoraUtc);

        Assert.Equal(PanelId, lectura.PanelId);
        Assert.Equal("PAN-001", lectura.CodigoSerie);
        Assert.Equal(400m, lectura.PotenciaGeneradaW);
        Assert.True(lectura.EstaOperativo);
        Assert.Equal(FechaHoraUtc, lectura.FechaHora);
    }

    [Fact]
    public void GenerarLectura_PanelEnFalla_InformaCeroWatts()
    {
        var panel = CrearPanel(400m);
        panel.ActualizarEstado(1m, 0.4m, 0m);

        var lectura = panel.GenerarLectura(HoraDelPico, 0m, FechaHoraUtc);

        Assert.Equal(0m, lectura.PotenciaGeneradaW);
        Assert.Equal(EstadoPanelSimulado.EnFalla, lectura.Estado);
        Assert.False(lectura.EstaOperativo);
    }
}
