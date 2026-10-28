using System;

namespace SolarUY.Domain.Entities;

public class Panel
{
    public const int CodigoSerieMaxLength = 50;
    public const int MarcaMaxLength = 100;
    public const int ModeloMaxLength = 100;
    public const int ObservacionesMaxLength = 500;

    private Panel()
    {
        CodigoSerie = string.Empty;
        Marca = string.Empty;
        Modelo = string.Empty;
    }

    public Panel(
        Guid nodoId,
        string codigoSerie,
        string marca,
        string modelo,
        decimal potenciaWp,
        DateTime? fechaInstalacion = null,
        string? observaciones = null)
    {
        ValidarNodoId(nodoId);
        ValidarCodigoSerie(codigoSerie);
        ValidarMarca(marca);
        ValidarModelo(modelo);
        ValidarPotencia(potenciaWp);
        ValidarObservaciones(observaciones);

        Id = Guid.NewGuid();
        NodoId = nodoId;
        CodigoSerie = codigoSerie.Trim();
        Marca = marca.Trim();
        Modelo = modelo.Trim();
        PotenciaWp = potenciaWp;
        FechaInstalacion = fechaInstalacion;
        Observaciones = NormalizarObservaciones(observaciones);
        FechaCreacion = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid NodoId { get; private set; }

    public Nodo? Nodo { get; private set; }

    public string CodigoSerie { get; private set; }

    public string Marca { get; private set; }

    public string Modelo { get; private set; }

    public decimal PotenciaWp { get; private set; }

    public DateTime? FechaInstalacion { get; private set; }

    public string? Observaciones { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public void ActualizarDatos(
        string codigoSerie,
        string marca,
        string modelo,
        decimal potenciaWp,
        DateTime? fechaInstalacion,
        string? observaciones)
    {
        ValidarCodigoSerie(codigoSerie);
        ValidarMarca(marca);
        ValidarModelo(modelo);
        ValidarPotencia(potenciaWp);
        ValidarObservaciones(observaciones);

        CodigoSerie = codigoSerie.Trim();
        Marca = marca.Trim();
        Modelo = modelo.Trim();
        PotenciaWp = potenciaWp;
        FechaInstalacion = fechaInstalacion;
        Observaciones = NormalizarObservaciones(observaciones);
    }

    private static void ValidarNodoId(Guid nodoId)
    {
        if (nodoId == Guid.Empty)
            throw new ArgumentException("El nodo asociado es obligatorio.", nameof(nodoId));
    }

    private static void ValidarCodigoSerie(string codigoSerie)
    {
        if (string.IsNullOrWhiteSpace(codigoSerie))
            throw new ArgumentException("El código/serie del panel es obligatorio.", nameof(codigoSerie));

        if (codigoSerie.Trim().Length > CodigoSerieMaxLength)
            throw new ArgumentException(
                $"El código/serie del panel no puede superar los {CodigoSerieMaxLength} caracteres.",
                nameof(codigoSerie));
    }

    private static void ValidarMarca(string marca)
    {
        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("La marca del panel es obligatoria.", nameof(marca));

        if (marca.Trim().Length > MarcaMaxLength)
            throw new ArgumentException(
                $"La marca del panel no puede superar los {MarcaMaxLength} caracteres.",
                nameof(marca));
    }

    private static void ValidarModelo(string modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("El modelo del panel es obligatorio.", nameof(modelo));

        if (modelo.Trim().Length > ModeloMaxLength)
            throw new ArgumentException(
                $"El modelo del panel no puede superar los {ModeloMaxLength} caracteres.",
                nameof(modelo));
    }

    private static void ValidarPotencia(decimal potenciaWp)
    {
        if (potenciaWp <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(potenciaWp),
                "La potencia del panel debe ser mayor a 0.");
    }

    private static void ValidarObservaciones(string? observaciones)
    {
        if (observaciones is null)
            return;

        if (observaciones.Trim().Length > ObservacionesMaxLength)
            throw new ArgumentException(
                $"Las observaciones no pueden superar los {ObservacionesMaxLength} caracteres.",
                nameof(observaciones));
    }

    private static string? NormalizarObservaciones(string? observaciones)
    {
        return string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
    }
}
