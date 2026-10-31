using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

// Autenticación por cookies.
// El proveedor real (OpenIddict / OIDC) se incorpora en una iteración posterior;
// en Development hay un stub de acceso para el rol Administrador.
// Los endpoints internos del simulador usan el esquema ApiKey, no la cookie.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/dev/login";
        options.LogoutPath = "/dev/logout";
        options.AccessDeniedPath = "/dev/denegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
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
