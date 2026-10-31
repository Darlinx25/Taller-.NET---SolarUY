using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

public interface IHistoricoService
{
    FiltroHistoricoDto CrearFiltroPredeterminado();

    Task<OpcionesHistoricoDto> ObtenerOpcionesAsync(CancellationToken cancellationToken = default);

    Task<HistoricoGeneracionDto> ConsultarAsync(
        FiltroHistoricoDto filtro,
        CancellationToken cancellationToken = default);
}
