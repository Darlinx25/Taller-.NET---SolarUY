using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarUY.Simulador.Contracts;
using SolarUY.Simulador.Domain;

namespace SolarUY.Simulador.Simulation;

/// <summary>
/// Genera las lecturas de cada tick a partir de la topología registrada. Conserva en memoria el estado
/// de fallas de los paneles y la energía acumulada de los nodos. No conoce HTTP ni MySQL.
/// </summary>
public class SimuladorInversor
{
    private const string EstadoNodoActivo = "Activo";

    private readonly IProveedorTopologia _proveedorTopologia;
    private readonly ConfiguracionSimulador _configuracion;
    private readonly TimeProvider _tiempo;
    private readonly ILogger<SimuladorInversor> _logger;

    private readonly Dictionary<Guid, PanelSimulado> _panelesPorId = new();
    private readonly Dictionary<Guid, decimal> _energiaAcumuladaPorNodo = new();

    public SimuladorInversor(
        IProveedorTopologia proveedorTopologia,
        IOptions<ConfiguracionSimulador> configuracion,
        TimeProvider tiempo,
        ILogger<SimuladorInversor> logger)
    {
        _proveedorTopologia = proveedorTopologia ?? throw new ArgumentNullException(nameof(proveedorTopologia));
        ArgumentNullException.ThrowIfNull(configuracion);
        _configuracion = configuracion.Value;
        _tiempo = tiempo ?? throw new ArgumentNullException(nameof(tiempo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Genera una lectura por parque registrado. Devuelve <c>null</c> (sin modificar el estado) si la
    /// topología no está disponible, y una lista vacía si no hay parques.
    /// </summary>
    public async Task<IReadOnlyList<LecturaParqueSolarSimulado>?> GenerarLecturaAsync(
        CancellationToken cancellationToken)
    {
        var topologia = await _proveedorTopologia
            .ObtenerAsync(cancellationToken)
            .ConfigureAwait(false);

        if (topologia is null)
            return null;

        var fechaHoraUtc = _tiempo.GetUtcNow().UtcDateTime;
        var horaLocal = fechaHoraUtc.AddHours(_configuracion.DesfaseHorarioUruguayHoras);
        var intervalo = TimeSpan.FromSeconds(_configuracion.IntervaloSegundos);

        var nodosEnTopologia = new HashSet<Guid>();
        var panelesActivos = new HashSet<Guid>();
        var lecturas = new List<LecturaParqueSolarSimulado>(topologia.Parques.Count);

        foreach (var parque in topologia.Parques)
        {
            var lecturasNodo = new List<LecturaNodoSimulado>();
            foreach (var nodo in parque.Nodos)
            {
                nodosEnTopologia.Add(nodo.NodoId);

                if (!EstaActivo(nodo))
                {
                    _logger.LogDebug("Nodo {CodigoNodo} en estado {Estado}: no se simula.", nodo.CodigoNodo, nodo.Estado);
                    continue;
                }

                lecturasNodo.Add(GenerarLecturaNodo(nodo, horaLocal, fechaHoraUtc, intervalo, panelesActivos));
            }

            lecturas.Add(LecturaParqueSolarSimulado.Crear(
                parque.ParqueSolarId,
                parque.NombreParque,
                lecturasNodo,
                fechaHoraUtc));
        }

        DescartarEstadoObsoleto(nodosEnTopologia, panelesActivos);

        return lecturas;
    }

    private static bool EstaActivo(NodoSimulacionDto nodo)
    {
        return string.Equals(nodo.Estado, EstadoNodoActivo, StringComparison.OrdinalIgnoreCase);
    }

    private LecturaNodoSimulado GenerarLecturaNodo(
        NodoSimulacionDto nodo,
        DateTime horaLocal,
        DateTime fechaHoraUtc,
        TimeSpan intervalo,
        HashSet<Guid> panelesActivos)
    {
        var lecturasPanel = new List<LecturaPanelSimulado>(nodo.Paneles.Count);
        foreach (var panelDto in nodo.Paneles)
        {
            var panel = ObtenerPanel(nodo.NodoId, panelDto);
            panelesActivos.Add(panel.PanelId);

            panel.ActualizarEstado(
                _configuracion.ProbabilidadFalla,
                _configuracion.DuracionFallaTicks,
                (decimal)Random.Shared.NextDouble());

            lecturasPanel.Add(panel.GenerarLectura(horaLocal, GenerarVariacion(), fechaHoraUtc));
        }

        var lectura = LecturaNodoSimulado.Crear(
            nodo.NodoId,
            nodo.CodigoNodo,
            lecturasPanel,
            _energiaAcumuladaPorNodo.GetValueOrDefault(nodo.NodoId),
            nodo.PotenciaNominalInversorKW,
            intervalo,
            fechaHoraUtc);

        _energiaAcumuladaPorNodo[nodo.NodoId] = lectura.EnergiaAcumuladaKWh;
        return lectura;
    }

    /// <summary>
    /// Devuelve el panel simulado conservando su estado de falla, salvo que haya cambiado su código o su
    /// potencia registrados: en ese caso se reemplaza para no superar nunca la potencia registrada.
    /// </summary>
    private PanelSimulado ObtenerPanel(Guid nodoId, PanelSimulacionDto panelDto)
    {
        var codigoSerie = (panelDto.CodigoSerie ?? string.Empty).Trim();

        if (_panelesPorId.TryGetValue(panelDto.PanelId, out var existente)
            && existente.CodigoSerie == codigoSerie
            && existente.PotenciaWp == panelDto.PotenciaWp)
        {
            return existente;
        }

        var nuevo = new PanelSimulado(panelDto.PanelId, nodoId, codigoSerie, panelDto.PotenciaWp);
        _panelesPorId[panelDto.PanelId] = nuevo;
        return nuevo;
    }

    private decimal GenerarVariacion()
    {
        var factor = (decimal)(Random.Shared.NextDouble() * 2d - 1d);
        return factor * _configuracion.VariacionMaxima;
    }

    /// <summary>
    /// Olvida los paneles que ya no están en nodos activos y la energía de los nodos que ya no existen.
    /// La energía de un nodo inactivo pero todavía registrado se conserva.
    /// </summary>
    private void DescartarEstadoObsoleto(HashSet<Guid> nodosEnTopologia, HashSet<Guid> panelesActivos)
    {
        foreach (var panelId in _panelesPorId.Keys.Where(id => !panelesActivos.Contains(id)).ToList())
            _panelesPorId.Remove(panelId);

        foreach (var nodoId in _energiaAcumuladaPorNodo.Keys.Where(id => !nodosEnTopologia.Contains(id)).ToList())
            _energiaAcumuladaPorNodo.Remove(nodoId);
    }
}
