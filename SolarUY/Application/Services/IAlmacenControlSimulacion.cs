using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

/// <summary>
/// Conserva la orden vigente para el simulador. La implementación concreta vive en Infraestructura.
/// </summary>
public interface IAlmacenControlSimulacion
{
    /// <summary>
    /// Devuelve la orden vigente; si todavía no hay ninguna (o expiró) devuelve el estado inicial
    /// (sin simulación forzada y secuencia de falla 0).
    /// </summary>
    Task<ControlSimulacionDto> ObtenerAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reemplaza la orden vigente; la última escritura gana.
    /// </summary>
    Task GuardarAsync(ControlSimulacionDto control, CancellationToken cancellationToken = default);
}
