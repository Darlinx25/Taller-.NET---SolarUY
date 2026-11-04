using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;

namespace SolarUY.Infrastructure.Seguridad;

/// <summary>
/// Convierte los roles que publica Keycloak (claims directos, <c>realm_access</c> y
/// <c>resource_access</c>) en claims de rol estándar de ASP.NET Core.
/// </summary>
public static class MapeadorRolesKeycloak
{
    private static readonly string[] TiposDirectos =
    [
        "roles",
        "role",
        "groups",
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ];

    public static void AgregarRoles(ClaimsIdentity identidad, IEnumerable<Claim> claims, string? clientId)
    {
        ArgumentNullException.ThrowIfNull(identidad);
        ArgumentNullException.ThrowIfNull(claims);

        var roles = new HashSet<string>(StringComparer.Ordinal);
        foreach (var claim in claims)
        {
            if (Array.Exists(TiposDirectos, tipo => string.Equals(tipo, claim.Type, StringComparison.OrdinalIgnoreCase)))
            {
                AgregarRol(claim.Value, roles);
                continue;
            }

            if (string.Equals(claim.Type, "realm_access", StringComparison.OrdinalIgnoreCase))
            {
                AgregarRolesDesdeJson(claim.Value, roles);
                continue;
            }

            if (string.Equals(claim.Type, "resource_access", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(clientId))
            {
                AgregarRolesDeCliente(claim.Value, clientId, roles);
            }
        }

        foreach (var rol in roles)
        {
            if (!identidad.HasClaim(ClaimTypes.Role, rol))
            {
                identidad.AddClaim(new Claim(ClaimTypes.Role, rol));
            }
        }
    }

    private static void AgregarRolesDesdeJson(string json, ISet<string> roles)
    {
        try
        {
            using var documento = JsonDocument.Parse(json);
            if (documento.RootElement.TryGetProperty("roles", out var arreglo))
            {
                AgregarArreglo(arreglo, roles);
            }
        }
        catch (JsonException)
        {
        }
    }

    private static void AgregarRolesDeCliente(string json, string clientId, ISet<string> roles)
    {
        try
        {
            using var documento = JsonDocument.Parse(json);
            if (documento.RootElement.TryGetProperty(clientId, out var cliente) &&
                cliente.TryGetProperty("roles", out var arreglo))
            {
                AgregarArreglo(arreglo, roles);
            }
        }
        catch (JsonException)
        {
        }
    }

    private static void AgregarArreglo(JsonElement arreglo, ISet<string> roles)
    {
        if (arreglo.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var elemento in arreglo.EnumerateArray())
        {
            if (elemento.ValueKind == JsonValueKind.String)
            {
                AgregarRol(elemento.GetString(), roles);
            }
        }
    }

    private static void AgregarRol(string? valor, ISet<string> roles)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return;
        }

        var texto = valor.Trim();
        if (texto.StartsWith('[') && texto.EndsWith(']'))
        {
            try
            {
                using var documento = JsonDocument.Parse(texto);
                AgregarArreglo(documento.RootElement, roles);
                return;
            }
            catch (JsonException)
            {
            }
        }

        roles.Add(texto);
    }
}
