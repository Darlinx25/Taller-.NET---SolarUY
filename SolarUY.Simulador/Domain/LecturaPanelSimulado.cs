namespace SolarUY.Simulador.Domain;

/// <summary>
/// Lectura de un panel en un tick: potencia generada en W y estado de salud.
/// </summary>
public class LecturaPanelSimulado
{
    public LecturaPanelSimulado(
        Guid panelId,
        string codigoSerie,
        decimal potenciaGeneradaW,
        EstadoPanelSimulado estado,
        DateTime fechaHoraUtc)
    {
        Validaciones.ExigirIdentificador(panelId, nameof(panelId), "El panel es obligatorio.");
        var codigoNormalizado = Validaciones.NormalizarTextoObligatorio(
            codigoSerie,
            nameof(codigoSerie),
            "El código/serie del panel es obligatorio.");
        Validaciones.ExigirNoNegativo(
            potenciaGeneradaW,
            nameof(potenciaGeneradaW),
            "La potencia generada no puede ser negativa.");
        Validaciones.ExigirFechaHora(fechaHoraUtc, nameof(fechaHoraUtc));

        PanelId = panelId;
        CodigoSerie = codigoNormalizado;
        PotenciaGeneradaW = potenciaGeneradaW;
        Estado = estado;
        FechaHora = fechaHoraUtc;
    }

    public Guid PanelId { get; }

    public string CodigoSerie { get; }

    public decimal PotenciaGeneradaW { get; }

    public EstadoPanelSimulado Estado { get; }

    public DateTime FechaHora { get; }

    public bool EstaOperativo => Estado == EstadoPanelSimulado.Operativo;
}
