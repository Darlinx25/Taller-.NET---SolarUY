using SolarUY.Simulador.Domain;
using Xunit;

namespace SolarUY.Simulador.Tests.Domain;

public class LecturaPanelSimuladoTests
{
    private static readonly Guid PanelId = Guid.NewGuid();
    private static readonly DateTime FechaHoraUtc = new(2026, 11, 5, 16, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Crear_LecturaPanel_ConDatosValidos_AsignaValores()
    {
        var lectura = new LecturaPanelSimulado(PanelId, "  PAN-001 ", 350.5m, EstadoPanelSimulado.Operativo, FechaHoraUtc);

        Assert.Equal(PanelId, lectura.PanelId);
        Assert.Equal("PAN-001", lectura.CodigoSerie);
        Assert.Equal(350.5m, lectura.PotenciaGeneradaW);
        Assert.Equal(EstadoPanelSimulado.Operativo, lectura.Estado);
        Assert.Equal(FechaHoraUtc, lectura.FechaHora);
    }

    [Fact]
    public void Crear_LecturaPanel_ConPotenciaCero_SeAcepta()
    {
        var lectura = new LecturaPanelSimulado(PanelId, "PAN-001", 0m, EstadoPanelSimulado.EnFalla, FechaHoraUtc);

        Assert.Equal(0m, lectura.PotenciaGeneradaW);
    }

    [Fact]
    public void Crear_LecturaPanel_ConPanelIdVacio_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            new LecturaPanelSimulado(Guid.Empty, "PAN-001", 1m, EstadoPanelSimulado.Operativo, FechaHoraUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_LecturaPanel_ConCodigoSerieVacio_LanzaExcepcion(string? codigoSerie)
    {
        Assert.Throws<ArgumentException>(() =>
            new LecturaPanelSimulado(PanelId, codigoSerie!, 1m, EstadoPanelSimulado.Operativo, FechaHoraUtc));
    }

    [Fact]
    public void Crear_LecturaPanel_ConPotenciaNegativa_LanzaExcepcion()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LecturaPanelSimulado(PanelId, "PAN-001", -0.01m, EstadoPanelSimulado.Operativo, FechaHoraUtc));
    }

    [Fact]
    public void Crear_LecturaPanel_ConFechaHoraPorDefecto_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            new LecturaPanelSimulado(PanelId, "PAN-001", 1m, EstadoPanelSimulado.Operativo, default));
    }

    [Theory]
    [InlineData(EstadoPanelSimulado.Operativo, true)]
    [InlineData(EstadoPanelSimulado.EnFalla, false)]
    public void EstaOperativo_SegunElEstado_DevuelveElValorEsperado(EstadoPanelSimulado estado, bool esperado)
    {
        var lectura = new LecturaPanelSimulado(PanelId, "PAN-001", 0m, estado, FechaHoraUtc);

        Assert.Equal(esperado, lectura.EstaOperativo);
    }
}
