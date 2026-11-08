namespace SolarUY.Application.Dtos;

/// <summary>
/// Orden vigente para el simulador (mock del inversor): lo que el simulador lee con
/// <c>GET api/simulacion/control</c> y lo que el administrador cambia desde el dashboard.
/// </summary>
public class ControlSimulacionDto
{
    /// <summary>
    /// Si es <c>true</c>, el simulador genera como a pleno sol sin importar la hora.
    /// </summary>
    public bool SimulacionForzada { get; set; }

    /// <summary>
    /// Contador monótono: cada pedido de "simular falla" lo incrementa. El simulador aplica la falla
    /// cuando ve una secuencia mayor que la última que procesó.
    /// </summary>
    public long SecuenciaFalla { get; set; }
}
