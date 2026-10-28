using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SolarUY.Application.Dtos;

public class CrearTareaMantenimientoDto
{
    [Required]
    [JsonRequired]
    public Guid PanelId { get; set; }

    [Required(ErrorMessage = "El tipo de tarea es obligatorio")]
    [StringLength(100, ErrorMessage = "El tipo de tarea no puede superar los 100 caracteres")]
    [Display(Name = "Tipo de tarea")]
    public string TipoTarea { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Asignado a es obligatorio")]
    [StringLength(200, ErrorMessage = "Asignado a no puede superar los 200 caracteres")]
    [Display(Name = "Asignado a")]
    public string AsignadoA { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
    [Display(Name = "Fecha de vencimiento")]
    [DataType(DataType.Date)]
    public DateTime FechaVencimiento { get; set; } = DateTime.Today;

    [StringLength(1000, ErrorMessage = "Las observaciones no pueden superar los 1000 caracteres")]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }
}
