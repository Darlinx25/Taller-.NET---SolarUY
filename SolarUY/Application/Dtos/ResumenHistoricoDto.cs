using System;
using SolarUY.Domain.Enums;

namespace SolarUY.Application.Dtos;

public class ResumenHistoricoDto
{
    public Guid EntidadId { get; set; }

    public string EntidadNombre { get; set; } = string.Empty;

    public string Periodo { get; set; } = string.Empty;

    public decimal EnergiaKWh { get; set; }

    public decimal PotenciaPromedioW { get; set; }

    public decimal PotenciaMaximaW { get; set; }

    public int Fallas { get; set; }

    public EstadoPanel UltimoEstado { get; set; }
}
