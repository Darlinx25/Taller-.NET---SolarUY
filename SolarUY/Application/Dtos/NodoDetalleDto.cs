using System;

namespace SolarUY.Application.Dtos;

public class NodoDetalleDto
{
    public Guid Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public Guid ParqueSolarId { get; set; }

    public string? ParqueSolarNombre { get; set; }

    public string? Descripcion { get; set; }

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    public InversorDto? Inversor { get; set; }
}
