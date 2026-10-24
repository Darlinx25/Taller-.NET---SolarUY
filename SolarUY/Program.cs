using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SolarUY.Application.Services;
using SolarUY.Domain.Repositories;
using SolarUY.Infrastructure.Persistence;
using SolarUY.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Servicios de la capa de Presentación (MVC).
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

// Autenticación por cookies.
// El proveedor real (OpenIddict / OIDC) se incorpora en una iteración posterior;
// en Development hay un stub de acceso para el rol Administrador.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/DevAuth/Login";
        options.AccessDeniedPath = "/DevAuth/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

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
