using SolarUY.Domain.Entities;
using Xunit;

namespace SolarUY.Tests.Domain;

/// <summary>
/// Pruebas unitarias del comportamiento de la entidad de dominio <see cref="ParqueSolar"/>.
/// </summary>
public class ParqueSolarTests
{
    [Fact]
    public void Constructor_ConDatosValidos_AsignaElEstadoDelParque()
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12", 25.5m);

        Assert.NotEqual(Guid.Empty, parque.Id);
        Assert.Equal("Parque Manga", parque.Nombre);
        Assert.Equal("Ruta 5 km 12", parque.Ubicacion);
        Assert.Equal(25.5m, parque.CapacidadEstimadaMW);
        Assert.True(parque.TieneCapacidadEstimada);
        Assert.True(parque.FechaCreacion <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_SinCapacidadEstimada_DejaLaCapacidadSinValor()
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12");

        Assert.Null(parque.CapacidadEstimadaMW);
        Assert.False(parque.TieneCapacidadEstimada);
    }

    [Fact]
    public void Constructor_RecortaLosEspaciosDelNombreYLaUbicacion()
    {
        var parque = new ParqueSolar("  Parque Manga  ", "  Ruta 5 km 12  ");

        Assert.Equal("Parque Manga", parque.Nombre);
        Assert.Equal("Ruta 5 km 12", parque.Ubicacion);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NombreVacio_LanzaArgumentException(string? nombre)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => new ParqueSolar(nombre!, "Ruta 5 km 12"));

        Assert.Equal("nombre", excepcion.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_UbicacionVacia_LanzaArgumentException(string? ubicacion)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => new ParqueSolar("Parque Manga", ubicacion!));

        Assert.Equal("ubicacion", excepcion.ParamName);
    }

    [Fact]
    public void Constructor_NombreDemasiadoLargo_LanzaArgumentException()
    {
        var nombre = new string('a', ParqueSolar.NombreMaxLength + 1);

        var excepcion = Assert.Throws<ArgumentException>(
            () => new ParqueSolar(nombre, "Ruta 5 km 12"));

        Assert.Equal("nombre", excepcion.ParamName);
    }

    [Fact]
    public void Constructor_UbicacionDemasiadoLarga_LanzaArgumentException()
    {
        var ubicacion = new string('b', ParqueSolar.UbicacionMaxLength + 1);

        var excepcion = Assert.Throws<ArgumentException>(
            () => new ParqueSolar("Parque Manga", ubicacion));

        Assert.Equal("ubicacion", excepcion.ParamName);
    }

    [Fact]
    public void Constructor_CapacidadNegativa_LanzaArgumentOutOfRangeException()
    {
        var excepcion = Assert.Throws<ArgumentOutOfRangeException>(
            () => new ParqueSolar("Parque Manga", "Ruta 5 km 12", -1m));

        Assert.Equal("capacidadEstimadaMW", excepcion.ParamName);
    }

    [Fact]
    public void ActualizarCapacidadEstimada_ConValorValido_ActualizaLaCapacidad()
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12");

        parque.ActualizarCapacidadEstimada(42.75m);

        Assert.Equal(42.75m, parque.CapacidadEstimadaMW);
    }

    [Fact]
    public void ActualizarCapacidadEstimada_ConValorNegativo_LanzaArgumentOutOfRangeException()
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12");

        Assert.Throws<ArgumentOutOfRangeException>(() => parque.ActualizarCapacidadEstimada(-0.01m));
    }

    [Fact]
    public void ActualizarDatos_ConDatosValidos_ActualizaElEstadoDelParque()
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12", 25.5m);

        parque.ActualizarDatos("Parque Norte", "Ruta 8 km 20", 30.75m);

        Assert.Equal("Parque Norte", parque.Nombre);
        Assert.Equal("Ruta 8 km 20", parque.Ubicacion);
        Assert.Equal(30.75m, parque.CapacidadEstimadaMW);
    }

    [Fact]
    public void ActualizarDatos_RecortaLosEspaciosDelNombreYLaUbicacion()
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12");

        parque.ActualizarDatos("  Parque Norte  ", "  Ruta 8 km 20  ", null);

        Assert.Equal("Parque Norte", parque.Nombre);
        Assert.Equal("Ruta 8 km 20", parque.Ubicacion);
        Assert.Null(parque.CapacidadEstimadaMW);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ActualizarDatos_NombreVacio_LanzaArgumentException(string? nombre)
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12");

        var excepcion = Assert.Throws<ArgumentException>(
            () => parque.ActualizarDatos(nombre!, "Ruta 8 km 20", null));

        Assert.Equal("nombre", excepcion.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ActualizarDatos_UbicacionVacia_LanzaArgumentException(string? ubicacion)
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12");

        var excepcion = Assert.Throws<ArgumentException>(
            () => parque.ActualizarDatos("Parque Norte", ubicacion!, null));

        Assert.Equal("ubicacion", excepcion.ParamName);
    }

    [Fact]
    public void ActualizarDatos_NombreDemasiadoLargo_LanzaArgumentException()
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12");
        var nombre = new string('a', ParqueSolar.NombreMaxLength + 1);

        var excepcion = Assert.Throws<ArgumentException>(
            () => parque.ActualizarDatos(nombre, "Ruta 8 km 20", null));

        Assert.Equal("nombre", excepcion.ParamName);
    }

    [Fact]
    public void ActualizarDatos_UbicacionDemasiadoLarga_LanzaArgumentException()
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12");
        var ubicacion = new string('b', ParqueSolar.UbicacionMaxLength + 1);

        var excepcion = Assert.Throws<ArgumentException>(
            () => parque.ActualizarDatos("Parque Norte", ubicacion, null));

        Assert.Equal("ubicacion", excepcion.ParamName);
    }

    [Fact]
    public void ActualizarDatos_CapacidadNegativa_LanzaArgumentOutOfRangeException()
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12");

        var excepcion = Assert.Throws<ArgumentOutOfRangeException>(
            () => parque.ActualizarDatos("Parque Norte", "Ruta 8 km 20", -1m));

        Assert.Equal("capacidadEstimadaMW", excepcion.ParamName);
    }

    [Fact]
    public void ActualizarDatos_NoModificaElIdNiLaFechaDeCreacion()
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12");
        var idOriginal = parque.Id;
        var fechaCreacionOriginal = parque.FechaCreacion;

        parque.ActualizarDatos("Parque Norte", "Ruta 8 km 20", 10m);

        Assert.Equal(idOriginal, parque.Id);
        Assert.Equal(fechaCreacionOriginal, parque.FechaCreacion);
    }

    [Fact]
    public void Eliminar_MarcaElParqueComoEliminadoYRegistraLaFecha()
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12");

        parque.Eliminar();

        Assert.True(parque.Eliminado);
        Assert.NotNull(parque.FechaEliminacion);
        Assert.Equal(DateTimeKind.Utc, parque.FechaEliminacion!.Value.Kind);
    }

    [Fact]
    public void Eliminar_EsIdempotenteYConservaLaFechaOriginal()
    {
        var parque = new ParqueSolar("Parque Manga", "Ruta 5 km 12");

        parque.Eliminar();
        var fechaOriginal = parque.FechaEliminacion;

        parque.Eliminar();

        Assert.Equal(fechaOriginal, parque.FechaEliminacion);
    }
}
