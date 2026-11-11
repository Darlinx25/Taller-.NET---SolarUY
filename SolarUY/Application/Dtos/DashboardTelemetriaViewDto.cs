using System;
using System.Collections.Generic;

namespace SolarUY.Application.Dtos;

public class DashboardTelemetriaViewDto
{
    public DateTime? FechaHora { get; set; }

    public IReadOnlyList<DashboardParqueDto> Parques { get; set; } = Array.Empty<DashboardParqueDto>();

    public IReadOnlyList<DashboardBarraDto> GeneracionPorParque { get; set; } =
        Array.Empty<DashboardBarraDto>();

    public IReadOnlyList<DashboardBarraDto> GeneracionPorNodo { get; set; } =
        Array.Empty<DashboardBarraDto>();

    public DashboardDistribucionEstadosDto DistribucionEstadosPaneles { get; set; } = new();

    public bool TieneDatos { get; set; }

    /// <summary>
    /// Indica que el administrador inició la simulación forzada (el simulador genera como a pleno sol).
    /// </summary>
    public bool SimulacionForzadaActiva { get; set; }

    public string MensajeSinDatos { get; set; } = "Sin telemetría disponible";
}
