namespace SolarUY.Simulador.Simulation;

/// <summary>
/// Parámetros de la simulación (sección <c>Simulador</c>). Se validan al arrancar.
/// </summary>
public class ConfiguracionSimulador
{
    public const string Seccion = "Simulador";

    public int IntervaloSegundos { get; set; } = 5;

    public decimal ProbabilidadFalla { get; set; } = 0.05m;

    public int DuracionFallaTicks { get; set; } = 6;

    public decimal VariacionMaxima { get; set; } = 0.10m;

    public int DesfaseHorarioUruguayHoras { get; set; } = -3;

    public int IntentosEnvio { get; set; } = 3;

    public int EsperaEntreIntentosSegundos { get; set; } = 2;
}
