using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SolarUY.Domain.Entities;

namespace SolarUY.Application.Dtos;

public class EditarPanelDto
{
    [Required]
    [JsonRequired]
    public Guid Id { get; set; }

    [Required]
    [JsonRequired]
    public Guid NodoId { get; set; }

    [Required(ErrorMessage = "El código/serie del panel es obligatorio.")]
    [StringLength(Panel.CodigoSerieMaxLength, ErrorMessage = "El código/serie no puede superar los 50 caracteres.")]
    [Display(Name = "Código/Serie")]
    public string CodigoSerie { get; set; } = string.Empty;

    [Required(ErrorMessage = "La marca del panel es obligatoria.")]
    [StringLength(Panel.MarcaMaxLength, ErrorMessage = "La marca no puede superar los 100 caracteres.")]
    [Display(Name = "Marca")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo del panel es obligatorio.")]
    [StringLength(Panel.ModeloMaxLength, ErrorMessage = "El modelo no puede superar los 100 caracteres.")]
    [Display(Name = "Modelo")]
    public string Modelo { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999999999.99", ErrorMessage = "La potencia debe ser mayor a 0.")]
    [Display(Name = "Potencia (Wp)")]
    public decimal PotenciaWp { get; set; }

    [Display(Name = "Fecha de instalación")]
    [DataType(DataType.Date)]
    public DateTime? FechaInstalacion { get; set; }

    [StringLength(Panel.ObservacionesMaxLength, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }
}
