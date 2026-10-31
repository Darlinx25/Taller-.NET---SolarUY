using SolarUY.Application.Dtos;

namespace SolarUY.Models;

public class HistoricoViewModel
{
    public FiltroHistoricoDto Filtro { get; set; } = new();

    public OpcionesHistoricoDto Opciones { get; set; } = new();

    public HistoricoGeneracionDto Resultado { get; set; } = new();
}
