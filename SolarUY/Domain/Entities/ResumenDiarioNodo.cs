using System;
using System.Collections.Generic;
using System.Linq;
using SolarUY.Domain.Enums;
using SolarUY.Domain.ValueObjects;

namespace SolarUY.Domain.Entities;

/// <summary>
/// Resumen diario de generación de un nodo.
/// </summary>
public class ResumenDiarioNodo
{
    public const int CodigoNodoMaxLength = 50;

    private ResumenDiarioNodo()
    {
        CodigoNodo = string.Empty;
        Paneles = new List<ResumenDiarioPanel>();
        UltimoEstado = EstadoPanel.Operativo;
    }

    private ResumenDiarioNodo(
        Guid resumenDiarioParqueId,
        Guid nodoId,
        string codigoNodo,
        DateOnly fechaLocal)
    {
        Id = Guid.NewGuid();
        ResumenDiarioParqueId = resumenDiarioParqueId;
        NodoId = nodoId;
        CodigoNodo = codigoNodo.Trim();
        FechaLocal = fechaLocal;
        Paneles = new List<ResumenDiarioPanel>();
        UltimoEstado = EstadoPanel.Operativo;
    }

    public Guid Id { get; private set; }

    public Guid ResumenDiarioParqueId { get; private set; }

    public ResumenDiarioParque? ResumenDiarioParque { get; private set; }

    public Guid NodoId { get; private set; }

    public string CodigoNodo { get; private set; }

    public DateOnly FechaLocal { get; private set; }

    public decimal PotenciaAcumuladaW { get; private set; }

    public int CantidadLecturas { get; private set; }

    public decimal EnergiaKWh { get; private set; }

    public decimal PotenciaMaximaW { get; private set; }

    public int Fallas { get; private set; }

    public EstadoPanel UltimoEstado { get; private set; }

    public DateTime? UltimaFechaHoraUtc { get; private set; }

    public ICollection<ResumenDiarioPanel> Paneles { get; private set; }

    public static ResumenDiarioNodo Crear(
        Guid resumenDiarioParqueId,
        Guid nodoId,
        string codigoNodo,
        DateOnly fechaLocal)
    {
        if (resumenDiarioParqueId == Guid.Empty)
            throw new ArgumentException("El resumen del parque es obligatorio.", nameof(resumenDiarioParqueId));

        if (nodoId == Guid.Empty)
            throw new ArgumentException("El nodo es obligatorio.", nameof(nodoId));

        if (string.IsNullOrWhiteSpace(codigoNodo))
            throw new ArgumentException("El código del nodo es obligatorio.", nameof(codigoNodo));

        if (codigoNodo.Trim().Length > CodigoNodoMaxLength)
            throw new ArgumentException(
                $"El código del nodo no puede superar los {CodigoNodoMaxLength} caracteres.",
                nameof(codigoNodo));

        if (fechaLocal == default)
            throw new ArgumentException("La fecha local es obligatoria.", nameof(fechaLocal));

        return new ResumenDiarioNodo(resumenDiarioParqueId, nodoId, codigoNodo, fechaLocal);
    }

    public bool RegistrarLectura(
        DateTime fechaHoraUtc,
        decimal potenciaW,
        EstadoPanel estado,
        int brechaMaximaSegundos)
    {
        ValidarLectura(fechaHoraUtc, potenciaW, estado, brechaMaximaSegundos);

        if (UltimaFechaHoraUtc.HasValue && fechaHoraUtc <= UltimaFechaHoraUtc.Value)
            return false;

        var energiaKWh = CalculadoraEnergia.CalcularEnergiaKWh(
            potenciaW,
            fechaHoraUtc,
            UltimaFechaHoraUtc,
            brechaMaximaSegundos);

        PotenciaAcumuladaW += potenciaW;
        CantidadLecturas++;
        EnergiaKWh += energiaKWh;

        if (CantidadLecturas == 1 || potenciaW > PotenciaMaximaW)
            PotenciaMaximaW = potenciaW;

        if (estado == EstadoPanel.EnFalla)
            Fallas++;

        UltimoEstado = estado;
        UltimaFechaHoraUtc = fechaHoraUtc;
        return true;
    }

    public bool AgregarPanel(ResumenDiarioPanel panel)
    {
        ArgumentNullException.ThrowIfNull(panel);

        if (panel.FechaLocal != FechaLocal)
            throw new ArgumentException(
                "La fecha local del panel debe coincidir con la del nodo.",
                nameof(panel));

        if (Paneles.Any(existente => existente.PanelId == panel.PanelId))
            return false;

        Paneles.Add(panel);
        return true;
    }

    private static void ValidarLectura(
        DateTime fechaHoraUtc,
        decimal potenciaW,
        EstadoPanel estado,
        int brechaMaximaSegundos)
    {
        if (fechaHoraUtc == default)
            throw new ArgumentException("La fecha y hora de la lectura es obligatoria.", nameof(fechaHoraUtc));

        if (potenciaW < 0)
            throw new ArgumentOutOfRangeException(nameof(potenciaW), "La potencia no puede ser negativa.");

        if (!Enum.IsDefined(estado))
            throw new ArgumentOutOfRangeException(nameof(estado), "El estado del panel no es válido.");

        if (brechaMaximaSegundos <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(brechaMaximaSegundos),
                "La brecha máxima debe ser mayor a 0.");
    }
}
