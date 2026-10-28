using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Domain.Entities;

namespace SolarUY.Domain.Repositories;

/// <summary>
/// Contrato de repositorio para el agregado de nodos.
/// </summary>
public interface INodoRepository
{
    Task AgregarAsync(Nodo nodo, CancellationToken cancellationToken = default);

    Task<Nodo?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Nodo>> ObtenerPorParqueAsync(Guid parqueSolarId, CancellationToken cancellationToken = default);

    Task<bool> ExisteCodigoAsync(string codigo, Guid parqueSolarId, Guid? nodoIdExcluido = null, CancellationToken cancellationToken = default);

    void Eliminar(Nodo nodo);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
