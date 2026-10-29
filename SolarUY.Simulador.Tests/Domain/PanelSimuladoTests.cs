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

    [Fact]
    public void Crear_PanelSimulado_ConDatosValidos_IniciaOperativoSinTicksDeFalla()
    {
        var panel = new PanelSimulado(PanelId, NodoId, "  PAN-001  ", 400m);

        Assert.Equal(PanelId, panel.PanelId);
        Assert.Equal(NodoId, panel.NodoId);
        Assert.Equal("PAN-001", panel.CodigoSerie);
        Assert.Equal(400m, panel.PotenciaWp);
        Assert.Equal(EstadoPanelSimulado.Operativo, panel.Estado);
        Assert.Equal(0, panel.TicksRestantesFalla);
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
    public void ActualizarEstado_ConValorMenorALaProbabilidad_PasaAEnFallaConLaDuracionIndicada()
    {
        var panel = CrearPanel();

        panel.ActualizarEstado(0.05m, 6, 0.01m);

        Assert.Equal(EstadoPanelSimulado.EnFalla, panel.Estado);
        Assert.Equal(6, panel.TicksRestantesFalla);
    }

    [Theory]
    [InlineData(0.05, 0.05)]
    [InlineData(0.05, 0.9)]
    [InlineData(0, 0)]
    public void ActualizarEstado_ConValorNoMenorALaProbabilidad_SeMantieneOperativo(double probabilidad, double valorAleatorio)
    {
        var panel = CrearPanel();

        panel.ActualizarEstado((decimal)probabilidad, 6, (decimal)valorAleatorio);

        Assert.Equal(EstadoPanelSimulado.Operativo, panel.Estado);
        Assert.Equal(0, panel.TicksRestantesFalla);
    }

    [Fact]
    public void ActualizarEstado_ConDuracionN_InformaNLecturasEnFallaYLuegoSeRecupera()
    {
        var panel = CrearPanel();
        var estados = new List<EstadoPanelSimulado>();

        panel.ActualizarEstado(0.5m, 3, 0m);
        estados.Add(panel.Estado);
        for (var tick = 0; tick < 3; tick++)
        {
            panel.ActualizarEstado(0.5m, 3, 1m);
            estados.Add(panel.Estado);
        }

        Assert.Equal(
            new[]
            {
                EstadoPanelSimulado.EnFalla,
                EstadoPanelSimulado.EnFalla,
                EstadoPanelSimulado.EnFalla,
                EstadoPanelSimulado.Operativo
            },
            estados);
        Assert.Equal(0, panel.TicksRestantesFalla);
    }

    [Fact]
    public void ActualizarEstado_PanelQueSeRecupera_NoVuelveAFallarEnEsaMismaLectura()
    {
        var panel = CrearPanel();
        var estados = new List<EstadoPanelSimulado>();

        for (var tick = 0; tick < 4; tick++)
        {
            panel.ActualizarEstado(1m, 2, 0m);
            estados.Add(panel.Estado);
        }

        Assert.Equal(
            new[]
            {
                EstadoPanelSimulado.EnFalla,
                EstadoPanelSimulado.EnFalla,
                EstadoPanelSimulado.Operativo,
                EstadoPanelSimulado.EnFalla
            },
            estados);
    }

    [Theory]
    [InlineData(-0.1, 6, 0.5)]
    [InlineData(1.1, 6, 0.5)]
    [InlineData(0.5, 0, 0.5)]
    [InlineData(0.5, -1, 0.5)]
    [InlineData(0.5, 6, -0.1)]
    [InlineData(0.5, 6, 1.1)]
    public void ActualizarEstado_ConParametrosFueraDeRango_LanzaExcepcion(
        double probabilidad,
        int duracionTicks,
        double valorAleatorio)
    {
        var panel = CrearPanel();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            panel.ActualizarEstado((decimal)probabilidad, duracionTicks, (decimal)valorAleatorio));
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
        panel.ActualizarEstado(1m, 6, 0m);

        var lectura = panel.GenerarLectura(HoraDelPico, 0m, FechaHoraUtc);

        Assert.Equal(0m, lectura.PotenciaGeneradaW);
        Assert.Equal(EstadoPanelSimulado.EnFalla, lectura.Estado);
        Assert.False(lectura.EstaOperativo);
    }
}
