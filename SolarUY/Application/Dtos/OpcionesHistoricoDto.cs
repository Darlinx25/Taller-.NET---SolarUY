using System;
using System.Collections.Generic;

namespace SolarUY.Application.Dtos;

public class OpcionesHistoricoDto
{
    public IReadOnlyList<OpcionEntidadDto> Parques { get; set; } = Array.Empty<OpcionEntidadDto>();

    public IReadOnlyList<OpcionEntidadDto> Nodos { get; set; } = Array.Empty<OpcionEntidadDto>();

    public IReadOnlyList<OpcionEntidadDto> Paneles { get; set; } = Array.Empty<OpcionEntidadDto>();
}
