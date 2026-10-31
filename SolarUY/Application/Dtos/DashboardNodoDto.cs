using System;
using System.Collections.Generic;

namespace SolarUY.Application.Dtos;

public class DashboardNodoDto
{
    public Guid Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public decimal Generacion { get; set; }

    public IReadOnlyList<DashboardPanelDto> Paneles { get; set; } = Array.Empty<DashboardPanelDto>();
}
