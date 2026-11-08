using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarUY.Simulador.Simulation;
using SolarUY.Simulador.Transport;
using SolarUY.Simulador.Wasm;

// Mock del inversor y los sensores ARM, compilado a WebAssembly. La configuración llega por variables de
// entorno (SimulacionApi__UrlBase, SimulacionApi__ApiKey, Simulador__IntervaloSegundos, ...).
var configuracion = new ConfigurationBuilder().AddEnvironmentVariables().Build();

var opcionesSimulador = configuracion.GetSection(ConfiguracionSimulador.Seccion).Get<ConfiguracionSimulador>()
    ?? new ConfiguracionSimulador();
var opcionesApi = configuracion.GetSection(ConfiguracionSimulacionApi.Seccion).Get<ConfiguracionSimulacionApi>()
    ?? new ConfiguracionSimulacionApi();

using var fabricaLogs = LoggerFactory.Create(logging => logging
    .SetMinimumLevel(LogLevel.Information)
    .AddProvider(new ProveedorLoggerConsola()));
var log = fabricaLogs.CreateLogger("SolarUY.Simulador");

var errores = opcionesSimulador.Validar().Concat(opcionesApi.Validar()).ToList();
if (errores.Count > 0)
{
    foreach (var error in errores)
    {
        log.LogCritical("Configuración inválida: {Error}", error);
    }

    return 1;
}

var baseApi = new Uri(opcionesApi.UrlBase.TrimEnd('/') + "/");
var timeout = TimeSpan.FromSeconds(opcionesApi.TimeoutSegundos);
var opcionesApiEnvueltas = Options.Create(opcionesApi);

using var httpTopologia = new HttpClient { BaseAddress = baseApi, Timeout = timeout };
using var httpTelemetria = new HttpClient { BaseAddress = baseApi, Timeout = timeout };

var proveedorTopologia = new ClienteTopologia(
    httpTopologia,
    opcionesApiEnvueltas,
    fabricaLogs.CreateLogger<ClienteTopologia>());
var clienteTelemetria = new ClienteTelemetria(
    httpTelemetria,
    opcionesApiEnvueltas,
    fabricaLogs.CreateLogger<ClienteTelemetria>());

using var httpControl = new HttpClient { BaseAddress = baseApi, Timeout = timeout };
var proveedorControl = new ClienteControl(
    httpControl,
    opcionesApiEnvueltas,
    fabricaLogs.CreateLogger<ClienteControl>());

var opcionesSimuladorEnvueltas = Options.Create(opcionesSimulador);
var simulador = new SimuladorInversor(
    proveedorTopologia,
    opcionesSimuladorEnvueltas,
    TimeProvider.System,
    fabricaLogs.CreateLogger<SimuladorInversor>(),
    proveedorControl);

var ciclo = new CicloTelemetria(
    simulador,
    clienteTelemetria,
    opcionesSimuladorEnvueltas,
    fabricaLogs.CreateLogger<CicloTelemetria>());

log.LogInformation(
    "Placa ARM simulada (WebAssembly) enviando a {UrlBase}. Por panel y lectura: probabilidad de falla {Falla}, de recuperación {Recuperacion}.",
    opcionesApi.UrlBase,
    opcionesSimulador.ProbabilidadFalla,
    opcionesSimulador.ProbabilidadRecuperacion);

await ciclo.EjecutarAsync(CancellationToken.None);
return 0;
