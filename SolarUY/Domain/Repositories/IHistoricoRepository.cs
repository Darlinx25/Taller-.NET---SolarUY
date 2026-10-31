using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Domain.Entities;
using SolarUY.Domain.ValueObjects;

namespace SolarUY.Domain.Repositories;

public interface IHistoricoRepository
{
    Task<IReadOnlyList<ResumenDiarioParque>> ObtenerPorFechaAsync(
        DateOnly fechaLocal,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResumenDiarioGeneracion>> ObtenerResumenesAsync(
        FiltroHistorico filtro,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        ResumenDiarioParque resumen,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
