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
            ProbabilidadRecuperacion = 0.4m,
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

    // 06:00 UTC = 03:00 en Uruguay: fuera de la ventana solar.
    private static readonly DateTimeOffset Madrugada = new(2026, 11, 5, 6, 0, 0, TimeSpan.Zero);

    private readonly ProveedorControlFalso _control = new();

    private SimuladorInversor CrearSimuladorConControl(decimal probabilidadFalla = 0m, decimal probabilidadRecuperacion = 0.4m)
    {
        var configuracion = Options.Create(new ConfiguracionSimulador
        {
            IntervaloSegundos = 5,
            ProbabilidadFalla = probabilidadFalla,
            ProbabilidadRecuperacion = probabilidadRecuperacion,
            VariacionMaxima = 0m,
            DesfaseHorarioUruguayHoras = -3
        });

        return new SimuladorInversor(
            _proveedor,
            configuracion,
            new RelojFijo(Madrugada),
            NullLogger<SimuladorInversor>.Instance,
            _control);
    }

    private static ControlSimulacionDto Control(bool simulacionForzada = false, long secuenciaFalla = 0) =>
        new() { SimulacionForzada = simulacionForzada, SecuenciaFalla = secuenciaFalla };

    [Fact]
    public async Task GenerarLectura_SinSimulacionForzadaDeNoche_InformaCeroWatts()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        _control.Control = Control();

        var lecturas = await CrearSimuladorConControl().GenerarLecturaAsync(CancellationToken.None);

        Assert.Equal(0m, Assert.Single(lecturas!).PotenciaGeneradaW);
    }

    [Fact]
    public async Task GenerarLectura_ConSimulacionForzadaDeNoche_GeneraComoAlPicoSinSuperarLaPotenciaRegistrada()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        _control.Control = Control(simulacionForzada: true);

        var lecturas = await CrearSimuladorConControl().GenerarLecturaAsync(CancellationToken.None);

        var parque = Assert.Single(lecturas!);
        Assert.Equal(700m, parque.PotenciaGeneradaW);
        Assert.All(Assert.Single(parque.Nodos).Paneles, panel => Assert.True(panel.EstaOperativo));
    }

    [Fact]
    public async Task GenerarLectura_ConDiaForzadoYProbabilidadDeFallaUno_LosPanelesFallanAleatoriamente()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        _control.Control = Control(simulacionForzada: true);

        var lecturas = await CrearSimuladorConControl(probabilidadFalla: 1m).GenerarLecturaAsync(CancellationToken.None);

        var parque = Assert.Single(lecturas!);
        Assert.All(Assert.Single(parque.Nodos).Paneles, panel =>
        {
            Assert.Equal(EstadoPanelSimulado.EnFalla, panel.Estado);
            Assert.Equal(0m, panel.PotenciaGeneradaW);
        });
        Assert.Equal(0m, parque.PotenciaGeneradaW);
    }

    [Fact]
    public async Task GenerarLectura_ConDiaForzadoYProbabilidadDeFallaCero_NingunPanelFalla()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        _control.Control = Control(simulacionForzada: true);
        var simulador = CrearSimuladorConControl(probabilidadFalla: 0m);

        for (var tick = 0; tick < 20; tick++)
        {
            var lecturas = await simulador.GenerarLecturaAsync(CancellationToken.None);
            Assert.All(Assert.Single(Assert.Single(lecturas!).Nodos).Paneles, panel => Assert.True(panel.EstaOperativo));
        }
    }

    [Fact]
    public async Task GenerarLectura_ConSimulacionForzadaYInversorChico_RespetaElLimiteDelInversor()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles(potenciaNominalKW: 0.5m));
        _control.Control = Control(simulacionForzada: true);

        var lecturas = await CrearSimuladorConControl().GenerarLecturaAsync(CancellationToken.None);

        Assert.Equal(500m, Assert.Single(lecturas!).PotenciaGeneradaW);
    }

    [Fact]
    public async Task GenerarLectura_AlDesactivarLaSimulacionForzada_VuelveALaHoraReal()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        var simulador = CrearSimuladorConControl();

        _control.Control = Control(simulacionForzada: true);
        var forzada = await simulador.GenerarLecturaAsync(CancellationToken.None);

        _control.Control = Control(simulacionForzada: false);
        var real = await simulador.GenerarLecturaAsync(CancellationToken.None);

        Assert.Equal(700m, Assert.Single(forzada!).PotenciaGeneradaW);
        Assert.Equal(0m, Assert.Single(real!).PotenciaGeneradaW);
    }

    [Fact]
    public async Task GenerarLectura_ConControlNoDisponible_ConservaElModoAnterior()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        var simulador = CrearSimuladorConControl();

        _control.Control = Control(simulacionForzada: true);
        await simulador.GenerarLecturaAsync(CancellationToken.None);

        _control.Control = null;
        var lecturas = await simulador.GenerarLecturaAsync(CancellationToken.None);

        Assert.Equal(700m, Assert.Single(lecturas!).PotenciaGeneradaW);
    }

    [Fact]
    public async Task GenerarLectura_PrimeraRespuestaConSecuenciaDeFalla_EsLineaBaseYNoFalla()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        _control.Control = Control(simulacionForzada: true, secuenciaFalla: 5);

        var lecturas = await CrearSimuladorConControl().GenerarLecturaAsync(CancellationToken.None);

        var nodo = Assert.Single(Assert.Single(lecturas!).Nodos);
        Assert.All(nodo.Paneles, panel => Assert.True(panel.EstaOperativo));
    }

    [Fact]
    public async Task GenerarLectura_ConSecuenciaDeFallaMayor_FallaTodosLosPanelesActivosConCeroWatts()
    {
        _proveedor.Topologia = Topologia(
            NodoActivoConDosPaneles(),
            Nodo(NodoInactivoId, "EnMantenimiento", null, Panel(PanelCId, "PAN-C", 500m)));
        var simulador = CrearSimuladorConControl();

        _control.Control = Control(simulacionForzada: true, secuenciaFalla: 5);
        await simulador.GenerarLecturaAsync(CancellationToken.None);

        _control.Control = Control(simulacionForzada: true, secuenciaFalla: 6);
        var lecturas = await simulador.GenerarLecturaAsync(CancellationToken.None);

        var parque = Assert.Single(lecturas!);
        var nodo = Assert.Single(parque.Nodos);
        Assert.Equal(2, nodo.Paneles.Count);
        Assert.All(nodo.Paneles, panel =>
        {
            Assert.Equal(EstadoPanelSimulado.EnFalla, panel.Estado);
            Assert.Equal(0m, panel.PotenciaGeneradaW);
        });
        Assert.Equal(0m, parque.PotenciaGeneradaW);
    }

    [Fact]
    public async Task GenerarLectura_ConLaMismaSecuenciaDeFalla_NoVuelveAFallarLosPaneles()
    {
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        // Recuperación segura: tras la lectura de la falla forzada, el panel se recupera y no vuelve a fallar
        // mientras la secuencia no cambie (la probabilidad de falla aleatoria es 0).
        var simulador = CrearSimuladorConControl(probabilidadRecuperacion: 1m);

        _control.Control = Control(simulacionForzada: true, secuenciaFalla: 5);
        await simulador.GenerarLecturaAsync(CancellationToken.None);

        _control.Control = Control(simulacionForzada: true, secuenciaFalla: 6);
        var primera = await simulador.GenerarLecturaAsync(CancellationToken.None);
        var segunda = await simulador.GenerarLecturaAsync(CancellationToken.None);
        var tercera = await simulador.GenerarLecturaAsync(CancellationToken.None);

        Assert.All(Assert.Single(Assert.Single(primera!).Nodos).Paneles, p => Assert.False(p.EstaOperativo));
        Assert.All(Assert.Single(Assert.Single(segunda!).Nodos).Paneles, p => Assert.True(p.EstaOperativo));
        Assert.All(Assert.Single(Assert.Single(tercera!).Nodos).Paneles, p => Assert.True(p.EstaOperativo));
    }

    [Fact]
    public async Task GenerarLectura_ConTopologiaNoDisponible_NoConsumeLaFallaPendiente()
    {
        var simulador = CrearSimuladorConControl();
        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        _control.Control = Control(simulacionForzada: true, secuenciaFalla: 5);
        await simulador.GenerarLecturaAsync(CancellationToken.None);

        _proveedor.Topologia = null;
        _control.Control = Control(simulacionForzada: true, secuenciaFalla: 6);
        var omitida = await simulador.GenerarLecturaAsync(CancellationToken.None);

        _proveedor.Topologia = Topologia(NodoActivoConDosPaneles());
        var lecturas = await simulador.GenerarLecturaAsync(CancellationToken.None);

        Assert.Null(omitida);
        var nodo = Assert.Single(Assert.Single(lecturas!).Nodos);
        Assert.All(nodo.Paneles, panel => Assert.False(panel.EstaOperativo));
    }

    private sealed class ProveedorControlFalso : IProveedorControl
    {
        public ControlSimulacionDto? Control { get; set; }

        public Task<ControlSimulacionDto?> ObtenerAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Control);
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
