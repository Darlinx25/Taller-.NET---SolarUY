using System;

namespace SolarUY.Application.Dtos;

public class TelemetriaPanelDto
{
    public Guid PanelId { get; set; }

    public string CodigoSerie { get; set; } = string.Empty;

    public decimal PotenciaGeneradaW { get; set; }

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaHora { get; set; }
}
