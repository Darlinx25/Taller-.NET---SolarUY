using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using SolarUY.Application.Seguridad;
using SolarUY.Application.Services;
using SolarUY.Domain.Repositories;
using SolarUY.Infrastructure.Persistence;
using SolarUY.Infrastructure.Repositories;
using SolarUY.Infrastructure.Telemetria;

var builder = WebApplication.CreateBuilder(args);

// Servicios de la capa de Presentación (MVC).
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

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
// el simulador (componente separado). La caché es en memoria; con Redis solo cambia esta línea.
builder.Services.AddDistributedMemoryCache();
builder.Services.Configure<ConfiguracionAlmacenTelemetria>(
    builder.Configuration.GetSection(ConfiguracionAlmacenTelemetria.Seccion));
builder.Services.Configure<ConfiguracionApiSimulacion>(
    builder.Configuration.GetSection(ConfiguracionApiSimulacion.Seccion));
builder.Services.AddSingleton<IAlmacenTelemetria, AlmacenTelemetriaCache>();
builder.Services.AddSingleton<IValidadorApiKey, ValidadorApiKey>();
builder.Services.AddSingleton<ValidadorTelemetria>();
builder.Services.AddScoped<ITelemetriaService, TelemetriaService>();
builder.Services.AddScoped<ITopologiaSimulacionService, TopologiaSimulacionService>();

// Dashboard de telemetría en vivo
builder.Services.AddSignalR();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITelemetriaNotifier, SolarUY.Hubs.SignalRTelemetriaNotifier>();

// Autenticación externa mediante OpenID Connect + sesión local con cookies.
// Los endpoints internos del simulador usan el esquema ApiKey, no la cookie.
var oidcAuthority = builder.Configuration["Authentication:Oidc:Authority"];
var oidcClientId = builder.Configuration["Authentication:Oidc:ClientId"];
var oidcConfigurado = !string.IsNullOrWhiteSpace(oidcAuthority) && !string.IsNullOrWhiteSpace(oidcClientId);

var authBuilder = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = oidcConfigurado
            ? OpenIdConnectDefaults.AuthenticationScheme
            : CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/dev/login";
        options.LogoutPath = "/dev/logout";
        options.AccessDeniedPath = "/dev/denegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

if (oidcConfigurado)
{
    authBuilder.AddOpenIdConnect("oidc", options =>
    {
        options.Authority = oidcAuthority;
        options.ClientId = oidcClientId;
        options.ClientSecret = builder.Configuration["Authentication:Oidc:ClientSecret"];
        options.CallbackPath = builder.Configuration["Authentication:Oidc:CallbackPath"] ?? "/signin-oidc";
        options.ResponseType = builder.Configuration["Authentication:Oidc:ResponseType"] ?? "code";
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity is null)
                {
                    return Task.CompletedTask;
                }

                string[] roleClaims = ["roles", "role", "groups", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
                foreach (var claimType in roleClaims)
                {
                    if (context.Principal?.HasClaim(c => c.Type == claimType) == true)
                    {
                        foreach (var claim in context.Principal.FindAll(claimType))
                        {
                            if (!claimsIdentity.HasClaim(ClaimTypes.Role, claim.Value))
                            {
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                            }
                        }
                    }
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
