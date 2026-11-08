namespace SolarUY.Simulador.Simulation;

/// <summary>
/// Parámetros de la simulación (sección <c>Simulador</c>). Se validan al arrancar.
/// </summary>
public class ConfiguracionSimulador
{
    public const string Seccion = "Simulador";

    public int IntervaloSegundos { get; set; } = 5;

    /// <summary>
    /// Probabilidad de que un panel operativo falle en cada lectura.
    /// </summary>
    public decimal ProbabilidadFalla { get; set; } = 0.20m;

    /// <summary>
    /// Probabilidad de que un panel en falla se recupere en cada lectura. La duración esperada de una falla
    /// es <c>1 / ProbabilidadRecuperacion</c> lecturas.
    /// </summary>

    public decimal ProbabilidadRecuperacion { get; set; } = 0.40m;

    public decimal VariacionMaxima { get; set; } = 0.10m;

    public int DesfaseHorarioUruguayHoras { get; set; } = -3;

    public int IntentosEnvio { get; set; } = 3;

    public int EsperaEntreIntentosSegundos { get; set; } = 2;

    /// <summary>
    /// Devuelve los errores de configuración; la lista vacía significa configuración válida.
    /// </summary>
    public IReadOnlyList<string> Validar()
    {
        var errores = new List<string>();

        if (IntervaloSegundos <= 0)
        {
            errores.Add("Simulador:IntervaloSegundos debe ser mayor a 0.");
        }

        if (ProbabilidadFalla is < 0 or > 1)
        {
            errores.Add("Simulador:ProbabilidadFalla debe estar entre 0 y 1.");
        }

        if (ProbabilidadRecuperacion is < 0 or > 1)
        {
            errores.Add("Simulador:ProbabilidadRecuperacion debe estar entre 0 y 1.");
        }

        if (VariacionMaxima is < 0 or > 1)
        {
            errores.Add("Simulador:VariacionMaxima debe estar entre 0 y 1.");
        }

        if (DesfaseHorarioUruguayHoras is < -12 or > 12)
        {
            errores.Add("Simulador:DesfaseHorarioUruguayHoras debe estar entre -12 y 12.");
        }

        if (IntentosEnvio < 1)
        {
            errores.Add("Simulador:IntentosEnvio debe ser al menos 1.");
        }

        if (EsperaEntreIntentosSegundos < 0)
        {
            errores.Add("Simulador:EsperaEntreIntentosSegundos no puede ser negativa.");
        }

        return errores;
    }
}
