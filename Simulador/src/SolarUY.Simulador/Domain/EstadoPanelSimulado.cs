namespace SolarUY.Simulador.Domain;

/// <summary>
/// Estado de salud de un panel durante la simulación.
/// </summary>
public enum EstadoPanelSimulado
{
    /// <summary>
    /// El panel genera según la curva solar.
    /// </summary>
    Operativo,

    /// <summary>
    /// El panel no genera (0 W) hasta recuperarse.
    /// </summary>
    EnFalla
}
