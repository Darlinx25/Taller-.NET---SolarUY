using SolarUY.Simulador.Contracts;

namespace SolarUY.Simulador.Simulation;

/// <summary>
/// Abstracción de "de dónde sale la orden vigente" (simulación forzada y falla forzada).
/// </summary>
public interface IProveedorControl
{
    /// <summary>
    /// Devuelve la orden vigente, o <c>null</c> si no está disponible.
    /// </summary>
    Task<ControlSimulacionDto?> ObtenerAsync(CancellationToken cancellationToken);
}
