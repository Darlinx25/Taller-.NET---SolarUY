namespace SolarUY.Simulador.Transport;

/// <summary>
/// Cómo llega el simulador a la API interna de la aplicación (sección <c>SimulacionApi</c>).
/// Una única URL base: hoy la de la aplicación, mañana la del balanceador.
/// </summary>
public class ConfiguracionSimulacionApi
{
    public const string Seccion = "SimulacionApi";

    public const string EncabezadoApiKey = "X-Api-Key";

    public string UrlBase { get; set; } = "http://app:8080";

    public string ApiKey { get; set; } = string.Empty;

    public int TimeoutSegundos { get; set; } = 10;
}
