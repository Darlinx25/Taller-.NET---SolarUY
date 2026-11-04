namespace SolarUY.Application.Seguridad;

/// <summary>
/// Opciones de configuración del proveedor de identidad OpenID Connect.
/// </summary>
public sealed class OpcionesOidc
{
    public const string Seccion = "Authentication:Oidc";

    public string? Authority { get; set; }

    public string? MetadataAddress { get; set; }

    public string? BackchannelBaseUrl { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string CallbackPath { get; set; } = "/signin-oidc";

    public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";

    public string PostLogoutRedirectUri { get; set; } = "/";

    public bool RequireHttpsMetadata { get; set; } = true;

    public bool EsConfigurado =>
        !string.IsNullOrWhiteSpace(Authority) && !string.IsNullOrWhiteSpace(ClientId);
}
