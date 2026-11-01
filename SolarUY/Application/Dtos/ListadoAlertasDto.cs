using System;
using System.Collections.Generic;

namespace SolarUY.Application.Dtos;

public class ListadoAlertasDto
{
    public IReadOnlyList<AlertaPanelDto> Activas { get; set; } = Array.Empty<AlertaPanelDto>();

    public IReadOnlyList<AlertaPanelDto> Historial { get; set; } = Array.Empty<AlertaPanelDto>();
}
