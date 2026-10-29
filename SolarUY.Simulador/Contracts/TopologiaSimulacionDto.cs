namespace SolarUY.Simulador.Contracts;

/// <summary>
/// Contrato de <c>GET /api/simulacion/topologia</c>. Copia deliberada del DTO de la aplicación.
/// </summary>
public class TopologiaSimulacionDto
{
    public IReadOnlyList<ParqueSimulacionDto> Parques { get; set; } = Array.Empty<ParqueSimulacionDto>();
}
