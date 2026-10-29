using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SolarUY.Application.Dtos;
using SolarUY.Application.Services;

namespace SolarUY.Infrastructure.Telemetria;

/// <summary>
/// Conserva la última lectura de telemetría en <see cref="IDistributedCache"/>. Hoy es una caché en memoria;
/// con Redis solo cambia el registro de dependencias.
/// </summary>
public class AlmacenTelemetriaCache : IAlmacenTelemetria
{
    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;
    private readonly string _clave;
    private readonly TimeSpan _vigencia;

    public AlmacenTelemetriaCache(
        IDistributedCache cache,
        IOptions<ConfiguracionAlmacenTelemetria> opciones)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        ArgumentNullException.ThrowIfNull(opciones);

        var configuracion = opciones.Value;
        if (string.IsNullOrWhiteSpace(configuracion.Clave))
            throw new InvalidOperationException(
                $"{ConfiguracionAlmacenTelemetria.Seccion}:{nameof(ConfiguracionAlmacenTelemetria.Clave)} es obligatoria.");

        if (configuracion.VigenciaHoras <= 0)
            throw new InvalidOperationException(
                $"{ConfiguracionAlmacenTelemetria.Seccion}:{nameof(ConfiguracionAlmacenTelemetria.VigenciaHoras)} debe ser mayor a 0.");

        _clave = configuracion.Clave;
        _vigencia = TimeSpan.FromHours(configuracion.VigenciaHoras);
    }

    public async Task<TelemetriaDto?> ObtenerAsync(CancellationToken cancellationToken = default)
    {
        var contenido = await _cache.GetAsync(_clave, cancellationToken).ConfigureAwait(false);
        if (contenido is null || contenido.Length == 0)
            return null;

        return JsonSerializer.Deserialize<TelemetriaDto>(contenido, OpcionesJson);
    }

    public Task GuardarAsync(TelemetriaDto lectura, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lectura);

        var contenido = JsonSerializer.SerializeToUtf8Bytes(lectura, OpcionesJson);
        var opciones = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _vigencia
        };

        return _cache.SetAsync(_clave, contenido, opciones, cancellationToken);
    }
}
