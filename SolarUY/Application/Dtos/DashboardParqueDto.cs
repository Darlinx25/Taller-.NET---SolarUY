using System;
using System.Collections.Generic;

namespace SolarUY.Application.Dtos;

public class DashboardParqueDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Ubicacion { get; set; } = string.Empty;

    public decimal GeneracionTotal { get; set; }

    public IReadOnlyList<DashboardNodoDto> Nodos { get; set; } = Array.Empty<DashboardNodoDto>();
}
