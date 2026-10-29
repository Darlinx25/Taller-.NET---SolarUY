using System;

namespace SolarUY.Application.Dtos;

public class PanelSimulacionDto
{
    public Guid PanelId { get; set; }

    public string CodigoSerie { get; set; } = string.Empty;

    public decimal PotenciaWp { get; set; }
}
