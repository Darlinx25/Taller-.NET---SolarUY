using System;

namespace SolarUY.Application.Dtos;

public class TareaMantenimientoDetalleDto : TareaMantenimientoDto
{
    public string? PanelCodigoSerie { get; set; }
    public Guid? NodoId { get; set; }
    public string? NodoCodigo { get; set; }
    public Guid? ParqueSolarId { get; set; }
    public string? ParqueSolarNombre { get; set; }
}
