using System;

namespace SolarUY.Application.Dtos;

public class PanelDetalleDto
{
    public Guid Id { get; set; }

    public Guid NodoId { get; set; }

    public string NodoCodigo { get; set; } = string.Empty;

    public Guid ParqueSolarId { get; set; }

    public string? ParqueSolarNombre { get; set; }

    public string CodigoSerie { get; set; } = string.Empty;

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public decimal PotenciaWp { get; set; }

    public DateTime? FechaInstalacion { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; }
}
