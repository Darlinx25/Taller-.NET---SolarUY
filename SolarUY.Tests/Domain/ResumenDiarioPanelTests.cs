using System;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Enums;
using Xunit;

namespace SolarUY.Tests.Domain;

public class ResumenDiarioPanelTests
{
    private static readonly DateOnly Fecha = new(2026, 1, 15);
    private static readonly DateTime FechaHora = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid ResumenNodoId = Guid.NewGuid();
    private static readonly Guid PanelId = Guid.NewGuid();

    private static ResumenDiarioPanel CrearResumen()
    {
        return ResumenDiarioPanel.Crear(ResumenNodoId, PanelId, "P-01", Fecha);
    }

    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var resumen = CrearResumen();

        Assert.NotEqual(Guid.Empty, resumen.Id);
        Assert.Equal(ResumenNodoId, resumen.ResumenDiarioNodoId);
        Assert.Equal(PanelId, resumen.PanelId);
        Assert.Equal("P-01", resumen.CodigoSerie);
        Assert.Equal(Fecha, resumen.FechaLocal);
        Assert.Equal(0, resumen.CantidadLecturas);
    }

    [Fact]
    public void Crear_DatosInvalidos_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(
            () => ResumenDiarioPanel.Crear(Guid.Empty, PanelId, "P-01", Fecha));

        Assert.Throws<ArgumentException>(
            () => ResumenDiarioPanel.Crear(ResumenNodoId, Guid.Empty, "P-01", Fecha));

        Assert.Throws<ArgumentException>(
            () => ResumenDiarioPanel.Crear(ResumenNodoId, PanelId, " ", Fecha));

        Assert.Throws<ArgumentException>(
            () => ResumenDiarioPanel.Crear(ResumenNodoId, PanelId, "P-01", default));
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
    public void RegistrarLectura_ConBrechaMayorAQuinceSegundos_LimitaLaEnergia()
    {
        var resumen = CrearResumen();
        resumen.RegistrarLectura(FechaHora, 1000m, EstadoPanel.Operativo, 15);

        resumen.RegistrarLectura(FechaHora.AddSeconds(60), 1000m, EstadoPanel.Operativo, 15);

        Assert.Equal(15m / 3600m, resumen.EnergiaKWh);
    }
}
