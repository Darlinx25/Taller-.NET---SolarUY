using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Domain.Entities;

namespace SolarUY.Domain.Repositories;

public interface IPanelRepository
{
    Task AgregarAsync(Panel panel, CancellationToken cancellationToken = default);

    Task<Panel?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Panel>> ObtenerPorNodoAsync(Guid nodoId, CancellationToken cancellationToken = default);

    Task<bool> ExisteCodigoAsync(
        string codigo,
        Guid nodoId,
        Guid? panelIdExcluido = null,
        CancellationToken cancellationToken = default);

    void Eliminar(Panel panel);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
