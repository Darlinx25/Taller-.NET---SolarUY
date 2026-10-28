using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SolarUY.Application.Dtos;

/// <summary>
/// Datos de entrada para la edición de un parque solar existente.
/// Las validaciones se aplican automáticamente vía ModelState en MVC.
/// </summary>
public class EditarParqueSolarDto
{
    /// <summary>
    /// Identificador del parque solar a editar.
    /// </summary>
    [Required]
    [JsonRequired]
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre único del parque solar.
    /// </summary>
    [Required(ErrorMessage = "El nombre del parque solar es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Ubicación geográfica o dirección del parque solar.
    /// </summary>
    [Required(ErrorMessage = "La ubicación del parque solar es obligatoria.")]
    [StringLength(200, ErrorMessage = "La ubicación no puede superar los 200 caracteres.")]
    [Display(Name = "Ubicación")]
    public string Ubicacion { get; set; } = string.Empty;

    /// <summary>
    /// Capacidad total estimada en megavatios. Es opcional.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "La capacidad estimada no puede ser negativa.")]
    [Display(Name = "Capacidad estimada (MW)")]
    public decimal? CapacidadEstimadaMW { get; set; }
}
