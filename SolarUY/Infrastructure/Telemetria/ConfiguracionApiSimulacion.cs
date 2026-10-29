namespace SolarUY.Infrastructure.Telemetria;

/// <summary>
/// Opciones de la API interna de simulación (sección <c>SimulacionApi</c>).
/// La key autentica al componente simulador; nunca se escribe en logs ni en respuestas.
/// </summary>
public class ConfiguracionApiSimulacion
{
    public const string Seccion = "SimulacionApi";

    public string ApiKey { get; set; } = string.Empty;
}
