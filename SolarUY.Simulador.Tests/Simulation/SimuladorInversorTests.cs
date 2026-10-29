using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SolarUY.Simulador.Contracts;
using SolarUY.Simulador.Domain;
using SolarUY.Simulador.Simulation;
using Xunit;

namespace SolarUY.Simulador.Tests.Simulation;

public class SimuladorInversorTests
{
    private static readonly DateTimeOffset LasTrece = new(2026, 11, 5, 16, 0, 0, TimeSpan.Zero);

    private static readonly Guid ParqueId = Guid.NewGuid();
    private static readonly Guid NodoActivoId = Guid.NewGuid();
    private static readonly Guid NodoInactivoId = Guid.NewGuid();
    private static readonly Guid PanelAId = Guid.NewGuid();
    private static readonly Guid PanelBId = Guid.NewGuid();
    private static readonly Guid PanelCId = Guid.NewGuid();

    private readonly ProveedorTopologiaFalso _proveedor = new();

    private SimuladorInversor CrearSimulador(decimal probabilidadFalla = 0m)
    {
        var configuracion = Options.Create(new ConfiguracionSimulador
        {
            IntervaloSegundos = 5,
            ProbabilidadFalla = probabilidadFalla,
            DuracionFallaTicks = 6,
            VariacionMaxima = 0m,
            DesfaseHorarioUruguayHoras = -3
        });

        return new SimuladorInversor(
            _proveedor,
            configuracion,
            new RelojFijo(LasTrece),
            NullLogger<SimuladorInversor>.Instance);
    }

    private static PanelSimulacionDto Panel(Guid id, string codigo, decimal potenciaWp) =>
        new() { PanelId = id, CodigoSerie = codigo, PotenciaWp = potenciaWp };

    private static NodoSimulacionDto Nodo(
        Guid id,
        string estado,
        decimal? potenciaNominalKW,
        params PanelSimulacionDto[] paneles) =>
        new()
        {
            NodoId = id,
            CodigoNodo = id == NodoActivoId ? "N-01" : "N-02",
            Estado = estado,
            PotenciaNominalInversorKW = potenciaNominalKW,
            Paneles = paneles
        };

    private static TopologiaSimulacionDto Topologia(params NodoSimulacionDto[] nodos) =>
        new()
        {
            Parques = new[]
            {
                new ParqueSimulacionDto { ParqueSolarId = ParqueId, NombreParque = "Parque Norte", Nodos = nodos }
            }
        };

    private static NodoSimulacionDto NodoActivoConDosPaneles(decimal? potenciaNominalKW = null) =>
        Nodo(NodoActivoId, "Activo", potenciaNominalKW, Panel(PanelAId, "PAN-A", 400m), Panel(PanelBId, "PAN-B", 300m));

    [Fact]
    public async Task GenerarLectura_ConNodoActivoYEnMantenimiento_SoloSimulaElActivo()
    {
        _proveedor.Topologia = Topologia(
            NodoActivoConDosPaneles(),
            Nodo(NodoInactivoId, "EnMantenimiento", null, Panel(PanelCId, "PAN-C", 500m)));

        var lecturas = await CrearSimulador().GenerarLecturaAsync(CancellationToken.None);

        var parque = Assert.Single(lecturas!);
        var nodo = Assert.Single(parque.Nodos);
        Assert.Equal(NodoActivoId, nodo.NodoId);
        Assert.Equal(700m, parque.PotenciaGeneradaW);
    }

    [Fact]
    public async Task GenerarLectura_EstadoActivoConOtraCapitalizacion_SeSimula()
    {
        _proveedor.Topologia = Topologia(Nodo(NodoActivoId, "activo", null, Panel(PanelAId, "PAN-A", 400m)));

        var lecturas = await CrearSimulador().GenerarLecturaAsync(CancellationToken.None);

        Assert.Single(Assert.Single(lecturas!).Nodos);
    }

