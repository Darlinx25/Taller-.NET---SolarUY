using System;

namespace SolarUY.Domain.Entities;

public class TareaMantenimiento
{
    public const int TipoTareaMaxLength = 100;
    public const int DescripcionMaxLength = 1000;
    public const int AsignadoAMaxLength = 200;
    public const int QuienAsignaMaxLength = 200;
    public const int QuienCompletaMaxLength = 200;
    public const int EstadoMaxLength = 50;
    public const int ObservacionesMaxLength = 1000;

    public static readonly string[] EstadosPermitidos = { "Pendiente", "EnProceso", "Completada", "Cancelada" };
    public static readonly string[] EstadosOrden = { "Pendiente", "EnProceso", "Completada", "Cancelada" };

    private TareaMantenimiento()
    {
        TipoTarea = string.Empty;
        Descripcion = string.Empty;
        AsignadoA = string.Empty;
        Estado = "Pendiente";
        QuienAsigna = string.Empty;
        QuienCompleta = null;
    }

    public TareaMantenimiento(
        Guid panelId,
        string tipoTarea,
        string descripcion,
        string asignadoA,
        DateTime fechaVencimiento,
        string quienAsigna,
        string? observaciones = null)
    {
        ValidarPanelId(panelId);
        ValidarTipoTarea(tipoTarea);
        ValidarDescripcion(descripcion);
        ValidarAsignadoA(asignadoA);
        ValidarFechaObligatoria(fechaVencimiento, nameof(fechaVencimiento));
        ValidarQuienAsigna(quienAsigna);

        Id = Guid.NewGuid();
        PanelId = panelId;
        TipoTarea = tipoTarea.Trim();
        Descripcion = descripcion.Trim();
        AsignadoA = asignadoA.Trim();
        FechaCreacion = DateTime.UtcNow;
        FechaAsignacion = DateTime.UtcNow.Date;
        FechaVencimiento = fechaVencimiento.Date;
        FechaInicio = default;
        FechaFinalizacion = default;
        Estado = "Pendiente";
        QuienAsigna = quienAsigna.Trim();
        QuienCompleta = null;
        Observaciones = NormalizarObservaciones(observaciones);
    }

    public Guid Id { get; private set; }

    public Guid PanelId { get; private set; }

    public Panel? Panel { get; private set; }

    public string TipoTarea { get; private set; }

    public string Descripcion { get; private set; }

