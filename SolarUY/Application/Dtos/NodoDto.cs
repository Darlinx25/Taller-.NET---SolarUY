using System;
using System.ComponentModel.DataAnnotations;
using SolarUY.Domain.Enums;

namespace SolarUY.Application.Dtos;

public class NodoDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El código del nodo es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    public Guid ParqueSolarId { get; set; }

    [StringLength(200)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Display(Name = "Estado")]
    public EstadoNodo Estado { get; set; } = EstadoNodo.Activo;

    public DateTime FechaCreacion { get; set; }

    public InversorDto? Inversor { get; set; }
}
