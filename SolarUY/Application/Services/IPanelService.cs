using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

public interface IPanelService
{
    Task<IReadOnlyList<PanelDto>> ObtenerPanelesPorNodoAsync(
        Guid nodoId,
        CancellationToken cancellationToken = default);

    Task<PanelDetalleDto?> ObtenerPanelAsync(
        Guid panelId,
        CancellationToken cancellationToken = default);

    Task<Guid> CrearPanelAsync(
        CrearPanelDto dto,
        CancellationToken cancellationToken = default);

    Task EditarPanelAsync(
        EditarPanelDto dto,
        CancellationToken cancellationToken = default);

    Task EliminarPanelAsync(
        Guid panelId,
        CancellationToken cancellationToken = default);
}
