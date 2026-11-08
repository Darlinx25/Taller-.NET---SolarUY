using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarUY.Simulador.Contracts;

namespace SolarUY.Simulador.Transport;

/// <summary>
/// Envía una lectura a la aplicación con <c>POST api/simulacion/telemetria</c>.
/// Devuelve <c>true</c> solo para una respuesta 2xx; cualquier otro resultado se registra y devuelve <c>false</c>.
/// </summary>
public class ClienteTelemetria
{
    private const string Ruta = "api/simulacion/telemetria";

    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ClienteTelemetria> _logger;

    public ClienteTelemetria(
        HttpClient httpClient,
        IOptions<ConfiguracionSimulacionApi> configuracion,
        ILogger<ClienteTelemetria> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        ArgumentNullException.ThrowIfNull(configuracion);
        _apiKey = configuracion.Value.ApiKey;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> EnviarAsync(TelemetriaDto lectura, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lectura);

        try
        {
            using var solicitud = new HttpRequestMessage(HttpMethod.Post, Ruta)
            {
                Content = JsonContent.Create(lectura, options: OpcionesJson)
            };
            solicitud.Headers.Add(ConfiguracionSimulacionApi.EncabezadoApiKey, _apiKey);

            using var respuesta = await _httpClient
                .SendAsync(solicitud, cancellationToken)
                .ConfigureAwait(false);

            if (respuesta.IsSuccessStatusCode)
                return true;

            var fragmento = await RespuestaHttp.LeerFragmentoAsync(respuesta, cancellationToken).ConfigureAwait(false);
            _logger.LogWarning(
                "La aplicación rechazó la lectura: respuesta {Codigo}. {Fragmento}",
                (int)respuesta.StatusCode,
                fragmento);
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
                                   && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("No se pudo enviar la lectura: {Motivo}", ex.Message);
            return false;
        }
    }
}
