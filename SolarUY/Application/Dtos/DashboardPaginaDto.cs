namespace SolarUY.Application.Dtos;

public class DashboardPaginaDto
{
    public DashboardTelemetriaViewDto Dashboard { get; set; } = new();

    public DashboardTendenciaEnergiaDto Tendencia { get; set; } = new();
}
