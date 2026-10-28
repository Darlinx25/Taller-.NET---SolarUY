using System;
using SolarUY.Domain.Entities;
using Xunit;

namespace SolarUY.Tests.Domain;

public class PanelTests
{
    private static readonly Guid NodoId = Guid.NewGuid();

    [Fact]
    public void Crear_Panel_ConDatosValidos_AsignaValores()
    {
        var fechaInstalacion = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        var panel = new Panel(
            NodoId,
            "PAN-001",
            "Trina Solar",
            "TSM-550",
            550.5m,
            fechaInstalacion,
            "Panel principal");

        Assert.NotEqual(Guid.Empty, panel.Id);
        Assert.Equal(NodoId, panel.NodoId);
        Assert.Equal("PAN-001", panel.CodigoSerie);
        Assert.Equal("Trina Solar", panel.Marca);
        Assert.Equal("TSM-550", panel.Modelo);
        Assert.Equal(550.5m, panel.PotenciaWp);
        Assert.Equal(fechaInstalacion, panel.FechaInstalacion);
        Assert.Equal("Panel principal", panel.Observaciones);
        Assert.True(panel.FechaCreacion <= DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, panel.FechaCreacion.Kind);
    }

    [Fact]
    public void Crear_Panel_SinDatosOpcionales_SeAcepta()
    {
        var panel = new Panel(NodoId, "PAN-001", "Trina Solar", "TSM-550", 550m);

        Assert.Null(panel.FechaInstalacion);
        Assert.Null(panel.Observaciones);
    }

    [Fact]
    public void Crear_Panel_Trim_DebeQuitarEspacios()
    {
        var panel = new Panel(
            NodoId,
            "  PAN-001  ",
            "  Trina Solar  ",
            "  TSM-550  ",
            550m,
            null,
            "  Observación  ");

        Assert.Equal("PAN-001", panel.CodigoSerie);
        Assert.Equal("Trina Solar", panel.Marca);
        Assert.Equal("TSM-550", panel.Modelo);
        Assert.Equal("Observación", panel.Observaciones);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_Panel_ObservacionesVacias_SeNormalizanANull(string observaciones)
    {
        var panel = new Panel(NodoId, "PAN-001", "Trina Solar", "TSM-550", 550m, null, observaciones);

        Assert.Null(panel.Observaciones);
    }

    [Fact]
    public void Crear_Panel_NodoIdVacio_LanzaArgumentException()
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => new Panel(Guid.Empty, "PAN-001", "Trina Solar", "TSM-550", 550m));

        Assert.Equal("nodoId", excepcion.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_Panel_CodigoSerieVacio_LanzaArgumentException(string? codigoSerie)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => new Panel(NodoId, codigoSerie!, "Trina Solar", "TSM-550", 550m));

