using System;
using System.Collections.Generic;

namespace SolarUY.Application.Dtos;

public class ParqueSimulacionDto
{
    public Guid ParqueSolarId { get; set; }

    public string NombreParque { get; set; } = string.Empty;

    public IReadOnlyList<NodoSimulacionDto> Nodos { get; set; } = Array.Empty<NodoSimulacionDto>();
}
