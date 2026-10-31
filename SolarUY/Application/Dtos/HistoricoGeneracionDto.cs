using System;
using System.Collections.Generic;

namespace SolarUY.Application.Dtos;

public class HistoricoGeneracionDto
{
    public IReadOnlyList<ResumenHistoricoDto> Resumenes { get; set; } = Array.Empty<ResumenHistoricoDto>();

    public IReadOnlyList<PuntoGraficoHistoricoDto> PuntosGrafico { get; set; } =
        Array.Empty<PuntoGraficoHistoricoDto>();

    public decimal EnergiaTotalKWh { get; set; }

    public decimal PotenciaPromedioW { get; set; }

    public decimal PotenciaMaximaW { get; set; }

    public int Fallas { get; set; }
}
