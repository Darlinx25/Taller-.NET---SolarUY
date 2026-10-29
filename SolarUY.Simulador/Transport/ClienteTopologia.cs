using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarUY.Simulador.Contracts;
using SolarUY.Simulador.Simulation;

namespace SolarUY.Simulador.Transport;

/// <summary>
/// Obtiene la topología registrada con <c>GET api/simulacion/topologia</c>. Cualquier fallo
/// (respuesta no exitosa, transporte, timeout o JSON inválido) se registra y devuelve <c>null</c>.
/// </summary>
public class ClienteTopologia : IProveedorTopologia
{
    private const string Ruta = "api/simulacion/topologia";

    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ClienteTopologia> _logger;

    public ClienteTopologia(
        HttpClient httpClient,
        IOptions<ConfiguracionSimulacionApi> configuracion,
        ILogger<ClienteTopologia> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        ArgumentNullException.ThrowIfNull(configuracion);
        _apiKey = configuracion.Value.ApiKey;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TopologiaSimulacionDto?> ObtenerAsync(CancellationToken cancellationToken)
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
                    "No se pudo obtener la topología: respuesta {Codigo}. {Fragmento}",
                    (int)respuesta.StatusCode,
                    fragmento);
                return null;
            }

            return await respuesta.Content
                .ReadFromJsonAsync<TopologiaSimulacionDto>(OpcionesJson, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException
                                   && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("No se pudo obtener la topología: {Motivo}", ex.Message);
            return null;
        }
    }
}
