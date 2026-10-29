using System;
using System.Collections.Generic;

namespace SolarUY.Application.Dtos;

public class NodoSimulacionDto
{
    public Guid NodoId { get; set; }

    public string CodigoNodo { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public decimal? PotenciaNominalInversorKW { get; set; }

    public IReadOnlyList<PanelSimulacionDto> Paneles { get; set; } = Array.Empty<PanelSimulacionDto>();
}
