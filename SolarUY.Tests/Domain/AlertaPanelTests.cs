using System;
using SolarUY.Domain.Entities;
using Xunit;

namespace SolarUY.Tests.Domain;

public class AlertaPanelTests
{
    private static readonly Guid ParqueId = Guid.NewGuid();
    private static readonly Guid NodoId = Guid.NewGuid();
    private static readonly Guid PanelId = Guid.NewGuid();
    private static readonly DateTime Inicio = new(2026, 11, 5, 16, 0, 0, DateTimeKind.Utc);

    private static AlertaPanel Abrir(DateTime? inicio = null) =>
        AlertaPanel.Abrir(ParqueId, "Parque Norte", NodoId, "N-01", PanelId, "PAN-001", inicio ?? Inicio);

    private static AlertaPanel AbrirYCerrar(DateTime fin)
    {
        var alerta = Abrir();
        Assert.True(alerta.Cerrar(fin));
        return alerta;
    }

    // ---------------------------------------------------------------- Abrir

    [Fact]
    public void Abrir_ConDatosValidos_CreaAlertaActiva()
    {
        var alerta = AlertaPanel.Abrir(
            ParqueId, "  Parque Norte  ", NodoId, "  N-01 ", PanelId, " PAN-001  ", Inicio);

        Assert.NotEqual(Guid.Empty, alerta.Id);
        Assert.Equal(ParqueId, alerta.ParqueSolarId);
        Assert.Equal("Parque Norte", alerta.NombreParque);
        Assert.Equal(NodoId, alerta.NodoId);
        Assert.Equal("N-01", alerta.CodigoNodo);
        Assert.Equal(PanelId, alerta.PanelId);
        Assert.Equal("PAN-001", alerta.CodigoSerie);
        Assert.Equal(Inicio, alerta.FechaInicioUtc);
        Assert.Null(alerta.FechaFinUtc);
        Assert.True(alerta.EstaActiva);
    }

    [Fact]
    public void Abrir_ConLongitudesMaximas_SeAcepta()
    {
        var alerta = AlertaPanel.Abrir(
            ParqueId,
            new string('P', AlertaPanel.NombreParqueMaxLength),
            NodoId,
            new string('N', AlertaPanel.CodigoNodoMaxLength),
            PanelId,
            new string('S', AlertaPanel.CodigoSerieMaxLength),
            Inicio);

        Assert.Equal(AlertaPanel.NombreParqueMaxLength, alerta.NombreParque.Length);
    }

