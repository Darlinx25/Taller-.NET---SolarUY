using System;
using System.Collections.Generic;
using System.Linq;
using SolarUY.Application.Dtos;

namespace SolarUY.Models;

public class ParqueSolarDetalleViewModel
{
    public ParqueSolarDto Parque { get; set; } = new();

    public IReadOnlyList<NodoConPanelesViewModel> Nodos { get; set; } = Array.Empty<NodoConPanelesViewModel>();

    public int CantidadNodos => Nodos.Count;

    public int CantidadPaneles => Nodos.Sum(nodo => nodo.CantidadPaneles);

    public decimal PotenciaTotalWp => Nodos.Sum(nodo => nodo.PotenciaTotalWp);
}
