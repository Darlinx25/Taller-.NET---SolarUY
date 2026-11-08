using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

/// <summary>
/// Gestiona las órdenes que el administrador le da al simulador desde el dashboard.
/// </summary>
public interface IControlSimulacionService
{
    /// <summary>
    /// Devuelve la orden vigente.
    /// </summary>
    Task<ControlSimulacionDto> ObtenerAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia o detiene la simulación forzada y avisa al dashboard. Devuelve la orden resultante.
    /// </summary>
    Task<ControlSimulacionDto> EstablecerSimulacionForzadaAsync(bool activa, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pide que el simulador falle todos los paneles activos y avisa al dashboard. Devuelve la orden resultante.
    /// </summary>
    Task<ControlSimulacionDto> SolicitarFallaAsync(CancellationToken cancellationToken = default);
}
