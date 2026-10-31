using System;
using System.ComponentModel.DataAnnotations;
using SolarUY.Domain.Enums;

namespace SolarUY.Application.Dtos;

public class FiltroHistoricoDto
{
    [Required(ErrorMessage = "El tipo de entidad es obligatorio.")]
    [Display(Name = "Tipo de entidad")]
    public TipoEntidadHistorico TipoEntidad { get; set; } = TipoEntidadHistorico.Parque;

    [Display(Name = "Entidad")]
    public Guid? EntidadId { get; set; }

    [Required(ErrorMessage = "La fecha desde es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Desde")]
    public DateOnly? Desde { get; set; }

    [Required(ErrorMessage = "La fecha hasta es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Hasta")]
    public DateOnly? Hasta { get; set; }

    [Required(ErrorMessage = "La agrupación es obligatoria.")]
    [Display(Name = "Agrupación")]
    public AgrupacionHistorico Agrupacion { get; set; } = AgrupacionHistorico.Dia;
}
