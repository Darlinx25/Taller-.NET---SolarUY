using Microsoft.Extensions.Options;
using SolarUY.Simulador.Simulation;
using SolarUY.Simulador.Transport;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<ConfiguracionSimulador>()
    .Bind(builder.Configuration.GetSection(ConfiguracionSimulador.Seccion))
    .Validate(c => c.IntervaloSegundos > 0, "Simulador:IntervaloSegundos debe ser mayor a 0.")
    .Validate(c => c.ProbabilidadFalla is >= 0 and <= 1, "Simulador:ProbabilidadFalla debe estar entre 0 y 1.")
    .Validate(c => c.DuracionFallaTicks > 0, "Simulador:DuracionFallaTicks debe ser mayor a 0.")
    .Validate(c => c.VariacionMaxima is >= 0 and <= 1, "Simulador:VariacionMaxima debe estar entre 0 y 1.")
    .Validate(
        c => c.DesfaseHorarioUruguayHoras is >= -12 and <= 12,
        "Simulador:DesfaseHorarioUruguayHoras debe estar entre -12 y 12.")
    .Validate(c => c.IntentosEnvio >= 1, "Simulador:IntentosEnvio debe ser al menos 1.")
    .Validate(
        c => c.EsperaEntreIntentosSegundos >= 0,
        "Simulador:EsperaEntreIntentosSegundos no puede ser negativa.")
    .ValidateOnStart();

builder.Services.AddOptions<ConfiguracionSimulacionApi>()
    .Bind(builder.Configuration.GetSection(ConfiguracionSimulacionApi.Seccion))
    .Validate(
        c => Uri.TryCreate(c.UrlBase, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
        "SimulacionApi:UrlBase debe ser una URL absoluta HTTP/HTTPS.")
    .Validate(c => !string.IsNullOrWhiteSpace(c.ApiKey), "SimulacionApi:ApiKey es obligatoria.")
    .Validate(c => c.TimeoutSegundos > 0, "SimulacionApi:TimeoutSegundos debe ser mayor a 0.")
    .ValidateOnStart();

builder.Services.AddHttpClient<IProveedorTopologia, ClienteTopologia>(ConfigurarCliente)
    .ConfigurePrimaryHttpMessageHandler(CrearManejador);
builder.Services.AddHttpClient<ClienteTelemetria>(ConfigurarCliente)
    .ConfigurePrimaryHttpMessageHandler(CrearManejador);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<SimuladorInversor>();
builder.Services.AddHostedService<SimuladorTelemetriaHostedService>();

var host = builder.Build();
await host.RunAsync();

static void ConfigurarCliente(IServiceProvider proveedor, HttpClient cliente)
{
    var configuracion = proveedor
        .GetRequiredService<IOptions<ConfiguracionSimulacionApi>>()
        .Value;

    cliente.BaseAddress = new Uri(configuracion.UrlBase.TrimEnd('/') + "/");
    cliente.Timeout = TimeSpan.FromSeconds(configuracion.TimeoutSegundos);
}

// Los clientes tipados quedan capturados por singletons: se limita la vida de las conexiones
// para que se re-resuelva el DNS de `app` (o del balanceador) cuando cambie de dirección.
static HttpMessageHandler CrearManejador() =>
    new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) };
