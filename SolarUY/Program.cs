using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using SolarUY.Application.Seguridad;
using SolarUY.Application.Services;
using SolarUY.Domain.Repositories;
using SolarUY.Infrastructure.Persistence;
using SolarUY.Infrastructure.Repositories;
using SolarUY.Infrastructure.Seguridad;
using SolarUY.Infrastructure.Telemetria;

var builder = WebApplication.CreateBuilder(args);

// Servicios de la capa de Presentación (MVC).
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Servidor MCP de consultas (solo lectura) para agentes externos como Antigravity.
builder.Services.AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true)
    .WithTools<SolarUY.Controllers.Mcp.SolarUyMcpTools>();

// Capa de Infraestructura: base de datos MySQL 8.4.
var cadenaConexion = builder.Configuration.GetConnectionString("SolarUYDb");
if (string.IsNullOrWhiteSpace(cadenaConexion))
{
    throw new InvalidOperationException(
        "Falta configurar la cadena de conexión 'SolarUYDb' (ConnectionStrings:SolarUYDb).");
}

builder.Services.AddDbContext<SolarUYDbContext>(options =>
    options.UseMySQL(cadenaConexion));

// Inyección de dependencias: la Presentación conoce Application, nunca Infrastructure.
builder.Services.AddScoped<IParqueSolarRepository, ParqueSolarRepository>();
builder.Services.AddScoped<IParqueSolarService, ParqueSolarService>();
builder.Services.AddScoped<INodoRepository, NodoRepository>();
builder.Services.AddScoped<INodoService, NodoService>();
builder.Services.AddScoped<IPanelRepository, PanelRepository>();
builder.Services.AddScoped<IPanelService, PanelService>();
builder.Services.AddScoped<ITareaMantenimientoRepository, TareaMantenimientoRepository>();
builder.Services.AddScoped<ITareaMantenimientoService, TareaMantenimientoService>();

// Históricos de generación: resúmenes diarios persistidos a partir de la telemetría.
builder.Services.AddOptions<ConfiguracionHistorico>()
    .Bind(builder.Configuration.GetSection(ConfiguracionHistorico.Seccion))
    .Validate(
        configuracion => configuracion.BrechaMaximaSegundos > 0,
        "Historico:BrechaMaximaSegundos debe ser mayor a 0.")
    .Validate(
        configuracion => configuracion.DesfaseHorarioUruguayHoras is >= -12 and <= 12,
        "Historico:DesfaseHorarioUruguayHoras debe estar entre -12 y 12.")
    .ValidateOnStart();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IHistoricoRepository, HistoricoRepository>();
builder.Services.AddScoped<IRegistroHistoricoService, RegistroHistoricoService>();
builder.Services.AddScoped<IHistoricoService, HistoricoService>();

// Alertas por fallos de paneles: se detectan en la ingesta de telemetría y se avisan en vivo por SignalR.
builder.Services.AddScoped<IAlertaPanelRepository, AlertaPanelRepository>();
builder.Services.AddScoped<IRegistroAlertasService, RegistroAlertasService>();
builder.Services.AddScoped<IAlertasService, AlertasService>();
builder.Services.AddScoped<IAlertasNotifier, SolarUY.Hubs.SignalRAlertasNotifier>();

// Telemetría del mock del inversor: la aplicación recibe, valida y conserva la última lectura que envía
// el simulador (componente separado). Con múltiples réplicas se usa Redis para estado compartido.
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    var redisMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redisMultiplexer);
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(redisMultiplexer, "SolarUY-DataProtection-Keys")
        .SetApplicationName("SolarUY");
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "SolarUY:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.Configure<ConfiguracionAlmacenTelemetria>(
    builder.Configuration.GetSection(ConfiguracionAlmacenTelemetria.Seccion));
builder.Services.Configure<ConfiguracionApiSimulacion>(
    builder.Configuration.GetSection(ConfiguracionApiSimulacion.Seccion));
builder.Services.AddSingleton<IAlmacenTelemetria, AlmacenTelemetriaCache>();
builder.Services.AddSingleton<IAlmacenControlSimulacion, AlmacenControlSimulacionCache>();
builder.Services.AddScoped<IControlSimulacionService, ControlSimulacionService>();
builder.Services.AddSingleton<IValidadorApiKey, ValidadorApiKey>();
builder.Services.AddSingleton<ValidadorTelemetria>();
builder.Services.AddScoped<ITelemetriaService, TelemetriaService>();
builder.Services.AddScoped<ITopologiaSimulacionService, TopologiaSimulacionService>();

// Dashboard de telemetría en vivo
var signalRBuilder = builder.Services.AddSignalR();
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    signalRBuilder.AddStackExchangeRedis(redisConnectionString, options =>
    {
        options.Configuration.ChannelPrefix = RedisChannel.Literal("SolarUY");
    });
}
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITelemetriaNotifier, SolarUY.Hubs.SignalRTelemetriaNotifier>();

