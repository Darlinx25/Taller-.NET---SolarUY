using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

public interface ITareaMantenimientoService
{
    Task<IReadOnlyList<TareaMantenimientoDto>> ObtenerTareasPorPanelAsync(Guid panelId, CancellationToken cancellationToken = default);
    Task<TareaMantenimientoDetalleDto> ObtenerTareaAsync(Guid tareaId, CancellationToken cancellationToken = default);
    Task<TareaMantenimientoDto> CrearTareaAsync(CrearTareaMantenimientoDto dto, CancellationToken cancellationToken = default);
    Task<TareaMantenimientoDto> EditarTareaAsync(EditarTareaMantenimientoDto dto, CancellationToken cancellationToken = default);
    Task CambiarEstadoAsync(CambiarEstadoTareaMantenimientoDto dto, CancellationToken cancellationToken = default);
    Task EliminarTareaAsync(Guid tareaId, CancellationToken cancellationToken = default);
}
