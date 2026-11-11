namespace SolarUY.Application.Dtos;

public class DashboardDistribucionEstadosDto
{
    public int Operativos { get; set; }

    public int EnFalla { get; set; }

    public int SinTelemetria { get; set; }

    public int TotalPaneles { get; set; }
}
