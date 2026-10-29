namespace SolarUY.Simulador.Domain;

/// <summary>
/// Lectura consolidada de un parque en un tick: suma de potencia y de energía de sus nodos activos.
/// </summary>
public class LecturaParqueSolarSimulado
{
    private LecturaParqueSolarSimulado(
        Guid parqueSolarId,
        string nombreParque,
        IReadOnlyList<LecturaNodoSimulado> nodos,
        DateTime fechaHoraUtc)
    {
        ParqueSolarId = parqueSolarId;
        NombreParque = nombreParque;
        Nodos = nodos;
        PotenciaGeneradaW = nodos.Sum(nodo => nodo.PotenciaGeneradaW);
        EnergiaAcumuladaKWh = nodos.Sum(nodo => nodo.EnergiaAcumuladaKWh);
        FechaHora = fechaHoraUtc;
    }

    public Guid ParqueSolarId { get; }

    public string NombreParque { get; }

    public decimal PotenciaGeneradaW { get; }

    public decimal EnergiaAcumuladaKWh { get; }

    public DateTime FechaHora { get; }

    public IReadOnlyList<LecturaNodoSimulado> Nodos { get; }

    public static LecturaParqueSolarSimulado Crear(
        Guid parqueSolarId,
        string nombreParque,
        IReadOnlyList<LecturaNodoSimulado> nodos,
        DateTime fechaHoraUtc)
    {
        Validaciones.ExigirIdentificador(parqueSolarId, nameof(parqueSolarId), "El parque solar es obligatorio.");
        var nombreNormalizado = Validaciones.NormalizarTextoObligatorio(
            nombreParque,
            nameof(nombreParque),
            "El nombre del parque solar es obligatorio.");
        ArgumentNullException.ThrowIfNull(nodos);
        Validaciones.ExigirFechaHora(fechaHoraUtc, nameof(fechaHoraUtc));

        return new LecturaParqueSolarSimulado(
            parqueSolarId,
            nombreNormalizado,
            nodos.ToList().AsReadOnly(),
            fechaHoraUtc);
    }
}
