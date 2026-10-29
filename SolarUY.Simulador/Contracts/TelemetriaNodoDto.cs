namespace SolarUY.Simulador.Contracts;

public class TelemetriaNodoDto
{
    public Guid NodoId { get; set; }

    public string CodigoNodo { get; set; } = string.Empty;

    public decimal PotenciaGeneradaW { get; set; }

    public decimal EnergiaAcumuladaKWh { get; set; }

    public DateTime FechaHora { get; set; }

    public IReadOnlyList<TelemetriaPanelDto> Paneles { get; set; } = Array.Empty<TelemetriaPanelDto>();
}
