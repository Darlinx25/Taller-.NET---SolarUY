namespace SolarUY.Simulador.Domain;

/// <summary>
/// Calcula la generación de un panel según una curva solar diaria: 0 W fuera de la ventana
/// de 07:00 a 19:00 y un pico en la mitad de la ventana (13:00).
/// </summary>
public static class GeneradorGeneracionSolar
{
    public const int HoraInicioGeneracion = 7;

    public const int HoraFinGeneracion = 19;

    private const int DuracionVentanaHoras = HoraFinGeneracion - HoraInicioGeneracion;

    /// <summary>
    /// Factor solar entre 0 y 1 para la hora local indicada (hora fraccionaria).
    /// </summary>
    public static decimal CalcularFactorSolar(DateTime horaLocal)
    {
        var hora = horaLocal.TimeOfDay.TotalHours;
        if (hora < HoraInicioGeneracion || hora >= HoraFinGeneracion)
            return 0m;

        var factor = Math.Sin(Math.PI * (hora - HoraInicioGeneracion) / DuracionVentanaHoras);
        var redondeado = Math.Round((decimal)factor, 4, MidpointRounding.AwayFromZero);
        return Math.Clamp(redondeado, 0m, 1m);
    }

    /// <summary>
    /// Potencia generada en W: 0 si el panel está en falla; en caso contrario, la potencia registrada
    /// por el factor solar con la variación aleatoria, sin superar nunca la potencia registrada.
    /// </summary>
    public static decimal CalcularPotenciaGenerada(
        decimal potenciaWp,
        DateTime horaLocal,
        decimal variacion,
        EstadoPanelSimulado estado)
    {
        if (potenciaWp <= 0)
            throw new ArgumentOutOfRangeException(nameof(potenciaWp), "La potencia del panel debe ser mayor a 0.");

        if (variacion is < -1m or > 1m)
            throw new ArgumentOutOfRangeException(nameof(variacion), "La variación debe estar entre -1 y 1.");

        if (estado == EstadoPanelSimulado.EnFalla)
            return 0m;

        var factor = Math.Clamp(CalcularFactorSolar(horaLocal) * (1m + variacion), 0m, 1m);
        var potencia = Math.Round(potenciaWp * factor, 2, MidpointRounding.AwayFromZero);
        return Math.Clamp(potencia, 0m, potenciaWp);
    }
}
