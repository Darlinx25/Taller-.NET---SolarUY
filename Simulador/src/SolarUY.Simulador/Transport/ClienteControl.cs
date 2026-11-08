using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarUY.Simulador.Contracts;
using SolarUY.Simulador.Simulation;

namespace SolarUY.Simulador.Transport;

/// <summary>
/// Obtiene la orden vigente con <c>GET api/simulacion/control</c>. Cualquier fallo
/// (respuesta no exitosa, transporte, timeout o JSON inválido) se registra y devuelve <c>null</c>.
/// </summary>
public class ClienteControl : IProveedorControl
{
    private const string Ruta = "api/simulacion/control";

    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ClienteControl> _logger;

    public ClienteControl(
        HttpClient httpClient,
        IOptions<ConfiguracionSimulacionApi> configuracion,
        ILogger<ClienteControl> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        ArgumentNullException.ThrowIfNull(configuracion);
        _apiKey = configuracion.Value.ApiKey;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ControlSimulacionDto?> ObtenerAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var solicitud = new HttpRequestMessage(HttpMethod.Get, Ruta);
            solicitud.Headers.Add(ConfiguracionSimulacionApi.EncabezadoApiKey, _apiKey);

            using var respuesta = await _httpClient
                .SendAsync(solicitud, cancellationToken)
                .ConfigureAwait(false);

            if (!respuesta.IsSuccessStatusCode)
            {
                var fragmento = await RespuestaHttp.LeerFragmentoAsync(respuesta, cancellationToken).ConfigureAwait(false);
                _logger.LogWarning(
                    "No se pudo obtener la orden de control: respuesta {Codigo}. {Fragmento}",
                    (int)respuesta.StatusCode,
                    fragmento);
                return null;
            }

            return await respuesta.Content
                .ReadFromJsonAsync<ControlSimulacionDto>(OpcionesJson, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException
                                   && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("No se pudo obtener la orden de control: {Motivo}", ex.Message);
            return null;
        }
    }
}
