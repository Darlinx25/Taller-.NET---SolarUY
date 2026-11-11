namespace SolarUY.Application.Dtos;

public class DashboardTendenciaEnergiaDto
{
    public IReadOnlyList<PuntoGraficoHistoricoDto> Puntos { get; set; } =
        Array.Empty<PuntoGraficoHistoricoDto>();

    public bool TieneDatos { get; set; }
}
