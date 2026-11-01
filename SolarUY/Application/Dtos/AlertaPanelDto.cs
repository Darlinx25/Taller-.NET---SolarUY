using System;

namespace SolarUY.Application.Dtos;

public class AlertaPanelDto
{
    public Guid Id { get; set; }

    public string NombreParque { get; set; } = string.Empty;

    public string CodigoNodo { get; set; } = string.Empty;

    public string CodigoSerie { get; set; } = string.Empty;

    public DateTime FechaInicioUtc { get; set; }

    public DateTime? FechaFinUtc { get; set; }

    public bool EstaActiva { get; set; }

    public TimeSpan Duracion { get; set; }
}
