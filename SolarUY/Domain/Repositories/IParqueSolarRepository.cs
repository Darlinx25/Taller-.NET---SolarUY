using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Domain.Entities;

namespace SolarUY.Domain.Repositories;

/// <summary>
/// Contrato de persistencia de parques solares.
/// La implementación concreta vive en la capa de Infraestructura.
/// </summary>
public interface IParqueSolarRepository
{
    /// <summary>
    /// Indica si ya existe un parque solar registrado con el nombre indicado.
    /// </summary>
    /// <param name="nombre">Nombre a verificar.</param>
    /// <param name="parqueIdExcluido">Identificador de parque que no debe considerarse en la verificación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><c>true</c> si el nombre ya está en uso.</returns>
    Task<bool> ExisteNombreAsync(
        string nombre,
        Guid? parqueIdExcluido = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un parque solar por su identificador.
    /// </summary>
    /// <param name="id">Identificador del parque solar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El parque solar encontrado o <c>null</c> si no existe.</returns>
    Task<ParqueSolar?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un parque solar para ser eliminado.
    /// </summary>
    /// <param name="parque">Parque solar a eliminar.</param>
    void Eliminar(ParqueSolar parque);

    /// <summary>
    /// Agrega un parque solar al conjunto de datos a persistir.
    /// </summary>
    /// <param name="parque">Parque solar a agregar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Tarea que representa la operación asíncrona.</returns>
    Task AgregarAsync(ParqueSolar parque, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los parques solares registrados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Listado de parques solares.</returns>
    Task<IReadOnlyList<ParqueSolar>> ObtenerTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste los cambios pendientes.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Cantidad de registros afectados.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
