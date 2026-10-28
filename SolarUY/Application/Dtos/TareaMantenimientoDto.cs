using System;

namespace SolarUY.Application.Dtos;

public class TareaMantenimientoDto
{
    public Guid Id { get; set; }
    public Guid PanelId { get; set; }
    public string TipoTarea { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string AsignadoA { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFinalizacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string QuienAsigna { get; set; } = string.Empty;
    public string? QuienCompleta { get; set; }
    public string? Observaciones { get; set; }
}
