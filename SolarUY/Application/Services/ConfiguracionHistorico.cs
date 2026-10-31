namespace SolarUY.Application.Services;

public class ConfiguracionHistorico
{
    public const string Seccion = "Historico";

    public int BrechaMaximaSegundos { get; set; } = 15;

    public int DesfaseHorarioUruguayHoras { get; set; } = -3;
}
