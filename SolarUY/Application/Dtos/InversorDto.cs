using System;
using System.ComponentModel.DataAnnotations;

namespace SolarUY.Application.Dtos;

public class InversorDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "La marca del inversor es obligatoria.")]
    [StringLength(100)]
    [Display(Name = "Marca")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo del inversor es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Modelo")]
    public string Modelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de serie del inversor es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Número de serie")]
    public string NumeroSerie { get; set; } = string.Empty;

    [Range(0, 1000000, ErrorMessage = "La potencia nominal no puede ser negativa.")]
    [Display(Name = "Potencia nominal (kW)")]
    public decimal? PotenciaNominalkW { get; set; }
}
