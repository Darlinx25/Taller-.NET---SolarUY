using System;
using System.Collections.Generic;
using System.Linq;
using SolarUY.Application.Dtos;

namespace SolarUY.Models;

public class NodoDetalleViewModel
{
    public NodoDetalleDto Nodo { get; set; } = new();

    public IReadOnlyList<PanelDto> Paneles { get; set; } = Array.Empty<PanelDto>();

    public int CantidadPaneles => Paneles.Count;

    public decimal PotenciaTotalWp => Paneles.Sum(panel => panel.PotenciaWp);
}