// Autenticación externa mediante OpenID Connect + sesión local con cookies.
// Los endpoints internos del simulador usan el esquema ApiKey, no la cookie.
builder.Services.Configure<OpcionesOidc>(
    builder.Configuration.GetSection(OpcionesOidc.Seccion));
var opcionesOidc = builder.Configuration
    .GetSection(OpcionesOidc.Seccion)
    .Get<OpcionesOidc>() ?? new OpcionesOidc();
var oidcConfigurado = opcionesOidc.EsConfigurado;

Uri? authorityUri = null;
Uri? backchannelUri = null;
if (oidcConfigurado)
{
    if (!Uri.TryCreate(opcionesOidc.Authority, UriKind.Absolute, out authorityUri))
    {
        throw new InvalidOperationException(
            "Authentication:Oidc:Authority debe ser una URL absoluta.");
    }

    if (!string.IsNullOrWhiteSpace(opcionesOidc.BackchannelBaseUrl) &&
        !Uri.TryCreate(opcionesOidc.BackchannelBaseUrl, UriKind.Absolute, out backchannelUri))
    {
        throw new InvalidOperationException(
            "Authentication:Oidc:BackchannelBaseUrl debe ser una URL absoluta.");
    }

    if (backchannelUri is not null &&
        (string.IsNullOrWhiteSpace(opcionesOidc.ClientSecret) ||
         string.IsNullOrWhiteSpace(opcionesOidc.MetadataAddress)))
    {
        throw new InvalidOperationException(
            "Con Authentication:Oidc:BackchannelBaseUrl configurado se requieren " +
            "ClientSecret y MetadataAddress.");
    }
}

var authBuilder = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = oidcConfigurado
            ? OpenIdConnectDefaults.AuthenticationScheme
            : CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/dev/denegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        if (!builder.Environment.IsDevelopment())
        {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        }
    });

if (oidcConfigurado)
{
    authBuilder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = opcionesOidc.Authority;
        options.ClientId = opcionesOidc.ClientId;
        options.ClientSecret = opcionesOidc.ClientSecret;

        if (!string.IsNullOrWhiteSpace(opcionesOidc.MetadataAddress))
        {
            options.MetadataAddress = opcionesOidc.MetadataAddress;
        }

        options.CallbackPath = string.IsNullOrWhiteSpace(opcionesOidc.CallbackPath)
            ? "/signin-oidc"
            : opcionesOidc.CallbackPath;
        options.SignedOutCallbackPath = string.IsNullOrWhiteSpace(opcionesOidc.SignedOutCallbackPath)
            ? "/signout-callback-oidc"
            : opcionesOidc.SignedOutCallbackPath;
        options.SignedOutRedirectUri = string.IsNullOrWhiteSpace(opcionesOidc.PostLogoutRedirectUri)
            ? "/"
            : opcionesOidc.PostLogoutRedirectUri;
        options.ResponseType = "code";
        options.UsePkce = true;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.RequireHttpsMetadata = opcionesOidc.RequireHttpsMetadata;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("roles");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = opcionesOidc.ClientId,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };

        if (authorityUri is not null && backchannelUri is not null)
        {
            var backchannelHandler = new KeycloakBackchannelHandler(authorityUri, backchannelUri)
            {
                InnerHandler = new HttpClientHandler()
            };
            options.Backchannel = new HttpClient(backchannelHandler);
        }

        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identidad)
                {
                    MapeadorRolesKeycloak.AgregarRoles(
                        identidad,
                        context.Principal.Claims,
                        opcionesOidc.ClientId);
                }

                return Task.CompletedTask;
            }
        };
    });
}

authBuilder.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
    AutenticacionComponentes.EsquemaApiKey,
    null);

builder.Services.AddAuthorization();

var app = builder.Build();

if (string.IsNullOrWhiteSpace(app.Configuration[$"{ConfiguracionApiSimulacion.Seccion}:ApiKey"]))
{
    app.Logger.LogWarning(
        "SimulacionApi:ApiKey no está configurada: /api/simulacion/* responderá 401.");
}

// Configurar el pipeline de requests HTTP.
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapHub<SolarUY.Hubs.TelemetriaHub>("/hubs/telemetria");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Endpoint MCP de consultas en lenguaje natural (solo lectura, acceso anónimo).
if (app.Configuration.GetValue("Mcp:Habilitado", true))
{
    app.MapMcp(app.Configuration["Mcp:Ruta"] ?? "/mcp");
}

app.MapGet("/health", async (SolarUYDbContext contexto, CancellationToken cancellationToken) =>
{
    var baseDeDatosOperativa = await contexto.Database.CanConnectAsync(cancellationToken);
    return baseDeDatosOperativa
        ? Results.Ok(new { estado = "ok", baseDeDatos = "operativa" })
        : Results.Json(
            new { estado = "degradado", baseDeDatos = "no disponible" },
            statusCode: StatusCodes.Status503ServiceUnavailable);
});

await app.RunAsync();
