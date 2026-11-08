using System;
using System.Collections.Generic;

namespace SolarUY.Application.Dtos;

public class DashboardTelemetriaViewDto
{
    public DateTime? FechaHora { get; set; }

    public IReadOnlyList<DashboardParqueDto> Parques { get; set; } = Array.Empty<DashboardParqueDto>();

    public bool TieneDatos { get; set; }

    /// <summary>
    /// Indica que el administrador inició la simulación forzada (el simulador genera como a pleno sol).
    /// </summary>
    public bool SimulacionForzadaActiva { get; set; }

    public string MensajeSinDatos { get; set; } = "Sin telemetría disponible";
}
