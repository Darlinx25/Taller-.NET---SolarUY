using System;

namespace SolarUY.Domain.Entities;

/// <summary>
/// Alerta por el fallo de un panel. Se abre cuando el panel informa el estado <c>EnFalla</c> y se cierra
/// cuando se recupera o deja de aparecer en la telemetría. Se conserva como historial.
/// </summary>
public class AlertaPanel
{
    public const int NombreParqueMaxLength = ParqueSolar.NombreMaxLength;
    public const int CodigoNodoMaxLength = Nodo.CodigoMaxLength;
    public const int CodigoSerieMaxLength = Panel.CodigoSerieMaxLength;

    private AlertaPanel()
    {
        NombreParque = string.Empty;
        CodigoNodo = string.Empty;
        CodigoSerie = string.Empty;
    }

    private AlertaPanel(
        Guid parqueSolarId,
        string nombreParque,
        Guid nodoId,
        string codigoNodo,
        Guid panelId,
        string codigoSerie,
        DateTime fechaInicioUtc)
    {
        Id = Guid.NewGuid();
        ParqueSolarId = parqueSolarId;
        NombreParque = nombreParque;
        NodoId = nodoId;
        CodigoNodo = codigoNodo;
        PanelId = panelId;
        CodigoSerie = codigoSerie;
        FechaInicioUtc = fechaInicioUtc;
    }

    public Guid Id { get; private set; }

    public Guid ParqueSolarId { get; private set; }

    public string NombreParque { get; private set; }

    public Guid NodoId { get; private set; }

    public string CodigoNodo { get; private set; }

    public Guid PanelId { get; private set; }

    public string CodigoSerie { get; private set; }

    public DateTime FechaInicioUtc { get; private set; }

    public DateTime? FechaFinUtc { get; private set; }

    public bool EstaActiva => FechaFinUtc is null;

    /// <summary>
    /// Abre una alerta activa para un panel en falla.
    /// </summary>
    public static AlertaPanel Abrir(
        Guid parqueSolarId,
        string nombreParque,
        Guid nodoId,
        string codigoNodo,
        Guid panelId,
        string codigoSerie,
        DateTime fechaInicioUtc)
    {
        ValidarIdentificador(parqueSolarId, "El parque solar de la alerta es obligatorio.", nameof(parqueSolarId));
        var nombreParqueNormalizado = NormalizarTexto(
            nombreParque, NombreParqueMaxLength, "El nombre del parque solar", nameof(nombreParque));
        ValidarIdentificador(nodoId, "El nodo de la alerta es obligatorio.", nameof(nodoId));
        var codigoNodoNormalizado = NormalizarTexto(
            codigoNodo, CodigoNodoMaxLength, "El código del nodo", nameof(codigoNodo));
        ValidarIdentificador(panelId, "El panel de la alerta es obligatorio.", nameof(panelId));
        var codigoSerieNormalizado = NormalizarTexto(
            codigoSerie, CodigoSerieMaxLength, "El código/serie del panel", nameof(codigoSerie));
        ValidarFecha(fechaInicioUtc, "La fecha de inicio de la alerta es obligatoria.", nameof(fechaInicioUtc));

        return new AlertaPanel(
            parqueSolarId,
            nombreParqueNormalizado,
            nodoId,
            codigoNodoNormalizado,
            panelId,
            codigoSerieNormalizado,
            fechaInicioUtc);
    }

    /// <summary>
    /// Cierra la alerta. Devuelve <c>false</c> sin modificarla si <paramref name="fechaFinUtc"/> es menor
    /// o igual al inicio (lectura repetida o anterior).
    /// </summary>
    /// <exception cref="InvalidOperationException">Si la alerta ya está cerrada.</exception>
    public bool Cerrar(DateTime fechaFinUtc)
    {
        if (!EstaActiva)
            throw new InvalidOperationException("La alerta ya está cerrada.");

        ValidarFecha(fechaFinUtc, "La fecha de fin de la alerta es obligatoria.", nameof(fechaFinUtc));

        if (fechaFinUtc <= FechaInicioUtc)
            return false;

        FechaFinUtc = fechaFinUtc;
        return true;
    }

    /// <summary>
    /// Duración de la alerta: hasta su fin si está cerrada o hasta <paramref name="ahoraUtc"/> si está activa.
    /// </summary>
    public TimeSpan CalcularDuracion(DateTime ahoraUtc)
    {
        ValidarFecha(ahoraUtc, "La fecha de referencia es obligatoria.", nameof(ahoraUtc));

        var duracion = (FechaFinUtc ?? ahoraUtc) - FechaInicioUtc;
        return duracion < TimeSpan.Zero ? TimeSpan.Zero : duracion;
    }

    /// <summary>
    /// Indica si un panel en falla debe abrir una alerta nueva, dada la última alerta que tuvo (si la tuvo):
    /// no si ya tiene una activa y no si la lectura es anterior o igual al fin de la última cerrada.
    /// </summary>
    public static bool DebeAbrirse(AlertaPanel? ultimaAlertaDelPanel, DateTime fechaHoraUtc)
    {
        ValidarFecha(fechaHoraUtc, "La fecha y hora de la lectura es obligatoria.", nameof(fechaHoraUtc));

        if (ultimaAlertaDelPanel is null)
            return true;

        if (ultimaAlertaDelPanel.EstaActiva)
            return false;

        return fechaHoraUtc > ultimaAlertaDelPanel.FechaFinUtc!.Value;
    }

    private static void ValidarIdentificador(Guid id, string mensaje, string parametro)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(mensaje, parametro);
    }

    private static string NormalizarTexto(string? texto, int longitudMaxima, string campo, string parametro)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException($"{campo} es obligatorio.", parametro);

        var normalizado = texto.Trim();
        if (normalizado.Length > longitudMaxima)
            throw new ArgumentException($"{campo} no puede superar los {longitudMaxima} caracteres.", parametro);

        return normalizado;
    }

    private static void ValidarFecha(DateTime fecha, string mensaje, string parametro)
    {
        if (fecha == default)
            throw new ArgumentException(mensaje, parametro);
    }
}
