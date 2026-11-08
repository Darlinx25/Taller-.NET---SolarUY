using SolarUY.Simulador.Domain;
using SolarUY.Simulador.Transport;
using Xunit;

namespace SolarUY.Simulador.Tests.Transport;

public class TelemetriaMapperTests
{
    private static readonly DateTime FechaHoraUtc = new(2026, 11, 5, 16, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan CincoSegundos = TimeSpan.FromSeconds(5);

    private static LecturaParqueSolarSimulado CrearParque(
        string nombre,
        EstadoPanelSimulado estadoPanel = EstadoPanelSimulado.Operativo,
        decimal potenciaPanelW = 400m,
        decimal energiaAnteriorKWh = 0m)
    {
        var panel = new LecturaPanelSimulado(Guid.NewGuid(), "PAN-001", potenciaPanelW, estadoPanel, FechaHoraUtc);
        var nodo = LecturaNodoSimulado.Crear(
            Guid.NewGuid(), "N-01", new[] { panel }, energiaAnteriorKWh, null, CincoSegundos, FechaHoraUtc);

        return LecturaParqueSolarSimulado.Crear(Guid.NewGuid(), nombre, new[] { nodo }, FechaHoraUtc);
    }

    [Fact]
    public void Mapear_ListaVacia_DevuelveFechaHoraNulaYParquesVacios()
    {
        var dto = TelemetriaMapper.Mapear(Array.Empty<LecturaParqueSolarSimulado>());

        Assert.Null(dto.FechaHora);
        Assert.Empty(dto.Parques);
    }

    [Fact]
    public void Mapear_ConLecturas_ConservaOrdenYValores()
    {
        var norte = CrearParque("Parque Norte");
        var sur = CrearParque("Parque Sur", potenciaPanelW: 250m);

        var dto = TelemetriaMapper.Mapear(new[] { norte, sur });

        Assert.Equal(FechaHoraUtc, dto.FechaHora);
        Assert.Equal(new[] { "Parque Norte", "Parque Sur" }, dto.Parques.Select(p => p.NombreParque));

        var parque = dto.Parques[0];
        Assert.Equal(norte.ParqueSolarId, parque.ParqueSolarId);
        Assert.Equal(400m, parque.PotenciaGeneradaW);
        Assert.Equal(FechaHoraUtc, parque.FechaHora);

        var nodo = Assert.Single(parque.Nodos);
        Assert.Equal("N-01", nodo.CodigoNodo);
        Assert.Equal(400m, nodo.PotenciaGeneradaW);

        var panel = Assert.Single(nodo.Paneles);
        Assert.Equal("PAN-001", panel.CodigoSerie);
        Assert.Equal(400m, panel.PotenciaGeneradaW);
        Assert.Equal(FechaHoraUtc, panel.FechaHora);
    }

    [Theory]
    [InlineData(EstadoPanelSimulado.Operativo, "Operativo")]
    [InlineData(EstadoPanelSimulado.EnFalla, "EnFalla")]
    public void Mapear_EstadoDelPanel_SeInformaComoTexto(EstadoPanelSimulado estado, string esperado)
    {
        var dto = TelemetriaMapper.Mapear(new[] { CrearParque("Parque Norte", estado) });

        Assert.Equal(esperado, dto.Parques[0].Nodos[0].Paneles[0].Estado);
    }

    [Fact]
    public void Mapear_EnergiaAcumulada_SeRedondeaA4DecimalesSoloEnElDto()
    {
        var parque = CrearParque("Parque Norte", potenciaPanelW: 1000m);

        var dto = TelemetriaMapper.Mapear(new[] { parque });

        Assert.Equal(0.0014m, dto.Parques[0].EnergiaAcumuladaKWh);
        Assert.Equal(0.0014m, dto.Parques[0].Nodos[0].EnergiaAcumuladaKWh);
        Assert.NotEqual(parque.EnergiaAcumuladaKWh, dto.Parques[0].EnergiaAcumuladaKWh);
    }

    [Fact]
    public void Mapear_ConListaNula_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() => TelemetriaMapper.Mapear(null!));
    }
}
