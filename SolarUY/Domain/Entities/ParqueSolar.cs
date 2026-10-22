using System;

namespace SolarUY.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un Parque Solar registrado en la plataforma.
/// Contiene su estado y las validaciones propias del negocio.
/// </summary>
public class ParqueSolar
{
    /// <summary>
    /// Longitud máxima permitida para <see cref="Nombre"/>.
    /// </summary>
    public const int NombreMaxLength = 100;

    /// <summary>
    /// Longitud máxima permitida para <see cref="Ubicacion"/>.
    /// </summary>
    public const int UbicacionMaxLength = 200;

    /// <summary>
    /// Constructor para el materializador de Entity Framework Core.
    /// </summary>
    private ParqueSolar()
    {
        Nombre = string.Empty;
        Ubicacion = string.Empty;
    }

    /// <summary>
    /// Crea un parque solar validando los datos obligatorios del negocio.
    /// </summary>
    /// <param name="nombre">Nombre único del parque solar.</param>
    /// <param name="ubicacion">Ubicación geográfica o dirección del parque.</param>
    /// <param name="capacidadEstimadaMW">Capacidad total estimada en megavatios (opcional).</param>
    /// <exception cref="ArgumentException">Si los datos son inválidos.</exception>
    public ParqueSolar(string nombre, string ubicacion, decimal? capacidadEstimadaMW = null)
    {
        ValidarNombre(nombre);
        ValidarUbicacion(ubicacion);
        ValidarCapacidad(capacidadEstimadaMW);

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Ubicacion = ubicacion.Trim();
        CapacidadEstimadaMW = capacidadEstimadaMW;
        FechaCreacion = DateTime.UtcNow;
    }

    /// <summary>
    /// Identificador único del parque solar.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Nombre del parque solar. Es único en el sistema.
    /// </summary>
    public string Nombre { get; private set; }

    /// <summary>
    /// Ubicación geográfica o dirección del parque solar.
    /// </summary>
    public string Ubicacion { get; private set; }

    /// <summary>
    /// Capacidad total estimada en megavatios. Es opcional.
    /// </summary>
    public decimal? CapacidadEstimadaMW { get; private set; }

    /// <summary>
    /// Fecha y hora (UTC) de alta del parque solar.
    /// </summary>
    public DateTime FechaCreacion { get; private set; }

    /// <summary>
    /// Actualiza la capacidad total estimada del parque solar.
    /// </summary>
    /// <param name="capacidadEstimadaMW">Nueva capacidad en megavatios.</param>
    /// <exception cref="ArgumentOutOfRangeException">Si la capacidad es negativa.</exception>
    public void ActualizarCapacidadEstimada(decimal capacidadEstimadaMW)
    {
        ValidarCapacidad(capacidadEstimadaMW);
        CapacidadEstimadaMW = capacidadEstimadaMW;
    }

    /// <summary>
    /// Indica si el parque solar tiene capacidad total estimada registrada.
    /// </summary>
    public bool TieneCapacidadEstimada => CapacidadEstimadaMW.HasValue;

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del parque solar es obligatorio.", nameof(nombre));
        }

        if (nombre.Trim().Length > NombreMaxLength)
        {
            throw new ArgumentException(
                $"El nombre del parque solar no puede superar los {NombreMaxLength} caracteres.",
                nameof(nombre));
        }
    }

    private static void ValidarUbicacion(string ubicacion)
    {
        if (string.IsNullOrWhiteSpace(ubicacion))
        {
            throw new ArgumentException("La ubicación del parque solar es obligatoria.", nameof(ubicacion));
        }

        if (ubicacion.Trim().Length > UbicacionMaxLength)
        {
            throw new ArgumentException(
                $"La ubicación del parque solar no puede superar los {UbicacionMaxLength} caracteres.",
                nameof(ubicacion));
        }
    }

    private static void ValidarCapacidad(decimal? capacidadEstimadaMW)
    {
        if (capacidadEstimadaMW is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacidadEstimadaMW),
                "La capacidad estimada no puede ser negativa.");
        }
    }
}