    [Fact]
    public void Abrir_ConParqueIdVacio_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            AlertaPanel.Abrir(Guid.Empty, "Parque Norte", NodoId, "N-01", PanelId, "PAN-001", Inicio));
    }

    [Fact]
    public void Abrir_ConNodoIdVacio_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            AlertaPanel.Abrir(ParqueId, "Parque Norte", Guid.Empty, "N-01", PanelId, "PAN-001", Inicio));
    }

    [Fact]
    public void Abrir_ConPanelIdVacio_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() =>
            AlertaPanel.Abrir(ParqueId, "Parque Norte", NodoId, "N-01", Guid.Empty, "PAN-001", Inicio));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Abrir_ConNombreParqueVacio_LanzaExcepcion(string? nombre)
    {
        Assert.Throws<ArgumentException>(() =>
            AlertaPanel.Abrir(ParqueId, nombre!, NodoId, "N-01", PanelId, "PAN-001", Inicio));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Abrir_ConCodigoNodoVacio_LanzaExcepcion(string? codigo)
    {
        Assert.Throws<ArgumentException>(() =>
            AlertaPanel.Abrir(ParqueId, "Parque Norte", NodoId, codigo!, PanelId, "PAN-001", Inicio));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Abrir_ConCodigoSerieVacio_LanzaExcepcion(string? codigo)
    {
        Assert.Throws<ArgumentException>(() =>
            AlertaPanel.Abrir(ParqueId, "Parque Norte", NodoId, "N-01", PanelId, codigo!, Inicio));
    }

    [Fact]
    public void Abrir_ConNombreParqueMuyLargo_LanzaExcepcion()
    {
        var nombre = new string('P', AlertaPanel.NombreParqueMaxLength + 1);

        Assert.Throws<ArgumentException>(() =>
            AlertaPanel.Abrir(ParqueId, nombre, NodoId, "N-01", PanelId, "PAN-001", Inicio));
    }

    [Fact]
    public void Abrir_ConCodigoNodoMuyLargo_LanzaExcepcion()
    {
        var codigo = new string('N', AlertaPanel.CodigoNodoMaxLength + 1);

        Assert.Throws<ArgumentException>(() =>
            AlertaPanel.Abrir(ParqueId, "Parque Norte", NodoId, codigo, PanelId, "PAN-001", Inicio));
    }

    [Fact]
    public void Abrir_ConCodigoSerieMuyLargo_LanzaExcepcion()
    {
        var codigo = new string('S', AlertaPanel.CodigoSerieMaxLength + 1);

        Assert.Throws<ArgumentException>(() =>
            AlertaPanel.Abrir(ParqueId, "Parque Norte", NodoId, "N-01", PanelId, codigo, Inicio));
    }

    [Fact]
    public void Abrir_ConFechaInicioPorDefecto_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => Abrir(default(DateTime)));
    }

    [Fact]
    public void Abrir_DosAlertas_TienenIdsDistintos()
    {
        Assert.NotEqual(Abrir().Id, Abrir().Id);
    }

    // --------------------------------------------------------------- Cerrar

    [Fact]
    public void Cerrar_ConFechaPosteriorAlInicio_CierraYDevuelveTrue()
    {
        var alerta = Abrir();
        var fin = Inicio.AddSeconds(30);

        var resultado = alerta.Cerrar(fin);

        Assert.True(resultado);
        Assert.Equal(fin, alerta.FechaFinUtc);
        Assert.False(alerta.EstaActiva);
    }

    [Fact]
    public void Cerrar_ConFechaIgualAlInicio_DevuelveFalseYSigueActiva()
    {
        var alerta = Abrir();

        var resultado = alerta.Cerrar(Inicio);

        Assert.False(resultado);
        Assert.Null(alerta.FechaFinUtc);
        Assert.True(alerta.EstaActiva);
    }

    [Fact]
    public void Cerrar_ConFechaAnteriorAlInicio_DevuelveFalseYSigueActiva()
    {
        var alerta = Abrir();

        var resultado = alerta.Cerrar(Inicio.AddSeconds(-5));

        Assert.False(resultado);
        Assert.True(alerta.EstaActiva);
    }

    [Fact]
    public void Cerrar_ConFechaPorDefecto_LanzaExcepcion()
    {
        var alerta = Abrir();

        Assert.Throws<ArgumentException>(() => alerta.Cerrar(default));
        Assert.True(alerta.EstaActiva);
    }

    [Fact]
    public void Cerrar_SobreAlertaCerrada_LanzaExcepcion()
    {
        var alerta = AbrirYCerrar(Inicio.AddSeconds(30));

        Assert.Throws<InvalidOperationException>(() => alerta.Cerrar(Inicio.AddSeconds(60)));
        Assert.Equal(Inicio.AddSeconds(30), alerta.FechaFinUtc);
    }

    // ------------------------------------------------------ CalcularDuracion

    [Fact]
    public void CalcularDuracion_AlertaCerrada_UsaSuFechaDeFin()
    {
        var alerta = AbrirYCerrar(Inicio.AddSeconds(30));

        var duracion = alerta.CalcularDuracion(Inicio.AddHours(5));

        Assert.Equal(TimeSpan.FromSeconds(30), duracion);
    }

    [Fact]
    public void CalcularDuracion_AlertaActiva_LlegaHastaLaFechaDeReferencia()
    {
        var alerta = Abrir();

        var duracion = alerta.CalcularDuracion(Inicio.AddMinutes(90));

        Assert.Equal(TimeSpan.FromMinutes(90), duracion);
    }

    [Fact]
    public void CalcularDuracion_ConReferenciaAnteriorAlInicio_NoEsNegativa()
    {
        var alerta = Abrir();

        var duracion = alerta.CalcularDuracion(Inicio.AddMinutes(-10));

        Assert.Equal(TimeSpan.Zero, duracion);
    }

    [Fact]
    public void CalcularDuracion_ConFechaPorDefecto_LanzaExcepcion()
    {
        var alerta = Abrir();

        Assert.Throws<ArgumentException>(() => alerta.CalcularDuracion(default));
    }

    // ----------------------------------------------------------- DebeAbrirse

    [Fact]
    public void DebeAbrirse_SinAlertaPrevia_DevuelveTrue()
    {
        Assert.True(AlertaPanel.DebeAbrirse(null, Inicio));
    }

    [Fact]
    public void DebeAbrirse_ConAlertaPreviaActiva_DevuelveFalse()
    {
        var previa = Abrir();

        Assert.False(AlertaPanel.DebeAbrirse(previa, Inicio.AddMinutes(5)));
    }

    [Fact]
    public void DebeAbrirse_ConPreviaCerradaYFechaPosteriorAlFin_DevuelveTrue()
    {
        var previa = AbrirYCerrar(Inicio.AddSeconds(30));

        Assert.True(AlertaPanel.DebeAbrirse(previa, Inicio.AddSeconds(35)));
    }

    [Fact]
    public void DebeAbrirse_ConPreviaCerradaYFechaIgualAlFin_DevuelveFalse()
    {
        var previa = AbrirYCerrar(Inicio.AddSeconds(30));

        Assert.False(AlertaPanel.DebeAbrirse(previa, Inicio.AddSeconds(30)));
    }

    [Fact]
    public void DebeAbrirse_ConPreviaCerradaYFechaAnteriorAlFin_DevuelveFalse()
    {
        var previa = AbrirYCerrar(Inicio.AddSeconds(30));

        Assert.False(AlertaPanel.DebeAbrirse(previa, Inicio.AddSeconds(10)));
    }

    [Fact]
    public void DebeAbrirse_ConFechaPorDefecto_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(() => AlertaPanel.DebeAbrirse(null, default));
    }
}
