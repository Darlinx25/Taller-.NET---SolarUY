using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

/// <summary>
/// Casos de uso relacionados con la gestión de parques solares.
/// </summary>
public interface IParqueSolarService
{
    /// <summary>
    /// Registra un nuevo parque solar en la plataforma.
    /// </summary>
    /// <param name="dto">Datos del parque solar a crear.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Identificador del parque solar creado.</returns>
    /// <exception cref="Domain.Exceptions.NombreParqueDuplicadoException">
    /// Si ya existe un parque solar con el mismo nombre.
    /// </exception>
    Task<Guid> CrearParqueSolarAsync(CrearParqueSolarDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el listado de parques solares registrados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Listado de parques solares.</returns>
    Task<IReadOnlyList<ParqueSolarDto>> ObtenerParquesAsync(CancellationToken cancellationToken = default);
}
