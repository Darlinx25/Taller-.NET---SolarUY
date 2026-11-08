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
    }

    public Guid PanelId { get; }

    public Guid NodoId { get; }

    public string CodigoSerie { get; }

    public decimal PotenciaWp { get; }

    public EstadoPanelSimulado Estado { get; private set; }

    /// <summary>
    /// Avanza un tick con un único valor aleatorio: un panel en falla se recupera si
    /// <paramref name="valorAleatorio"/> es menor que <paramref name="probabilidadRecuperacion"/>;
    /// un panel operativo falla si es menor que <paramref name="probabilidadFalla"/>.
    /// Las dos ramas son excluyentes: un panel que se recupera no vuelve a fallar en el mismo tick.
    /// </summary>
    public void ActualizarEstado(decimal probabilidadFalla, decimal probabilidadRecuperacion, decimal valorAleatorio)
    {
        if (probabilidadFalla is < 0m or > 1m)
            throw new ArgumentOutOfRangeException(nameof(probabilidadFalla), "La probabilidad de falla debe estar entre 0 y 1.");

        if (probabilidadRecuperacion is < 0m or > 1m)
            throw new ArgumentOutOfRangeException(
                nameof(probabilidadRecuperacion),
                "La probabilidad de recuperación debe estar entre 0 y 1.");

        if (valorAleatorio is < 0m or > 1m)
            throw new ArgumentOutOfRangeException(nameof(valorAleatorio), "El valor aleatorio debe estar entre 0 y 1.");

        if (Estado == EstadoPanelSimulado.EnFalla)
        {
            if (valorAleatorio < probabilidadRecuperacion)
                Estado = EstadoPanelSimulado.Operativo;

            return;
        }

        if (valorAleatorio < probabilidadFalla)
            Estado = EstadoPanelSimulado.EnFalla;
    }

    /// <summary>
    /// Fuerza una falla: el panel queda <see cref="EstadoPanelSimulado.EnFalla"/>. Se invoca después de
    /// <see cref="ActualizarEstado"/> en el tick en que se pide la falla; desde el tick siguiente rige la
    /// misma recuperación probabilística que para una falla aleatoria.
    /// </summary>
    public void ForzarFalla()
    {
        Estado = EstadoPanelSimulado.EnFalla;
    }

    public LecturaPanelSimulado GenerarLectura(DateTime horaLocal, decimal variacion, DateTime fechaHoraUtc)
    {
        var potencia = GeneradorGeneracionSolar.CalcularPotenciaGenerada(PotenciaWp, horaLocal, variacion, Estado);

        return new LecturaPanelSimulado(PanelId, CodigoSerie, potencia, Estado, fechaHoraUtc);
    }
}
