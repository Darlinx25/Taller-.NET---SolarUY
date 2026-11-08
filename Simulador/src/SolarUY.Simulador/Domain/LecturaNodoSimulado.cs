namespace SolarUY.Simulador.Domain;

/// <summary>
/// Lectura de un nodo en un tick: potencia (suma de sus paneles, limitada por el inversor)
/// y energía acumulada desde el arranque del simulador.
/// </summary>
public class LecturaNodoSimulado
{
    private LecturaNodoSimulado(
        Guid nodoId,
        string codigoNodo,
        IReadOnlyList<LecturaPanelSimulado> paneles,
        decimal potenciaGeneradaW,
        decimal energiaAcumuladaKWh,
        DateTime fechaHoraUtc)
    {
        NodoId = nodoId;
        CodigoNodo = codigoNodo;
        Paneles = paneles;
        PotenciaGeneradaW = potenciaGeneradaW;
        EnergiaAcumuladaKWh = energiaAcumuladaKWh;
        FechaHora = fechaHoraUtc;
    }

    public Guid NodoId { get; }

    public string CodigoNodo { get; }

    public decimal PotenciaGeneradaW { get; }

    /// <summary>
    /// Energía acumulada sin redondear: redondearla en cada tick introduciría un error sistemático.
    /// </summary>
    public decimal EnergiaAcumuladaKWh { get; }

    public DateTime FechaHora { get; }

    public IReadOnlyList<LecturaPanelSimulado> Paneles { get; }

    public static LecturaNodoSimulado Crear(
        Guid nodoId,
        string codigoNodo,
        IReadOnlyList<LecturaPanelSimulado> paneles,
        decimal energiaAcumuladaAnteriorKWh,
        decimal? potenciaMaximaKW,
        TimeSpan intervalo,
        DateTime fechaHoraUtc)
    {
        Validaciones.ExigirIdentificador(nodoId, nameof(nodoId), "El nodo es obligatorio.");
        var codigoNormalizado = Validaciones.NormalizarTextoObligatorio(
            codigoNodo,
            nameof(codigoNodo),
            "El código del nodo es obligatorio.");
        ArgumentNullException.ThrowIfNull(paneles);
        Validaciones.ExigirNoNegativo(
            energiaAcumuladaAnteriorKWh,
            nameof(energiaAcumuladaAnteriorKWh),
            "La energía acumulada anterior no puede ser negativa.");
        ValidarPotenciaMaxima(potenciaMaximaKW);
        ValidarIntervalo(intervalo);
        Validaciones.ExigirFechaHora(fechaHoraUtc, nameof(fechaHoraUtc));

        var potenciaW = CalcularPotencia(paneles, potenciaMaximaKW);
        var energiaKWh = energiaAcumuladaAnteriorKWh + CalcularEnergiaDelIntervalo(potenciaW, intervalo);

        return new LecturaNodoSimulado(
            nodoId,
            codigoNormalizado,
            paneles.ToList().AsReadOnly(),
            potenciaW,
            energiaKWh,
            fechaHoraUtc);
    }

    private static void ValidarPotenciaMaxima(decimal? potenciaMaximaKW)
    {
        if (potenciaMaximaKW is < 0m)
            throw new ArgumentOutOfRangeException(
                nameof(potenciaMaximaKW),
                "La potencia máxima del inversor no puede ser negativa.");
    }

    private static void ValidarIntervalo(TimeSpan intervalo)
    {
        if (intervalo <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(intervalo), "El intervalo debe ser mayor a 0.");
    }

    private static decimal CalcularPotencia(IReadOnlyList<LecturaPanelSimulado> paneles, decimal? potenciaMaximaKW)
    {
        var sumaPaneles = paneles.Sum(panel => panel.PotenciaGeneradaW);

        return potenciaMaximaKW is null
            ? sumaPaneles
            : Math.Min(sumaPaneles, potenciaMaximaKW.Value * 1000m);
    }

    private static decimal CalcularEnergiaDelIntervalo(decimal potenciaW, TimeSpan intervalo)
    {
        return potenciaW / 1000m * (decimal)intervalo.TotalSeconds / 3600m;
    }
}
