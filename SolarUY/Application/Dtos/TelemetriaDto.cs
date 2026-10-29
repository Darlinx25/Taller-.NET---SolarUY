using System;
using System.Collections.Generic;

namespace SolarUY.Application.Dtos;

/// <summary>
/// Última lectura de telemetría consolidada. Es el contrato de ingesta y de consulta.
/// <see cref="FechaHora"/> es <c>null</c> cuando la lectura no contiene parques.
/// </summary>
public class TelemetriaDto
{
    public DateTime? FechaHora { get; set; }

    public IReadOnlyList<TelemetriaParqueSolarDto> Parques { get; set; } = Array.Empty<TelemetriaParqueSolarDto>();
}
