namespace SolarUY.Simulador.Contracts;

public class TelemetriaParqueSolarDto
{
    public Guid ParqueSolarId { get; set; }

    public string NombreParque { get; set; } = string.Empty;

    public decimal PotenciaGeneradaW { get; set; }

    public decimal EnergiaAcumuladaKWh { get; set; }

    public DateTime FechaHora { get; set; }

    public IReadOnlyList<TelemetriaNodoDto> Nodos { get; set; } = Array.Empty<TelemetriaNodoDto>();
}
