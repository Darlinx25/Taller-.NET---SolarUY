using System;

namespace SolarUY.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un nodo dentro de un parque solar.
/// Cada nodo tiene exactamente un inversor asociado (relación 1:1).
/// </summary>
public class Nodo
{
    public const int CodigoMaxLength = 50;
    public const int DescripcionMaxLength = 200;
    public const int EstadoMaxLength = 50;

    private Nodo()
    {
        Codigo = string.Empty;
        Descripcion = null;
        Estado = "Activo";
    }

    public Nodo(string codigo, Guid parqueSolarId, Inversor inversor, string? descripcion = null, string estado = "Activo")
    {
        ValidarCodigo(codigo);
        ValidarParqueSolarId(parqueSolarId);
        ArgumentNullException.ThrowIfNull(inversor);
        ValidarDescripcion(descripcion);
        ValidarEstado(estado);

        Id = Guid.NewGuid();
        Codigo = codigo.Trim();
        ParqueSolarId = parqueSolarId;
        Inversor = inversor;
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        Estado = string.IsNullOrWhiteSpace(estado) ? "Activo" : estado.Trim();
        FechaCreacion = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Codigo { get; private set; }

    public Guid ParqueSolarId { get; private set; }

    public ParqueSolar? ParqueSolar { get; private set; }

    public Inversor Inversor { get; private set; } = null!;

    public string? Descripcion { get; private set; }

    public string Estado { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public void ActualizarDatos(string codigo, string? descripcion, string estado)
    {
        ValidarCodigo(codigo);
        ValidarDescripcion(descripcion);
        ValidarEstado(estado);

        Codigo = codigo.Trim();
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        Estado = string.IsNullOrWhiteSpace(estado) ? "Activo" : estado.Trim();
    }

    private static void ValidarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del nodo es obligatorio.", nameof(codigo));

        if (codigo.Trim().Length > CodigoMaxLength)
            throw new ArgumentException($"El código del nodo no puede superar los {CodigoMaxLength} caracteres.", nameof(codigo));
    }

    private static void ValidarParqueSolarId(Guid parqueSolarId)
    {
        if (parqueSolarId == Guid.Empty)
            throw new ArgumentException("El parque solar asociado es obligatorio.", nameof(parqueSolarId));
    }

    private static void ValidarDescripcion(string? descripcion)
    {
        if (descripcion is null)
            return;

        if (descripcion.Trim().Length > DescripcionMaxLength)
            throw new ArgumentException($"La descripción no puede superar los {DescripcionMaxLength} caracteres.", nameof(descripcion));
    }

    private static void ValidarEstado(string estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
            throw new ArgumentException("El estado del nodo es obligatorio.", nameof(estado));

        if (estado.Trim().Length > EstadoMaxLength)
            throw new ArgumentException($"El estado del nodo no puede superar los {EstadoMaxLength} caracteres.", nameof(estado));
    }
}
