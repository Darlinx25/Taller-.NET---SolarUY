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
}
