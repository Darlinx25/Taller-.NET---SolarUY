using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SolarUY.Application.Dtos;

public class CambiarEstadoTareaMantenimientoDto
{
    [Required]
    [JsonRequired]
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Nuevo estado")]
    public string NuevoEstado { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Quién completa")]
    public string? QuienCompleta { get; set; }
}
