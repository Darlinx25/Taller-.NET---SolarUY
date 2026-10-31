using System;
using System.Linq;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Enums;
using Xunit;

namespace SolarUY.Tests.Domain;

public class ResumenDiarioParqueTests
{
    private static readonly DateOnly Fecha = new(2026, 1, 15);
    private static readonly DateTime FechaHora = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid ParqueId = Guid.NewGuid();

    private static ResumenDiarioParque CrearResumen()
    {
        return ResumenDiarioParque.Crear(ParqueId, "Parque Norte", Fecha);
    }

    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var resumen = CrearResumen();

        Assert.NotEqual(Guid.Empty, resumen.Id);
        Assert.Equal(ParqueId, resumen.ParqueSolarId);
        Assert.Equal("Parque Norte", resumen.NombreParque);
        Assert.Equal(Fecha, resumen.FechaLocal);
        Assert.Equal(0, resumen.CantidadLecturas);
        Assert.Equal(EstadoPanel.Operativo, resumen.UltimoEstado);
        Assert.Empty(resumen.Nodos);
    }

    [Fact]
    public void Crear_DatosInvalidos_LanzaExcepcion()
    {
        Assert.Throws<ArgumentException>(
            () => ResumenDiarioParque.Crear(Guid.Empty, "Parque", Fecha));

        Assert.Throws<ArgumentException>(
            () => ResumenDiarioParque.Crear(ParqueId, " ", Fecha));

        Assert.Throws<ArgumentException>(
            () => ResumenDiarioParque.Crear(ParqueId, "Parque", default));
    }

    [Fact]
    public void RegistrarLectura_PrimeraLectura_IniciaAcumuladosConEnergiaCero()
    {
        var resumen = CrearResumen();

        var registrada = resumen.RegistrarLectura(FechaHora, 1000m, EstadoPanel.Operativo, 15);

        Assert.True(registrada);
        Assert.Equal(1000m, resumen.PotenciaAcumuladaW);
        Assert.Equal(1, resumen.CantidadLecturas);
        Assert.Equal(0m, resumen.EnergiaKWh);
        Assert.Equal(1000m, resumen.PotenciaMaximaW);
        Assert.Equal(0, resumen.Fallas);
        Assert.Equal(EstadoPanel.Operativo, resumen.UltimoEstado);
        Assert.Equal(FechaHora, resumen.UltimaFechaHoraUtc);
    }

    [Fact]
    public void RegistrarLectura_SegundaLectura_AcumulaEnergiaPotenciaYFallas()
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
        var lecturas = resumen.CantidadLecturas;

        var duplicada = resumen.RegistrarLectura(FechaHora, 2000m, EstadoPanel.EnFalla, 15);
        var anterior = resumen.RegistrarLectura(FechaHora.AddSeconds(-1), 2000m, EstadoPanel.EnFalla, 15);

        Assert.False(duplicada);
        Assert.False(anterior);
        Assert.Equal(potencia, resumen.PotenciaAcumuladaW);
        Assert.Equal(lecturas, resumen.CantidadLecturas);
        Assert.Equal(0, resumen.Fallas);
        Assert.Equal(FechaHora, resumen.UltimaFechaHoraUtc);
    }

    [Fact]
    public void AgregarNodo_EsIdempotentePorNodo()
    {
        var resumen = CrearResumen();
        var nodoId = Guid.NewGuid();
        var nodo = ResumenDiarioNodo.Crear(resumen.Id, nodoId, "N-01", Fecha);

        var agregado = resumen.AgregarNodo(nodo);
        var repetido = resumen.AgregarNodo(ResumenDiarioNodo.Crear(resumen.Id, nodoId, "N-01", Fecha));

        Assert.True(agregado);
        Assert.False(repetido);
        Assert.Single(resumen.Nodos);
        Assert.Equal(nodoId, resumen.Nodos.Single().NodoId);
    }

    [Fact]
    public void AgregarNodo_ConFechaDistinta_LanzaExcepcion()
    {
        var resumen = CrearResumen();
        var nodo = ResumenDiarioNodo.Crear(
            resumen.Id,
            Guid.NewGuid(),
            "N-01",
            Fecha.AddDays(1));

        Assert.Throws<ArgumentException>(() => resumen.AgregarNodo(nodo));
    }
}
