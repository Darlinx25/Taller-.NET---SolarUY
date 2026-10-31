using System;
using SolarUY.Domain.Enums;
using SolarUY.Domain.Exceptions;
using SolarUY.Domain.ValueObjects;
using Xunit;

namespace SolarUY.Tests.Domain;

public class FiltroHistoricoTests
{
    private static readonly DateOnly Desde = new(2026, 1, 1);
    private static readonly DateOnly Hasta = new(2026, 1, 31);

    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var entidadId = Guid.NewGuid();

        var filtro = FiltroHistorico.Crear(
            TipoEntidadHistorico.Panel,
            entidadId,
            Desde,
            Hasta,
            AgrupacionHistorico.Mes);

        Assert.Equal(TipoEntidadHistorico.Panel, filtro.TipoEntidad);
        Assert.Equal(entidadId, filtro.EntidadId);
        Assert.Equal(Desde, filtro.Desde);
        Assert.Equal(Hasta, filtro.Hasta);
        Assert.Equal(AgrupacionHistorico.Mes, filtro.Agrupacion);
    }

    [Fact]
    public void Crear_SinEntidad_EsValido()
    {
        var filtro = FiltroHistorico.Crear(
            TipoEntidadHistorico.Parque,
            null,
            Desde,
            Hasta,
            AgrupacionHistorico.Dia);

        Assert.Null(filtro.EntidadId);
    }

    [Fact]
    public void Crear_RangoInvertido_LanzaExcepcion()
    {
        Assert.Throws<FiltroHistoricoInvalidoException>(
            () => FiltroHistorico.Crear(
                TipoEntidadHistorico.Parque,
                null,
                Hasta,
                Desde,
                AgrupacionHistorico.Dia));
    }

    [Fact]
    public void Crear_FechasVacias_LanzaExcepcion()
    {
        Assert.Throws<FiltroHistoricoInvalidoException>(
            () => FiltroHistorico.Crear(
                TipoEntidadHistorico.Parque,
                null,
                default,
                Hasta,
                AgrupacionHistorico.Dia));

        Assert.Throws<FiltroHistoricoInvalidoException>(
            () => FiltroHistorico.Crear(
                TipoEntidadHistorico.Parque,
                null,
                Desde,
                default,
                AgrupacionHistorico.Dia));
    }

    [Fact]
    public void Crear_EntidadVacia_LanzaExcepcion()
    {
        Assert.Throws<FiltroHistoricoInvalidoException>(
            () => FiltroHistorico.Crear(
                TipoEntidadHistorico.Nodo,
                Guid.Empty,
                Desde,
                Hasta,
                AgrupacionHistorico.Dia));
    }

    [Fact]
    public void Crear_EnumsInvalidos_LanzanExcepcion()
    {
        Assert.Throws<FiltroHistoricoInvalidoException>(
            () => FiltroHistorico.Crear(
                (TipoEntidadHistorico)99,
                null,
                Desde,
                Hasta,
                AgrupacionHistorico.Dia));

        Assert.Throws<FiltroHistoricoInvalidoException>(
            () => FiltroHistorico.Crear(
                TipoEntidadHistorico.Parque,
                null,
                Desde,
                Hasta,
                (AgrupacionHistorico)99));
    }
}
