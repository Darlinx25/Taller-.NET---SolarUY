namespace SolarUY.Simulador.Transport;

/// <summary>
/// Cómo llega el simulador a la API interna de la aplicación (sección <c>SimulacionApi</c>).
/// Una única URL base: la del balanceador de carga.
/// </summary>
public class ConfiguracionSimulacionApi
{
    public const string Seccion = "SimulacionApi";

    public const string EncabezadoApiKey = "X-Api-Key";

    public string UrlBase { get; set; } = "http://nginx:80";

    public string ApiKey { get; set; } = string.Empty;

    public int TimeoutSegundos { get; set; } = 10;

    /// <summary>
    /// Devuelve los errores de configuración; la lista vacía significa configuración válida.
    /// </summary>
    public IReadOnlyList<string> Validar()
    {
        var errores = new List<string>();

        if (!Uri.TryCreate(UrlBase, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            errores.Add("SimulacionApi:UrlBase debe ser una URL absoluta HTTP/HTTPS.");
        }

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            errores.Add("SimulacionApi:ApiKey es obligatoria.");
        }

        if (TimeoutSegundos <= 0)
        {
            errores.Add("SimulacionApi:TimeoutSegundos debe ser mayor a 0.");
        }

        return errores;
    }
}
