using System;
using SolarUY.Domain.Enums;
using SolarUY.Domain.ValueObjects;

namespace SolarUY.Domain.Entities;

/// <summary>
/// Resumen diario de generación de un panel.
/// </summary>
public class ResumenDiarioPanel
{
    public const int CodigoSerieMaxLength = 50;

    private ResumenDiarioPanel()
    {
        CodigoSerie = string.Empty;
        UltimoEstado = EstadoPanel.Operativo;
    }

    private ResumenDiarioPanel(
        Guid resumenDiarioNodoId,
        Guid panelId,
        string codigoSerie,
        DateOnly fechaLocal)
    {
        Id = Guid.NewGuid();
        ResumenDiarioNodoId = resumenDiarioNodoId;
        PanelId = panelId;
        CodigoSerie = codigoSerie.Trim();
        FechaLocal = fechaLocal;
        UltimoEstado = EstadoPanel.Operativo;
    }

    public Guid Id { get; private set; }

    public Guid ResumenDiarioNodoId { get; private set; }

    public ResumenDiarioNodo? ResumenDiarioNodo { get; private set; }

    public Guid PanelId { get; private set; }

    public string CodigoSerie { get; private set; }

    public DateOnly FechaLocal { get; private set; }

    public decimal PotenciaAcumuladaW { get; private set; }

    public int CantidadLecturas { get; private set; }

    public decimal EnergiaKWh { get; private set; }

    public decimal PotenciaMaximaW { get; private set; }

    public int Fallas { get; private set; }

    public EstadoPanel UltimoEstado { get; private set; }

    public DateTime? UltimaFechaHoraUtc { get; private set; }

    public static ResumenDiarioPanel Crear(
        Guid resumenDiarioNodoId,
        Guid panelId,
        string codigoSerie,
        DateOnly fechaLocal)
    {
        if (resumenDiarioNodoId == Guid.Empty)
            throw new ArgumentException("El resumen del nodo es obligatorio.", nameof(resumenDiarioNodoId));

        if (panelId == Guid.Empty)
            throw new ArgumentException("El panel es obligatorio.", nameof(panelId));

        if (string.IsNullOrWhiteSpace(codigoSerie))
            throw new ArgumentException("El código/serie del panel es obligatorio.", nameof(codigoSerie));

        if (codigoSerie.Trim().Length > CodigoSerieMaxLength)
            throw new ArgumentException(
                $"El código/serie del panel no puede superar los {CodigoSerieMaxLength} caracteres.",
                nameof(codigoSerie));

        if (fechaLocal == default)
            throw new ArgumentException("La fecha local es obligatoria.", nameof(fechaLocal));

        return new ResumenDiarioPanel(resumenDiarioNodoId, panelId, codigoSerie, fechaLocal);
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
