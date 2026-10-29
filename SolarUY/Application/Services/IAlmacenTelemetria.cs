using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

/// <summary>
/// Conserva la última lectura de telemetría recibida. La implementación concreta vive en Infraestructura.
/// </summary>
public interface IAlmacenTelemetria
{
    /// <summary>
    /// Devuelve la última lectura, o <c>null</c> si todavía no se recibió ninguna o si expiró.
    /// </summary>
    Task<TelemetriaDto?> ObtenerAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reemplaza la última lectura; la última escritura gana.
    /// </summary>
    Task GuardarAsync(TelemetriaDto lectura, CancellationToken cancellationToken = default);
}
