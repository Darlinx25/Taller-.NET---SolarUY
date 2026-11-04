using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SolarUY.Application.Seguridad;

namespace SolarUY.Controllers;

/// <summary>
/// Punto de entrada de autenticación: inicia el flujo OIDC y cierra la sesión local y remota.
/// </summary>
[AllowAnonymous]
public class AuthController : Controller
{
    private readonly OpcionesOidc _opcionesOidc;

    /// <summary>
    /// Crea el controlador de autenticación.
    /// </summary>
    /// <param name="opcionesOidc">Opciones del proveedor OpenID Connect.</param>
    public AuthController(IOptions<OpcionesOidc> opcionesOidc)
    {
        _opcionesOidc = opcionesOidc?.Value ?? throw new ArgumentNullException(nameof(opcionesOidc));
    }

    /// <summary>
    /// Inicia el flujo de autenticación OIDC o el acceso de desarrollo si no hay IdP configurado.
    /// </summary>
    /// <param name="returnUrl">Url local a la que volver después del login.</param>
    /// <returns>Desafío OIDC o redirección al stub de desarrollo.</returns>
    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        var destino = EsUrlLocal(returnUrl)
            ? returnUrl!
            : Url.Action(nameof(HomeController.Index), "Home") ?? "/";

        if (!_opcionesOidc.EsConfigurado)
        {
            return RedirectToAction(
                nameof(DevAuthController.Login),
                "DevAuth",
                new { returnUrl = destino });
        }

        return Challenge(
            new AuthenticationProperties { RedirectUri = destino },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Cierra la sesión local y, cuando hay IdP configurado, también la sesión remota.
    /// </summary>
    /// <returns>Redirección al inicio o al proveedor de identidad.</returns>
    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        var propiedades = new AuthenticationProperties
        {
            RedirectUri = _opcionesOidc.EsConfigurado
                ? _opcionesOidc.PostLogoutRedirectUri
                : "/"
        };

        if (!_opcionesOidc.EsConfigurado)
        {
            return SignOut(propiedades, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        return SignOut(
            propiedades,
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    private bool EsUrlLocal(string? returnUrl)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl);
    }
}
