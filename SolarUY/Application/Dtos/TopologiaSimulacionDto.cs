using System;
using System.Collections.Generic;

namespace SolarUY.Application.Dtos;

/// <summary>
/// Topología registrada (parques, nodos, inversores y paneles) que se entrega al simulador en modo solo lectura.
/// </summary>
public class TopologiaSimulacionDto
{
    public IReadOnlyList<ParqueSimulacionDto> Parques { get; set; } = Array.Empty<ParqueSimulacionDto>();
}