    public string AsignadoA { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public DateTime FechaAsignacion { get; private set; }

    public DateTime FechaVencimiento { get; private set; }

    public DateTime FechaInicio { get; private set; }

    public DateTime FechaFinalizacion { get; private set; }

    public string Estado { get; private set; }

    public string QuienAsigna { get; private set; }

    public string? QuienCompleta { get; private set; }

    public string? Observaciones { get; private set; }

    public void ActualizarDatos(
        string tipoTarea,
        string descripcion,
        string asignadoA,
        DateTime fechaVencimiento,
        string? observaciones = null)
    {
        if (EsEstadoFinal(Estado))
            throw new Exceptions.TareaMantenimientoNoEditableException(Id);

        ValidarTipoTarea(tipoTarea);
        ValidarDescripcion(descripcion);
        ValidarAsignadoA(asignadoA);
        ValidarFechaObligatoria(fechaVencimiento, nameof(fechaVencimiento));

        TipoTarea = tipoTarea.Trim();
        Descripcion = descripcion.Trim();
        AsignadoA = asignadoA.Trim();
        FechaVencimiento = fechaVencimiento.Date;
        Observaciones = NormalizarObservaciones(observaciones);
    }

    public void CambiarEstado(string nuevoEstado, string? quienCompleta = null)
    {
        if (EsEstadoFinal(Estado))
            throw new Exceptions.TareaMantenimientoEstadoInvalidoException($"No se puede cambiar el estado desde {Estado}.");

        ValidarEstado(nuevoEstado);

        var estadoActualNormalizado = NormalizarEstado(Estado);
        var nuevoEstadoNormalizado = NormalizarEstado(nuevoEstado);

        var indiceActual = ObtenerIndiceEstado(estadoActualNormalizado);
        var indiceNuevo = ObtenerIndiceEstado(nuevoEstadoNormalizado);

        if (indiceNuevo <= indiceActual)
            throw new Exceptions.TareaMantenimientoEstadoInvalidoException("Solo se permite avanzar al siguiente estado, no retroceder.");

        Estado = nuevoEstadoNormalizado;

        if (string.Equals(Estado, "EnProceso", StringComparison.OrdinalIgnoreCase) && FechaInicio == default)
            FechaInicio = DateTime.UtcNow;

        if (string.Equals(Estado, "Completada", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(quienCompleta))
                throw new ArgumentException("QuienCompleta es obligatorio al pasar a Completada.", nameof(quienCompleta));
            QuienCompleta = quienCompleta.Trim();
            if (FechaFinalizacion == default)
                FechaFinalizacion = DateTime.UtcNow;
        }

        if (string.Equals(Estado, "Cancelada", StringComparison.OrdinalIgnoreCase))
        {
            if (FechaFinalizacion == default)
                FechaFinalizacion = DateTime.UtcNow;
        }
    }

    private static bool EsEstadoFinal(string estado)
    {
        return string.Equals(estado, "Completada", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(estado, "Cancelada", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizarEstado(string estado)
    {
        var t = estado.Trim();
        if (string.Equals(t, "Pendiente", StringComparison.OrdinalIgnoreCase)) return "Pendiente";
        if (string.Equals(t, "EnProceso", StringComparison.OrdinalIgnoreCase)) return "EnProceso";
        if (string.Equals(t, "Completada", StringComparison.OrdinalIgnoreCase)) return "Completada";
        if (string.Equals(t, "Cancelada", StringComparison.OrdinalIgnoreCase)) return "Cancelada";
        return t;
    }

    private static int ObtenerIndiceEstado(string estado)
    {
        var n = NormalizarEstado(estado);
        for (int i = 0; i < EstadosOrden.Length; i++)
        {
            if (string.Equals(EstadosOrden[i], n, StringComparison.OrdinalIgnoreCase)) return i;
        }
        throw new Exceptions.TareaMantenimientoEstadoInvalidoException(estado);
    }

    private static void ValidarPanelId(Guid panelId)
    {
        if (panelId == Guid.Empty)
            throw new ArgumentException("El panel asociado es obligatorio.", nameof(panelId));
    }

    private static void ValidarTipoTarea(string tipoTarea)
    {
        if (string.IsNullOrWhiteSpace(tipoTarea))
            throw new ArgumentException("El tipo de tarea es obligatorio.", nameof(tipoTarea));
        if (tipoTarea.Trim().Length > TipoTareaMaxLength)
            throw new ArgumentException($"El tipo de tarea no puede superar los {TipoTareaMaxLength} caracteres.", nameof(tipoTarea));
    }

    private static void ValidarDescripcion(string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La descripción es obligatoria.", nameof(descripcion));
        if (descripcion.Trim().Length > DescripcionMaxLength)
            throw new ArgumentException($"La descripción no puede superar los {DescripcionMaxLength} caracteres.", nameof(descripcion));
    }

    private static void ValidarAsignadoA(string asignadoA)
    {
        if (string.IsNullOrWhiteSpace(asignadoA))
            throw new ArgumentException("AsignadoA es obligatorio.", nameof(asignadoA));
        if (asignadoA.Trim().Length > AsignadoAMaxLength)
            throw new ArgumentException($"AsignadoA no puede superar los {AsignadoAMaxLength} caracteres.", nameof(asignadoA));
    }

    private static void ValidarQuienAsigna(string quienAsigna)
    {
        if (string.IsNullOrWhiteSpace(quienAsigna))
            throw new ArgumentException("QuienAsigna es obligatorio.", nameof(quienAsigna));
        if (quienAsigna.Trim().Length > QuienAsignaMaxLength)
            throw new ArgumentException($"QuienAsigna no puede superar los {QuienAsignaMaxLength} caracteres.", nameof(quienAsigna));
    }

    private static void ValidarFechaObligatoria(DateTime fecha, string nombre)
    {
        if (fecha == default)
            throw new ArgumentException($"La fecha {nombre} es obligatoria.", nombre);
    }

    private static void ValidarEstado(string estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
            throw new ArgumentException("El estado es obligatorio.", nameof(estado));
        var estadoTrim = estado.Trim();
        var esValido = false;
        foreach (var e in EstadosPermitidos)
        {
            if (string.Equals(e, estadoTrim, StringComparison.OrdinalIgnoreCase))
            {
                esValido = true;
                break;
            }
        }
        if (!esValido)
            throw new Exceptions.TareaMantenimientoEstadoInvalidoException(estadoTrim);
        if (estadoTrim.Length > EstadoMaxLength)
            throw new ArgumentException($"El estado no puede superar los {EstadoMaxLength} caracteres.", nameof(estado));
    }

    private static void ValidarObservaciones(string? observaciones)
    {
        if (observaciones is null) return;
        if (observaciones.Trim().Length > ObservacionesMaxLength)
            throw new ArgumentException($"Las observaciones no pueden superar los {ObservacionesMaxLength} caracteres.", nameof(observaciones));
    }

    private static string? NormalizarObservaciones(string? observaciones)
    {
        return string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
    }
}
