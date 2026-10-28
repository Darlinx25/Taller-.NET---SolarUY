using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

public interface INodoService
{
    Task<IReadOnlyList<NodoDto>> ObtenerNodosPorParqueAsync(Guid parqueSolarId, CancellationToken cancellationToken = default);

    Task<NodoDetalleDto?> ObtenerNodoAsync(Guid nodoId, CancellationToken cancellationToken = default);

    Task<Guid> CrearNodoInversorAsync(CrearNodoInversorDto dto, CancellationToken cancellationToken = default);

    Task EditarNodoInversorAsync(EditarNodoInversorDto dto, CancellationToken cancellationToken = default);

    Task EliminarNodoAsync(Guid nodoId, CancellationToken cancellationToken = default);
}
