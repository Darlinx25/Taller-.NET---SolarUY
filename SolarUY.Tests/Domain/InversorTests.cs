using System;
using SolarUY.Domain.Entities;
using Xunit;

namespace SolarUY.Tests.Domain;

public class InversorTests
{
    [Fact]
    public void Crear_Inversor_ConDatosValidos_AsignaValores()
    {
        var inversor = new Inversor("Huawei", "SUN2000", "SN123456", 100m);

        Assert.NotEqual(Guid.Empty, inversor.Id);
        Assert.Equal("Huawei", inversor.Marca);
        Assert.Equal("SUN2000", inversor.Modelo);
        Assert.Equal("SN123456", inversor.NumeroSerie);
        Assert.Equal(100m, inversor.PotenciaNominalkW);
    }

    [Fact]
    public void Crear_Inversor_Trim_DebeQuitarEspacios()
    {
        var inversor = new Inversor("  Huawei  ", "  SUN2000  ", "  SN123  ", 50m);

        Assert.Equal("Huawei", inversor.Marca);
        Assert.Equal("SUN2000", inversor.Modelo);
        Assert.Equal("SN123", inversor.NumeroSerie);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Crear_Inversor_MarcaVacia_LanzaArgumentException(string marca)
    {
        Assert.Throws<ArgumentException>(() => new Inversor(marca, "Modelo", "SN1"));
    }

    [Fact]
    public void Crear_Inversor_MarcaNull_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Inversor(null!, "Modelo", "SN1"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Crear_Inversor_ModeloVacio_LanzaArgumentException(string modelo)
    {
        Assert.Throws<ArgumentException>(() => new Inversor("Marca", modelo, "SN1"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Crear_Inversor_NumeroSerieVacio_LanzaArgumentException(string numeroSerie)
    {
        Assert.Throws<ArgumentException>(() => new Inversor("Marca", "Modelo", numeroSerie));
    }

    [Fact]
    public void Crear_Inversor_PotenciaNegativa_LanzaArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Inversor("Marca", "Modelo", "SN1", -1m));
    }

    [Fact]
    public void ActualizarDatos_Inversor_ActualizaValores()
    {
        var inversor = new Inversor("Marca", "Modelo", "SN1", 10m);
        var idOriginal = inversor.Id;
        var fechaOriginal = inversor.FechaCreacion;

        inversor.ActualizarDatos("Marca2", "Modelo2", "SN2", 20m);

        Assert.Equal(idOriginal, inversor.Id);
        Assert.Equal(fechaOriginal, inversor.FechaCreacion);
        Assert.Equal("Marca2", inversor.Marca);
        Assert.Equal("Modelo2", inversor.Modelo);
        Assert.Equal("SN2", inversor.NumeroSerie);
        Assert.Equal(20m, inversor.PotenciaNominalkW);
    }

    [Fact]
    public void Eliminar_MarcaElInversorComoEliminadoYRegistraLaFecha()
    {
        var inversor = new Inversor("Marca", "Modelo", "SN1");

        inversor.Eliminar();

        Assert.True(inversor.Eliminado);
        Assert.NotNull(inversor.FechaEliminacion);
        Assert.Equal(DateTimeKind.Utc, inversor.FechaEliminacion!.Value.Kind);
    }
}
