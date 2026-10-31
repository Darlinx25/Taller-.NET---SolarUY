using System;
using System.Collections.Generic;

namespace SolarUY.Application.Dtos;

public class DashboardTelemetriaViewDto
{
    public DateTime? FechaHora { get; set; }

    public IReadOnlyList<DashboardParqueDto> Parques { get; set; } = Array.Empty<DashboardParqueDto>();

    public bool TieneDatos { get; set; }

    public string MensajeSinDatos { get; set; } = "Sin telemetría disponible";
}
