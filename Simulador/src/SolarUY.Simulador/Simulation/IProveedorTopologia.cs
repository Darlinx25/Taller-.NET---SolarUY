using SolarUY.Simulador.Contracts;

namespace SolarUY.Simulador.Simulation;

/// <summary>
/// De dónde obtiene el simulador la topología registrada. Permite que la simulación no conozca HTTP.
/// </summary>
public interface IProveedorTopologia
{
    /// <summary>
    /// Devuelve la topología, o <c>null</c> si no está disponible.
    /// </summary>
    Task<TopologiaSimulacionDto?> ObtenerAsync(CancellationToken cancellationToken);
}
