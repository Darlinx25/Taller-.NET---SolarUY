using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;

namespace SolarUY.Application.Services;

public interface ITelemetriaService
{
    Task<TelemetriaDto?> ObtenerUltimaLecturaAsync(CancellationToken cancellationToken = default);

    Task<ResultadoIngestaTelemetria> RegistrarLecturaAsync(
        TelemetriaDto? lectura,
        CancellationToken cancellationToken = default);
}

public sealed record ResultadoIngestaTelemetria(
    bool EsValida,
    IReadOnlyList<string> Errores);