    [Fact]
    public async Task GenerarLectura_ElNodoSumaSusPanelesYTodasLasLecturasComparteFechaHora()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());

        var lecturas = await CrearSimulador().GenerarLecturaAsync(CancellationToken.None);

        var parque = Assert.Single(lecturas!);
        var nodo = Assert.Single(parque.Nodos);
        Assert.Equal(700m, nodo.PotenciaGeneradaW);
        Assert.Equal(new[] { 400m, 300m }, nodo.Paneles.Select(p => p.PotenciaGeneradaW));
        Assert.All(nodo.Paneles, panel => Assert.Equal(LasTrece.UtcDateTime, panel.FechaHora));
        Assert.Equal(LasTrece.UtcDateTime, nodo.FechaHora);
        Assert.Equal(LasTrece.UtcDateTime, parque.FechaHora);
    }

    [Fact]
    public async Task GenerarLectura_ConInversorDePotenciaNominal_LimitaLaPotenciaDelNodo()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles(potenciaNominalKW: 0.5m));

        var lecturas = await CrearSimulador().GenerarLecturaAsync(CancellationToken.None);

        Assert.Equal(500m, Assert.Single(lecturas!).PotenciaGeneradaW);
    }

    [Fact]
    public async Task GenerarLectura_ConInversorDeCeroKW_InformaCeroWatts()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles(potenciaNominalKW: 0m));

        var lecturas = await CrearSimulador().GenerarLecturaAsync(CancellationToken.None);

        Assert.Equal(0m, Assert.Single(lecturas!).PotenciaGeneradaW);
    }

    [Fact]
    public async Task GenerarLectura_EnTicksConsecutivos_LaEnergiaAcumuladaCrece()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        var simulador = CrearSimulador();

        var primera = await simulador.GenerarLecturaAsync(CancellationToken.None);
        var segunda = await simulador.GenerarLecturaAsync(CancellationToken.None);

        var energiaPrimera = Assert.Single(primera!).EnergiaAcumuladaKWh;
        var energiaSegunda = Assert.Single(segunda!).EnergiaAcumuladaKWh;
        Assert.True(energiaPrimera > 0m);
        Assert.Equal(2 * energiaPrimera, energiaSegunda, 10);
    }

    [Fact]
    public async Task GenerarLectura_ParqueSinNodosActivos_InformaCeroWattsCeroEnergiaYNodosVacios()
    {
        _proveedor.Topologia = Topologia(Nodo(NodoInactivoId, "EnMantenimiento", null, Panel(PanelCId, "PAN-C", 500m)));

        var lecturas = await CrearSimulador().GenerarLecturaAsync(CancellationToken.None);

        var parque = Assert.Single(lecturas!);
        Assert.Equal(0m, parque.PotenciaGeneradaW);
        Assert.Equal(0m, parque.EnergiaAcumuladaKWh);
        Assert.Empty(parque.Nodos);
    }

    [Fact]
    public async Task GenerarLectura_TopologiaSinParques_DevuelveListaVacia()
    {
        _proveedor.Topologia = new TopologiaSimulacionDto();

        var lecturas = await CrearSimulador().GenerarLecturaAsync(CancellationToken.None);

        Assert.NotNull(lecturas);
        Assert.Empty(lecturas);
    }

    [Fact]
    public async Task GenerarLectura_ConTopologiaNoDisponible_DevuelveNullYConservaElEstado()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        var simulador = CrearSimulador();
        var primera = await simulador.GenerarLecturaAsync(CancellationToken.None);

        _proveedor.Topologia = null;
        var fallida = await simulador.GenerarLecturaAsync(CancellationToken.None);

        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        var tercera = await simulador.GenerarLecturaAsync(CancellationToken.None);

        Assert.Null(fallida);
        var energiaPrimera = Assert.Single(primera!).EnergiaAcumuladaKWh;
        Assert.Equal(2 * energiaPrimera, Assert.Single(tercera!).EnergiaAcumuladaKWh, 10);
    }

    [Fact]
    public async Task GenerarLectura_ConPotenciaDelPanelModificada_NoSuperaElNuevoValorRegistrado()
    {
        _proveedor.Topologia = Topologia(Nodo(NodoActivoId, "Activo", null, Panel(PanelAId, "PAN-A", 400m)));
        var simulador = CrearSimulador();
        await simulador.GenerarLecturaAsync(CancellationToken.None);

        _proveedor.Topologia = Topologia(Nodo(NodoActivoId, "Activo", null, Panel(PanelAId, "PAN-A", 100m)));
        var lecturas = await simulador.GenerarLecturaAsync(CancellationToken.None);

        var panel = Assert.Single(Assert.Single(Assert.Single(lecturas!).Nodos).Paneles);
        Assert.Equal(100m, panel.PotenciaGeneradaW);
    }

    [Fact]
    public async Task GenerarLectura_ConProbabilidadDeFallaUno_LosPanelesPasanAEnFallaConCeroWatts()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());

        var lecturas = await CrearSimulador(probabilidadFalla: 1m).GenerarLecturaAsync(CancellationToken.None);

        var nodo = Assert.Single(Assert.Single(lecturas!).Nodos);
        Assert.All(nodo.Paneles, panel =>
        {
            Assert.Equal(EstadoPanelSimulado.EnFalla, panel.Estado);
            Assert.Equal(0m, panel.PotenciaGeneradaW);
        });
        Assert.Equal(0m, nodo.PotenciaGeneradaW);
    }

    [Fact]
    public async Task GenerarLectura_NodoQueDejaDeEstarActivoYVuelve_ConservaSuEnergia()
    {
        var simulador = CrearSimulador();
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        var primera = await simulador.GenerarLecturaAsync(CancellationToken.None);

        _proveedor.Topologia = Topologia(Nodo(NodoActivoId, "EnMantenimiento", null, Panel(PanelAId, "PAN-A", 400m)));
        var intermedia = await simulador.GenerarLecturaAsync(CancellationToken.None);

        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        var ultima = await simulador.GenerarLecturaAsync(CancellationToken.None);

        Assert.Empty(Assert.Single(intermedia!).Nodos);
        var energiaPrimera = Assert.Single(primera!).EnergiaAcumuladaKWh;
        Assert.Equal(2 * energiaPrimera, Assert.Single(ultima!).EnergiaAcumuladaKWh, 10);
    }

    [Fact]
    public async Task GenerarLectura_NodoQueDesapareceDeLaTopologiaYVuelve_ReiniciaSuEnergia()
    {
        var simulador = CrearSimulador();
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        var primera = await simulador.GenerarLecturaAsync(CancellationToken.None);

        _proveedor.Topologia = Topologia();
        await simulador.GenerarLecturaAsync(CancellationToken.None);

        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        var ultima = await simulador.GenerarLecturaAsync(CancellationToken.None);

        Assert.Equal(
            Assert.Single(primera!).EnergiaAcumuladaKWh,
            Assert.Single(ultima!).EnergiaAcumuladaKWh,
            10);
    }

    private sealed class ProveedorTopologiaFalso : IProveedorTopologia
    {
        public TopologiaSimulacionDto? Topologia { get; set; }

        public Task<TopologiaSimulacionDto?> ObtenerAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Topologia);
    }

    private sealed class RelojFijo : TimeProvider
    {
        private readonly DateTimeOffset _ahora;

        public RelojFijo(DateTimeOffset ahora)
        {
            _ahora = ahora;
        }

        public override DateTimeOffset GetUtcNow() => _ahora;
    }
}
