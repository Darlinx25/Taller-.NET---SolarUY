using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace SolarUY.Controllers;

/// <summary>
/// Stub de autenticación disponible únicamente en el entorno de desarrollo.
/// Permite firmar un usuario con rol Administrador sin depender de un proveedor
/// externo; la integración con OpenIddict / OIDC se realiza en una iteración posterior.
/// </summary>
[AllowAnonymous]
public class DevAuthController : Controller
{
    private const string RolAdministrador = "Administrador";

    private readonly IHostEnvironment _entorno;

    /// <summary>
    /// Crea el controlador de acceso de desarrollo.
    /// </summary>
    /// <param name="entorno">Entorno de ejecución de la aplicación.</param>
    public DevAuthController(IHostEnvironment entorno)
    {
        _entorno = entorno ?? throw new ArgumentNullException(nameof(entorno));
    }

    /// <summary>
    /// Firma un usuario de prueba con rol Administrador.
    /// </summary>
    /// <param name="returnUrl">Url local a la que redirigir tras el acceso.</param>
    /// <returns>Redirección al destino solicitado.</returns>
    [HttpGet("/dev/login")]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (!_entorno.IsDevelopment())
        {
            return NotFound();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "administrador@solaruy.uy"),
            new(ClaimTypes.Role, RolAdministrador),
        };

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identidad);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        var destino = EsUrlLocal(returnUrl)
            ? returnUrl!
            : Url.Action(nameof(ParquesSolaresController.Index), "ParquesSolares") ?? "/";

        return Redirect(destino);
    }

    /// <summary>
    /// Cierra la sesión del usuario de prueba.
    /// </summary>
    /// <returns>Redirección a la página de inicio.</returns>
    [HttpGet("/dev/logout")]
    public async Task<IActionResult> Logout()
    {
        if (!_entorno.IsDevelopment())
        {
            return NotFound();
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    /// <summary>
    /// Informa que el usuario no tiene permisos para la operación solicitada.
    /// </summary>
    /// <returns>Vista de acceso denegado.</returns>
    [HttpGet("/dev/denegado")]
    public IActionResult AccesoDenegado()
    {
        return View();
    }

    private bool EsUrlLocal(string? returnUrl)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl);
    }
}
