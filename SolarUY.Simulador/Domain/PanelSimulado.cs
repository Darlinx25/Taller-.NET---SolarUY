namespace SolarUY.Simulador.Domain;

/// <summary>
/// Estado en memoria de un panel durante la simulación (fallas y recuperación). No se persiste.
/// </summary>
public class PanelSimulado
{
    public PanelSimulado(Guid panelId, Guid nodoId, string codigoSerie, decimal potenciaWp)
    {
        Validaciones.ExigirIdentificador(panelId, nameof(panelId), "El panel es obligatorio.");
        Validaciones.ExigirIdentificador(nodoId, nameof(nodoId), "El nodo del panel es obligatorio.");
        var codigoNormalizado = Validaciones.NormalizarTextoObligatorio(
            codigoSerie,
            nameof(codigoSerie),
            "El código/serie del panel es obligatorio.");

        if (potenciaWp <= 0)
            throw new ArgumentOutOfRangeException(nameof(potenciaWp), "La potencia del panel debe ser mayor a 0.");

        PanelId = panelId;
        NodoId = nodoId;
        CodigoSerie = codigoNormalizado;
        PotenciaWp = potenciaWp;
        Estado = EstadoPanelSimulado.Operativo;
        TicksRestantesFalla = 0;
    }

    public Guid PanelId { get; }

    public Guid NodoId { get; }

    public string CodigoSerie { get; }

    public decimal PotenciaWp { get; }

    public EstadoPanelSimulado Estado { get; private set; }

    public int TicksRestantesFalla { get; private set; }

    /// <summary>
    /// Avanza un tick: un panel en falla descuenta un tick y se recupera al llegar a 0;
    /// un panel operativo falla si <paramref name="valorAleatorio"/> es menor que la probabilidad.
    /// Las dos ramas son excluyentes: un panel que se recupera no vuelve a fallar en el mismo tick.
    /// </summary>
    public void ActualizarEstado(decimal probabilidadFalla, int duracionFallaTicks, decimal valorAleatorio)
    {
        if (probabilidadFalla is < 0m or > 1m)
            throw new ArgumentOutOfRangeException(nameof(probabilidadFalla), "La probabilidad de falla debe estar entre 0 y 1.");

        if (duracionFallaTicks <= 0)
            throw new ArgumentOutOfRangeException(nameof(duracionFallaTicks), "La duración de la falla debe ser mayor a 0.");

        if (valorAleatorio is < 0m or > 1m)
            throw new ArgumentOutOfRangeException(nameof(valorAleatorio), "El valor aleatorio debe estar entre 0 y 1.");

        if (Estado == EstadoPanelSimulado.EnFalla)
        {
            Recuperarse();
            return;
        }

        if (valorAleatorio < probabilidadFalla)
        {
            Estado = EstadoPanelSimulado.EnFalla;
            TicksRestantesFalla = duracionFallaTicks;
        }
    }

    public LecturaPanelSimulado GenerarLectura(DateTime horaLocal, decimal variacion, DateTime fechaHoraUtc)
    {
        var potencia = GeneradorGeneracionSolar.CalcularPotenciaGenerada(PotenciaWp, horaLocal, variacion, Estado);

        return new LecturaPanelSimulado(PanelId, CodigoSerie, potencia, Estado, fechaHoraUtc);
    }

    private void Recuperarse()
    {
        TicksRestantesFalla--;

        if (TicksRestantesFalla <= 0)
        {
            TicksRestantesFalla = 0;
            Estado = EstadoPanelSimulado.Operativo;
        }
    }
}
