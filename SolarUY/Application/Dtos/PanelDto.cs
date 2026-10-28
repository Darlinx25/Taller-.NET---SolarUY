using System;

namespace SolarUY.Application.Dtos;

public class PanelDto
{
    public Guid Id { get; set; }

    public Guid NodoId { get; set; }

    public string CodigoSerie { get; set; } = string.Empty;

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public decimal PotenciaWp { get; set; }

    public DateTime? FechaInstalacion { get; set; }

    public DateTime FechaCreacion { get; set; }
}
