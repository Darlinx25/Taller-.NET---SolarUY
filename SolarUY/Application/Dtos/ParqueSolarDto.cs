using System;

namespace SolarUY.Application.Dtos;

/// <summary>
/// Proyección de un parque solar para las vistas de la capa de Presentación.
/// </summary>
public class ParqueSolarDto
{
    /// <summary>
    /// Identificador único del parque solar.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del parque solar.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Ubicación geográfica o dirección del parque solar.
    /// </summary>
    public string Ubicacion { get; set; } = string.Empty;

    /// <summary>
    /// Capacidad total estimada en megavatios.
    /// </summary>
    public decimal? CapacidadEstimadaMW { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) de alta del parque solar.
    /// </summary>
    public DateTime FechaCreacion { get; set; }
}
