namespace SolarUY.Infrastructure.Telemetria;

/// <summary>
/// Opciones del almacén de la última lectura de telemetría (sección <c>TelemetriaAlmacen</c>).
/// </summary>
public class ConfiguracionAlmacenTelemetria
{
    public const string Seccion = "TelemetriaAlmacen";

    public string Clave { get; set; } = "simulacion:ultima-telemetria";

    /// <summary>
    /// Clave de la orden vigente para el simulador (simulación forzada y falla forzada).
    /// </summary>
    public string ClaveControl { get; set; } = "simulacion:control";

    public int VigenciaHoras { get; set; } = 24;
}
