using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SolarUY.Application.Dtos;

public class EditarTareaMantenimientoDto
{
    [Required]
    [JsonRequired]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El tipo de tarea es obligatorio")]
    [StringLength(100)]
    [Display(Name = "Tipo de tarea")]
    public string TipoTarea { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(1000)]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Asignado a es obligatorio")]
    [StringLength(200)]
    [Display(Name = "Asignado a")]
    public string AsignadoA { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
    [Display(Name = "Fecha de vencimiento")]
    [DataType(DataType.Date)]
    public DateTime FechaVencimiento { get; set; }

    [StringLength(1000)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }
}
