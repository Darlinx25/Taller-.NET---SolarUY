using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Domain.Entities;

namespace SolarUY.Domain.Repositories;

public interface ITareaMantenimientoRepository
{
    Task AgregarAsync(TareaMantenimiento tarea, CancellationToken cancellationToken = default);
    Task<TareaMantenimiento?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TareaMantenimiento>> ObtenerPorPanelAsync(Guid panelId, CancellationToken cancellationToken = default);
    void Eliminar(TareaMantenimiento tarea);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
