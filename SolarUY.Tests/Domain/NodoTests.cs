using System;
using SolarUY.Domain.Entities;
using Xunit;

namespace SolarUY.Tests.Domain;

public class NodoTests
{
    [Fact]
    public void Crear_NodoConInversor_ConDatosValidos_AsignaValores()
    {
        var inversor = new Inversor("Huawei", "SUN2000", "SN123456", 100m);
        var parqueId = Guid.NewGuid();
        var nodo = new Nodo("N-01", parqueId, inversor, "Nodo principal", "Activo");

        Assert.NotEqual(Guid.Empty, nodo.Id);
        Assert.Equal("N-01", nodo.Codigo);
        Assert.Equal(parqueId, nodo.ParqueSolarId);
        Assert.Same(inversor, nodo.Inversor);
        Assert.Equal("Nodo principal", nodo.Descripcion);
        Assert.Equal("Activo", nodo.Estado);
    }

    [Fact]
    public void Crear_Nodo_Trim_DebeQuitarEspacios()
    {
        var inversor = new Inversor("Huawei", "SUN2000", "SN123456", 100m);
        var parqueId = Guid.NewGuid();
        var nodo = new Nodo("  N-01  ", parqueId, inversor, "  Desc  ", "  Activo  ");

        Assert.Equal("N-01", nodo.Codigo);
        Assert.Equal("Desc", nodo.Descripcion);
        Assert.Equal("Activo", nodo.Estado);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Crear_Nodo_CodigoVacio_LanzaArgumentException(string codigo)
    {
        var inversor = new Inversor("Huawei", "SUN2000", "SN123456");
        Assert.Throws<ArgumentException>(() => new Nodo(codigo, Guid.NewGuid(), inversor));
    }

    [Fact]
    public void Crear_Nodo_ParqueSolarIdVacio_LanzaArgumentException()
    {
        var inversor = new Inversor("Huawei", "SUN2000", "SN123456");
        Assert.Throws<ArgumentException>(() => new Nodo("N-01", Guid.Empty, inversor));
    }

    [Fact]
    public void Crear_Nodo_InversorNulo_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Nodo("N-01", Guid.NewGuid(), null!));
    }

    [Fact]
    public void Crear_Nodo_DescripcionNull_SeAcepta()
    {
        var inversor = new Inversor("Huawei", "SUN2000", "SN123456");
        var nodo = new Nodo("N-01", Guid.NewGuid(), inversor, null, "Activo");

        Assert.Null(nodo.Descripcion);
    }

    [Fact]
    public void ActualizarDatos_Nodo_ActualizaValores()
    {
        var inversor = new Inversor("Huawei", "SUN2000", "SN123456");
        var nodo = new Nodo("N-01", Guid.NewGuid(), inversor);
        var idOriginal = nodo.Id;
        var fechaOriginal = nodo.FechaCreacion;

        nodo.ActualizarDatos("N-02", "Nueva descripcion", "Inactivo");

        Assert.Equal(idOriginal, nodo.Id);
        Assert.Equal(fechaOriginal, nodo.FechaCreacion);
        Assert.Equal("N-02", nodo.Codigo);
        Assert.Equal("Nueva descripcion", nodo.Descripcion);
        Assert.Equal("Inactivo", nodo.Estado);
    }

    [Fact]
    public void ActualizarDatos_Nodo_DescripcionVacia_SeConvierteANull()
    {
        var inversor = new Inversor("Huawei", "SUN2000", "SN123456");
        var nodo = new Nodo("N-01", Guid.NewGuid(), inversor, "Desc");
        nodo.ActualizarDatos("N-01", "   ", "Activo");
        Assert.Null(nodo.Descripcion);
    }
}
