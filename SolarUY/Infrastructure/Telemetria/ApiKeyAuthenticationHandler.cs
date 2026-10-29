using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarUY.Application.Seguridad;
using SolarUY.Application.Services;

namespace SolarUY.Infrastructure.Telemetria;

/// <summary>
/// Autentica al componente simulador con la API key del encabezado <c>X-Api-Key</c>.
/// No usa cookie: un fallo responde 401 y no redirige al login.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string Componente = "simulador";

    private readonly IValidadorApiKey _validadorApiKey;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> opciones,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IValidadorApiKey validadorApiKey)
        : base(opciones, logger, encoder)
    {
        _validadorApiKey = validadorApiKey ?? throw new ArgumentNullException(nameof(validadorApiKey));
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var apiKey = Request.Headers[AutenticacionComponentes.EncabezadoApiKey].ToString();
        if (!_validadorApiKey.EsValida(apiKey))
            return Task.FromResult(AuthenticateResult.Fail("API key inválida."));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, Componente),
            new Claim("componente", Componente)
        };
        var identidad = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identidad), Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
