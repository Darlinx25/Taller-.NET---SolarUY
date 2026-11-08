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
/// Conserva la orden vigente para el simulador en <see cref="IDistributedCache"/>, compartida entre réplicas
/// cuando hay Redis. Se pierde al expirar o, sin Redis, al reiniciar la aplicación.
/// </summary>
public class AlmacenControlSimulacionCache : IAlmacenControlSimulacion
{
    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;
    private readonly string _clave;
    private readonly TimeSpan _vigencia;

    public AlmacenControlSimulacionCache(
        IDistributedCache cache,
        IOptions<ConfiguracionAlmacenTelemetria> opciones)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        ArgumentNullException.ThrowIfNull(opciones);

        var configuracion = opciones.Value;
        if (string.IsNullOrWhiteSpace(configuracion.ClaveControl))
            throw new InvalidOperationException(
                $"{ConfiguracionAlmacenTelemetria.Seccion}:{nameof(ConfiguracionAlmacenTelemetria.ClaveControl)} es obligatoria.");

        if (configuracion.VigenciaHoras <= 0)
            throw new InvalidOperationException(
                $"{ConfiguracionAlmacenTelemetria.Seccion}:{nameof(ConfiguracionAlmacenTelemetria.VigenciaHoras)} debe ser mayor a 0.");

        _clave = configuracion.ClaveControl;
        _vigencia = TimeSpan.FromHours(configuracion.VigenciaHoras);
    }

    public async Task<ControlSimulacionDto> ObtenerAsync(CancellationToken cancellationToken = default)
    {
        var contenido = await _cache.GetAsync(_clave, cancellationToken).ConfigureAwait(false);
        if (contenido is null || contenido.Length == 0)
            return new ControlSimulacionDto();

        return JsonSerializer.Deserialize<ControlSimulacionDto>(contenido, OpcionesJson) ?? new ControlSimulacionDto();
    }

    public Task GuardarAsync(ControlSimulacionDto control, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(control);

        var contenido = JsonSerializer.SerializeToUtf8Bytes(control, OpcionesJson);
        var opciones = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _vigencia
        };

        return _cache.SetAsync(_clave, contenido, opciones, cancellationToken);
    }
}