        Assert.Equal("codigoSerie", excepcion.ParamName);
    }

    [Fact]
    public void Crear_Panel_CodigoSerieDemasiadoLargo_LanzaArgumentException()
    {
        var codigoSerie = new string('a', Panel.CodigoSerieMaxLength + 1);

        var excepcion = Assert.Throws<ArgumentException>(
            () => new Panel(NodoId, codigoSerie, "Trina Solar", "TSM-550", 550m));

        Assert.Equal("codigoSerie", excepcion.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_Panel_MarcaVacia_LanzaArgumentException(string? marca)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => new Panel(NodoId, "PAN-001", marca!, "TSM-550", 550m));

        Assert.Equal("marca", excepcion.ParamName);
    }

    [Fact]
    public void Crear_Panel_MarcaDemasiadoLarga_LanzaArgumentException()
    {
        var marca = new string('a', Panel.MarcaMaxLength + 1);

        var excepcion = Assert.Throws<ArgumentException>(
            () => new Panel(NodoId, "PAN-001", marca, "TSM-550", 550m));

        Assert.Equal("marca", excepcion.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_Panel_ModeloVacio_LanzaArgumentException(string? modelo)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => new Panel(NodoId, "PAN-001", "Trina Solar", modelo!, 550m));

        Assert.Equal("modelo", excepcion.ParamName);
    }

    [Fact]
    public void Crear_Panel_ModeloDemasiadoLargo_LanzaArgumentException()
    {
        var modelo = new string('a', Panel.ModeloMaxLength + 1);

        var excepcion = Assert.Throws<ArgumentException>(
            () => new Panel(NodoId, "PAN-001", "Trina Solar", modelo, 550m));

        Assert.Equal("modelo", excepcion.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Crear_Panel_PotenciaNoPositiva_LanzaArgumentOutOfRangeException(decimal potenciaWp)
    {
        var excepcion = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Panel(NodoId, "PAN-001", "Trina Solar", "TSM-550", potenciaWp));

        Assert.Equal("potenciaWp", excepcion.ParamName);
    }

    [Fact]
    public void Crear_Panel_ObservacionesDemasiadoLargas_LanzaArgumentException()
    {
        var observaciones = new string('a', Panel.ObservacionesMaxLength + 1);

        var excepcion = Assert.Throws<ArgumentException>(
            () => new Panel(NodoId, "PAN-001", "Trina Solar", "TSM-550", 550m, null, observaciones));

        Assert.Equal("observaciones", excepcion.ParamName);
    }

    [Fact]
    public void ActualizarDatos_Panel_ActualizaValoresYConservaIdentidad()
    {
        var panel = new Panel(NodoId, "PAN-001", "Trina Solar", "TSM-550", 550m);
        var idOriginal = panel.Id;
        var fechaOriginal = panel.FechaCreacion;
        var nodoOriginal = panel.NodoId;
        var fechaInstalacion = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        panel.ActualizarDatos("PAN-002", "Jinko", "JKM-600", 600m, fechaInstalacion, "Actualizado");

        Assert.Equal(idOriginal, panel.Id);
        Assert.Equal(fechaOriginal, panel.FechaCreacion);
        Assert.Equal(nodoOriginal, panel.NodoId);
        Assert.Equal("PAN-002", panel.CodigoSerie);
        Assert.Equal("Jinko", panel.Marca);
        Assert.Equal("JKM-600", panel.Modelo);
        Assert.Equal(600m, panel.PotenciaWp);
        Assert.Equal(fechaInstalacion, panel.FechaInstalacion);
        Assert.Equal("Actualizado", panel.Observaciones);
    }

    [Fact]
    public void ActualizarDatos_Panel_ObservacionesVacias_SeNormalizanANull()
    {
        var panel = new Panel(NodoId, "PAN-001", "Trina Solar", "TSM-550", 550m, null, "Inicial");

        panel.ActualizarDatos("PAN-001", "Trina Solar", "TSM-550", 550m, null, "   ");

        Assert.Null(panel.Observaciones);
    }

    [Fact]
    public void ActualizarDatos_Panel_CodigoSerieVacio_LanzaArgumentException()
    {
        var panel = new Panel(NodoId, "PAN-001", "Trina Solar", "TSM-550", 550m);

        var excepcion = Assert.Throws<ArgumentException>(
            () => panel.ActualizarDatos("", "Trina Solar", "TSM-550", 550m, null, null));

        Assert.Equal("codigoSerie", excepcion.ParamName);
    }

    [Fact]
    public void ActualizarDatos_Panel_PotenciaNoPositiva_LanzaArgumentOutOfRangeException()
    {
        var panel = new Panel(NodoId, "PAN-001", "Trina Solar", "TSM-550", 550m);

        var excepcion = Assert.Throws<ArgumentOutOfRangeException>(
            () => panel.ActualizarDatos("PAN-001", "Trina Solar", "TSM-550", 0m, null, null));

        Assert.Equal("potenciaWp", excepcion.ParamName);
    }
}
