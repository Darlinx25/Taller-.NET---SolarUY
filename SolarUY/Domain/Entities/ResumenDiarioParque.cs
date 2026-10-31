using System;
using System.Collections.Generic;
using System.Linq;
using SolarUY.Domain.Enums;
using SolarUY.Domain.ValueObjects;

namespace SolarUY.Domain.Entities;

/// <summary>
/// Resumen diario de generación de un parque solar.
/// </summary>
public class ResumenDiarioParque
{
    public const int NombreParqueMaxLength = 100;

    private ResumenDiarioParque()
    {
        NombreParque = string.Empty;
        Nodos = new List<ResumenDiarioNodo>();
        UltimoEstado = EstadoPanel.Operativo;
    }

    private ResumenDiarioParque(
        Guid parqueSolarId,
        string nombreParque,
        DateOnly fechaLocal)
    {
        Id = Guid.NewGuid();
        ParqueSolarId = parqueSolarId;
        NombreParque = nombreParque.Trim();
        FechaLocal = fechaLocal;
        Nodos = new List<ResumenDiarioNodo>();
        UltimoEstado = EstadoPanel.Operativo;
    }

    public Guid Id { get; private set; }

    public Guid ParqueSolarId { get; private set; }

    public string NombreParque { get; private set; }

    public DateOnly FechaLocal { get; private set; }

    public decimal PotenciaAcumuladaW { get; private set; }

    public int CantidadLecturas { get; private set; }

    public decimal EnergiaKWh { get; private set; }

    public decimal PotenciaMaximaW { get; private set; }

    public int Fallas { get; private set; }

    public EstadoPanel UltimoEstado { get; private set; }

    public DateTime? UltimaFechaHoraUtc { get; private set; }

    public ICollection<ResumenDiarioNodo> Nodos { get; private set; }

    public static ResumenDiarioParque Crear(
        Guid parqueSolarId,
        string nombreParque,
        DateOnly fechaLocal)
    {
        if (parqueSolarId == Guid.Empty)
            throw new ArgumentException("El parque solar es obligatorio.", nameof(parqueSolarId));

        if (string.IsNullOrWhiteSpace(nombreParque))
            throw new ArgumentException("El nombre del parque es obligatorio.", nameof(nombreParque));

        if (nombreParque.Trim().Length > NombreParqueMaxLength)
            throw new ArgumentException(
                $"El nombre del parque no puede superar los {NombreParqueMaxLength} caracteres.",
                nameof(nombreParque));

        if (fechaLocal == default)
            throw new ArgumentException("La fecha local es obligatoria.", nameof(fechaLocal));

        return new ResumenDiarioParque(parqueSolarId, nombreParque, fechaLocal);
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

    public bool AgregarNodo(ResumenDiarioNodo nodo)
    {
        ArgumentNullException.ThrowIfNull(nodo);

        if (nodo.FechaLocal != FechaLocal)
            throw new ArgumentException(
                "La fecha local del nodo debe coincidir con la del parque.",
                nameof(nodo));

        if (Nodos.Any(existente => existente.NodoId == nodo.NodoId))
            return false;

        Nodos.Add(nodo);
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
