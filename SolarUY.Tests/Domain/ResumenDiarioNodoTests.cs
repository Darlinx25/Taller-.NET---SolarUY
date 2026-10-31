using System;
using System.Linq;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Enums;
using Xunit;

namespace SolarUY.Tests.Domain;

public class ResumenDiarioNodoTests
{
    private static readonly DateOnly Fecha = new(2026, 1, 15);
    private static readonly DateTime FechaHora = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid ResumenParqueId = Guid.NewGuid();
    private static readonly Guid NodoId = Guid.NewGuid();

    private static ResumenDiarioNodo CrearResumen()
    {
        return ResumenDiarioNodo.Crear(ResumenParqueId, NodoId, "N-01", Fecha);
    }

    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var resumen = CrearResumen();

        Assert.NotEqual(Guid.Empty, resumen.Id);
        Assert.Equal(ResumenParqueId, resumen.ResumenDiarioParqueId);
        Assert.Equal(NodoId, resumen.NodoId);
        Assert.Equal("N-01", resumen.CodigoNodo);
        Assert.Equal(Fecha, resumen.FechaLocal);
        Assert.Equal(0, resumen.CantidadLecturas);
        Assert.Empty(resumen.Paneles);
    }

    [Fact]
    public void Crear_DatosInvalidos_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(
            () => ResumenDiarioNodo.Crear(Guid.Empty, NodoId, "N-01", Fecha));

        Assert.Throws<ArgumentException>(
            () => ResumenDiarioNodo.Crear(ResumenParqueId, Guid.Empty, "N-01", Fecha));

        Assert.Throws<ArgumentException>(
            () => ResumenDiarioNodo.Crear(ResumenParqueId, NodoId, " ", Fecha));

        Assert.Throws<ArgumentException>(
            () => ResumenDiarioNodo.Crear(ResumenParqueId, NodoId, "N-01", default));
    }

    [Fact]
    public void RegistrarLectura_AcumulaEnergiaPotenciaYFallas()
    {
        var resumen = CrearResumen();

        resumen.RegistrarLectura(FechaHora, 1000m, EstadoPanel.Operativo, 15);
        var registrada = resumen.RegistrarLectura(
            FechaHora.AddSeconds(5),
            500m,
            EstadoPanel.EnFalla,
            15);

        Assert.True(registrada);
        Assert.Equal(1500m, resumen.PotenciaAcumuladaW);
        Assert.Equal(2, resumen.CantidadLecturas);
        Assert.Equal(500m / 1000m * 5m / 3600m, resumen.EnergiaKWh);
        Assert.Equal(1000m, resumen.PotenciaMaximaW);
        Assert.Equal(1, resumen.Fallas);
        Assert.Equal(EstadoPanel.EnFalla, resumen.UltimoEstado);
    }

    [Fact]
    public void RegistrarLectura_DuplicadaOFueraDeOrden_NoModificaElResumen()
    {
        var resumen = CrearResumen();
        resumen.RegistrarLectura(FechaHora, 1000m, EstadoPanel.Operativo, 15);
        var potencia = resumen.PotenciaAcumuladaW;

        var duplicada = resumen.RegistrarLectura(FechaHora, 2000m, EstadoPanel.EnFalla, 15);
        var anterior = resumen.RegistrarLectura(FechaHora.AddSeconds(-1), 2000m, EstadoPanel.EnFalla, 15);

        Assert.False(duplicada);
        Assert.False(anterior);
        Assert.Equal(potencia, resumen.PotenciaAcumuladaW);
        Assert.Equal(1, resumen.CantidadLecturas);
        Assert.Equal(0, resumen.Fallas);
    }

    [Fact]
    public void AgregarPanel_EsIdempotentePorPanel()
    {
        var resumen = CrearResumen();
        var panelId = Guid.NewGuid();
        var panel = ResumenDiarioPanel.Crear(resumen.Id, panelId, "P-01", Fecha);

        var agregado = resumen.AgregarPanel(panel);
        var repetido = resumen.AgregarPanel(ResumenDiarioPanel.Crear(resumen.Id, panelId, "P-01", Fecha));

        Assert.True(agregado);
        Assert.False(repetido);
        Assert.Single(resumen.Paneles);
        Assert.Equal(panelId, resumen.Paneles.Single().PanelId);
    }

    [Fact]
    public void AgregarPanel_ConFechaDistinta_LanzaExcepcion()
    {
        var resumen = CrearResumen();
        var panel = ResumenDiarioPanel.Crear(
            resumen.Id,
            Guid.NewGuid(),
            "P-01",
            Fecha.AddDays(1));

        Assert.Throws<ArgumentException>(() => resumen.AgregarPanel(panel));
    }
}
