using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SolarUY.Application.Dtos;

public class EditarNodoInversorDto
{
    [Required]
    [JsonRequired]
    public Guid Id { get; set; }

    [Required]
    [JsonRequired]
    public Guid ParqueSolarId { get; set; }

    [Required(ErrorMessage = "El código del nodo es obligatorio.")]
    [StringLength(50, ErrorMessage = "El código no puede superar los 50 caracteres.")]
    [Display(Name = "Código del nodo")]
    public string Codigo { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El estado del nodo es obligatorio.")]
    [StringLength(50)]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "Activo";

    [Required]
    [JsonRequired]
    public Guid InversorId { get; set; }

    [Required(ErrorMessage = "La marca del inversor es obligatoria.")]
    [StringLength(100)]
    [Display(Name = "Marca")]
    public string InversorMarca { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo del inversor es obligatoria.")]
    [StringLength(100)]
    [Display(Name = "Modelo")]
    public string InversorModelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de serie del inversor es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Número de serie")]
    public string InversorNumeroSerie { get; set; } = string.Empty;

    [Range(0, 1000000, ErrorMessage = "La potencia nominal no puede ser negativa.")]
    [Display(Name = "Potencia nominal (kW)")]
    public decimal? InversorPotenciaNominalkW { get; set; }
}