namespace SolarUY.Simulador.Contracts;

/// <summary>
/// Orden vigente que la aplicación le da al simulador (<c>GET api/simulacion/control</c>).
/// Copia deliberada del contrato de la aplicación: es el único acoplamiento entre ambos componentes.
/// </summary>
public class ControlSimulacionDto
{
    /// <summary>
    /// Si es <c>true</c>, el simulador genera como a pleno sol sin importar la hora.
    /// </summary>
    public bool SimulacionForzada { get; set; }

    /// <summary>
    /// Contador monótono: cada pedido de "simular falla" lo incrementa.
    /// </summary>
    public long SecuenciaFalla { get; set; }
}
