using System;

namespace SolarUY.Application.Dtos;

public class DashboardPanelDto
{
    public Guid Id { get; set; }

    public string CodigoSerie { get; set; } = string.Empty;

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public decimal PotenciaWp { get; set; }

    public decimal Generacion { get; set; }

    public string EstadoSalud { get; set; } = string.Empty;

    public bool EsFallo { get; set; }
}
