namespace SolarUY.Simulador.Contracts;

/// <summary>
/// Contrato de <c>POST /api/simulacion/telemetria</c>. Copia deliberada del DTO de la aplicación:
/// el contrato JSON es el único acoplamiento entre ambos componentes.
/// <see cref="FechaHora"/> es <c>null</c> cuando la lectura no contiene parques.
/// </summary>
public class TelemetriaDto
{
    public DateTime? FechaHora { get; set; }

    public IReadOnlyList<TelemetriaParqueSolarDto> Parques { get; set; } = Array.Empty<TelemetriaParqueSolarDto>();
}
