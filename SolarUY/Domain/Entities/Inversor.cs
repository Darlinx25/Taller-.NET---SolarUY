using System;

namespace SolarUY.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un inversor asociado a un nodo.
/// </summary>
public class Inversor
{
    public const int MarcaMaxLength = 100;
    public const int ModeloMaxLength = 100;
    public const int NumeroSerieMaxLength = 100;

    private Inversor()
    {
        Marca = string.Empty;
        Modelo = string.Empty;
        NumeroSerie = string.Empty;
    }

    public Inversor(string marca, string modelo, string numeroSerie, decimal? potenciaNominalkW = null)
    {
        ValidarMarca(marca);
        ValidarModelo(modelo);
        ValidarNumeroSerie(numeroSerie);
        ValidarPotencia(potenciaNominalkW);

        Id = Guid.NewGuid();
        Marca = marca.Trim();
        Modelo = modelo.Trim();
        NumeroSerie = numeroSerie.Trim();
        PotenciaNominalkW = potenciaNominalkW;
        FechaCreacion = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Marca { get; private set; }

    public string Modelo { get; private set; }

    public string NumeroSerie { get; private set; }

    public decimal? PotenciaNominalkW { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public bool Eliminado { get; private set; }

    public DateTime? FechaEliminacion { get; private set; }

    public Guid? NodoId { get; private set; }

    public Nodo? Nodo { get; private set; }

    public void Eliminar()
    {
        if (Eliminado)
        {
            return;
        }

        Eliminado = true;
        FechaEliminacion = DateTime.UtcNow;
    }

    public void ActualizarDatos(string marca, string modelo, string numeroSerie, decimal? potenciaNominalkW)
    {
        ValidarMarca(marca);
        ValidarModelo(modelo);
        ValidarNumeroSerie(numeroSerie);
        ValidarPotencia(potenciaNominalkW);

        Marca = marca.Trim();
        Modelo = modelo.Trim();
        NumeroSerie = numeroSerie.Trim();
        PotenciaNominalkW = potenciaNominalkW;
    }

    private static void ValidarMarca(string marca)
    {
        if (string.IsNullOrWhiteSpace(marca))
            throw new ArgumentException("La marca del inversor es obligatoria.", nameof(marca));

        if (marca.Trim().Length > MarcaMaxLength)
            throw new ArgumentException($"La marca del inversor no puede superar los {MarcaMaxLength} caracteres.", nameof(marca));
    }

    private static void ValidarModelo(string modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("El modelo del inversor es obligatorio.", nameof(modelo));

        if (modelo.Trim().Length > ModeloMaxLength)
            throw new ArgumentException($"El modelo del inversor no puede superar los {ModeloMaxLength} caracteres.", nameof(modelo));
    }

    private static void ValidarNumeroSerie(string numeroSerie)
    {
        if (string.IsNullOrWhiteSpace(numeroSerie))
            throw new ArgumentException("El número de serie del inversor es obligatorio.", nameof(numeroSerie));

        if (numeroSerie.Trim().Length > NumeroSerieMaxLength)
            throw new ArgumentException($"El número de serie del inversor no puede superar los {NumeroSerieMaxLength} caracteres.", nameof(numeroSerie));
    }

    private static void ValidarPotencia(decimal? potenciaNominalkW)
    {
        if (potenciaNominalkW is < 0)
            throw new ArgumentOutOfRangeException(nameof(potenciaNominalkW), "La potencia nominal no puede ser negativa.");
    }
}
