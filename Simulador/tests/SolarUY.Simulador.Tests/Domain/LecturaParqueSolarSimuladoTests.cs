using SolarUY.Simulador.Domain;
using Xunit;

namespace SolarUY.Simulador.Tests.Domain;

public class LecturaParqueSolarSimuladoTests
{
    private static readonly Guid ParqueId = Guid.NewGuid();
    private static readonly DateTime FechaHoraUtc = new(2026, 11, 5, 16, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan CincoSegundos = TimeSpan.FromSeconds(5);

    private static LecturaNodoSimulado Nodo(string codigo, decimal potenciaW, decimal energiaAnteriorKWh = 0m)
    {
        var panel = new LecturaPanelSimulado(Guid.NewGuid(), "PAN-001", potenciaW, EstadoPanelSimulado.Operativo, FechaHoraUtc);

        return LecturaNodoSimulado.Crear(
            Guid.NewGuid(), codigo, new[] { panel }, energiaAnteriorKWh, null, CincoSegundos, FechaHoraUtc);
    }

    [Fact]
    public void Crear_LecturaParque_SumaPotenciaYEnergiaDeSusNodos()
    {
        var nodoA = Nodo("N-01", 1000m, 1m);
        var nodoB = Nodo("N-02", 500m, 2m);

        var lectura = LecturaParqueSolarSimulado.Crear(ParqueId, "  Parque Norte ", new[] { nodoA, nodoB }, FechaHoraUtc);

        Assert.Equal(ParqueId, lectura.ParqueSolarId);
        Assert.Equal("Parque Norte", lectura.NombreParque);
        Assert.Equal(1500m, lectura.PotenciaGeneradaW);
        Assert.Equal(nodoA.EnergiaAcumuladaKWh + nodoB.EnergiaAcumuladaKWh, lectura.EnergiaAcumuladaKWh);
        Assert.Equal(2, lectura.Nodos.Count);
        Assert.Equal(FechaHoraUtc, lectura.FechaHora);
    }

    [Fact]
    public void Crear_LecturaParque_SinNodos_InformaCeroWattsCeroEnergiaYListaVacia()
    {
        var lectura = LecturaParqueSolarSimulado.Crear(ParqueId, "Parque Norte", Array.Empty<LecturaNodoSimulado>(), FechaHoraUtc);

        Assert.Equal(0m, lectura.PotenciaGeneradaW);
        Assert.Equal(0m, lectura.EnergiaAcumuladaKWh);
        Assert.Empty(lectura.Nodos);
    }

    [Fact]
    public void Crear_LecturaParque_ConIdVacio_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            LecturaParqueSolarSimulado.Crear(Guid.Empty, "Parque Norte", Array.Empty<LecturaNodoSimulado>(), FechaHoraUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_LecturaParque_ConNombreVacio_LanzaExcepcion(string? nombre)
    {
        Assert.Throws<ArgumentException>(() =>
            LecturaParqueSolarSimulado.Crear(ParqueId, nombre!, Array.Empty<LecturaNodoSimulado>(), FechaHoraUtc));
    }

    [Fact]
    public void Crear_LecturaParque_ConNodosNulos_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() =>
            LecturaParqueSolarSimulado.Crear(ParqueId, "Parque Norte", null!, FechaHoraUtc));
    }

    [Fact]
    public void Crear_LecturaParque_ConFechaHoraPorDefecto_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            LecturaParqueSolarSimulado.Crear(ParqueId, "Parque Norte", Array.Empty<LecturaNodoSimulado>(), default));
    }
}
