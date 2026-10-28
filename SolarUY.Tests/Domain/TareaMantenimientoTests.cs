using System;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Exceptions;
using Xunit;

namespace SolarUY.Tests.Domain;

public class TareaMantenimientoTests
{
    private static TareaMantenimiento CrearTareaValida()
    {
        var panelId = Guid.NewGuid();
        var fecha = DateTime.Today;
        return new TareaMantenimiento(panelId, "Limpieza", "Limpiar paneles", "Juan Perez", fecha.AddDays(7), "Admin", "Observacion opcional");
    }

    [Fact]
    public void CrearTareaMantenimiento_ConDatosValidos_CreaInstancia()
    {
        var panelId = Guid.NewGuid();
        var fecha = DateTime.Today;
        var tarea = new TareaMantenimiento(panelId, "Revision", "Revisar conexion", "Maria Lopez", fecha.AddDays(3), "Admin", null);

        Assert.NotEqual(Guid.Empty, tarea.Id);
        Assert.Equal(panelId, tarea.PanelId);
        Assert.Equal("Revision", tarea.TipoTarea);
        Assert.Equal("Revisar conexion", tarea.Descripcion);
        Assert.Equal("Maria Lopez", tarea.AsignadoA);
        Assert.Equal("Pendiente", tarea.Estado);
        Assert.Equal("Admin", tarea.QuienAsigna);
        Assert.NotEqual(default(DateTime), tarea.FechaCreacion);
        Assert.Equal(DateTime.UtcNow.Date, tarea.FechaAsignacion.Date);
    }

    [Fact]
    public void CrearTareaMantenimiento_TrimNormaliza()
    {
        var fecha = DateTime.Today;
        var t2 = new TareaMantenimiento(Guid.NewGuid(), "  Limpieza  ", "  Desc  ", "  Ana  ", fecha.AddDays(1), "  User  ", "  obs  ");
        Assert.Equal("Limpieza", t2.TipoTarea);
        Assert.Equal("Desc", t2.Descripcion);
        Assert.Equal("Ana", t2.AsignadoA);
        Assert.Equal("Pendiente", t2.Estado);
        Assert.Equal("User", t2.QuienAsigna);
        Assert.Equal("obs", t2.Observaciones);
    }

    [Fact]
    public void CrearTareaMantenimiento_CamposObligatorios_Invalidos_LanzanExcepcion()
    {
        var fecha = DateTime.Today;
        Assert.Throws<ArgumentException>(() => new TareaMantenimiento(Guid.NewGuid(), "", "Desc", "Ana", fecha.AddDays(1), "Admin"));
        Assert.Throws<ArgumentException>(() => new TareaMantenimiento(Guid.NewGuid(), "Limpieza", "", "Ana", fecha.AddDays(1), "Admin"));
        Assert.Throws<ArgumentException>(() => new TareaMantenimiento(Guid.NewGuid(), "Limpieza", "Desc", "", fecha.AddDays(1), "Admin"));
        Assert.Throws<ArgumentException>(() => new TareaMantenimiento(Guid.NewGuid(), "Limpieza", "Desc", "Ana", fecha.AddDays(1), ""));
    }

    [Fact]
    public void CambiarEstado_PendienteAEnProceso_EstableceFechaInicio()
    {
        var tarea = CrearTareaValida();
        tarea.CambiarEstado("EnProceso");
        Assert.Equal("EnProceso", tarea.Estado);
        Assert.NotEqual(default(DateTime), tarea.FechaInicio);
    }

    [Fact]
    public void CambiarEstado_EnProcesoACompletada_ExigeQuienCompleta_YEstableceFechaFinalizacion()
    {
        var tarea = CrearTareaValida();
        tarea.CambiarEstado("EnProceso");
        tarea.CambiarEstado("Completada", "Carlos");
        Assert.Equal("Completada", tarea.Estado);
        Assert.Equal("Carlos", tarea.QuienCompleta);
        Assert.NotEqual(default(DateTime), tarea.FechaFinalizacion);
    }

    [Fact]
    public void CambiarEstado_Retroceder_LanzaExcepcion()
    {
        var tarea = CrearTareaValida();
        tarea.CambiarEstado("EnProceso");
        Assert.Throws<TareaMantenimientoEstadoInvalidoException>(() => tarea.CambiarEstado("Pendiente"));
    }

    [Fact]
    public void ActualizarDatos_TareaCompletada_LanzaTareaMantenimientoNoEditableException()
    {
        var tarea = CrearTareaValida();
        tarea.CambiarEstado("EnProceso");
        tarea.CambiarEstado("Completada", "Carlos");
        var fecha = DateTime.Today;
        Assert.Throws<TareaMantenimientoNoEditableException>(() => tarea.ActualizarDatos("Revision", "Nueva desc", "Luis", fecha.AddDays(2)));
    }

    [Fact]
    public void ActualizarDatos_TareaCancelada_LanzaTareaMantenimientoNoEditableException()
    {
        var tarea = CrearTareaValida();
        tarea.CambiarEstado("Cancelada");
        var fecha = DateTime.Today;
        Assert.Throws<TareaMantenimientoNoEditableException>(() => tarea.ActualizarDatos("Revision", "Nueva desc", "Luis", fecha.AddDays(2)));
    }

    [Fact]
    public void Entidad_Inmutabilidad_IdPanelIdFechaCreacion()
    {
        var panelId = Guid.NewGuid();
        var tarea = new TareaMantenimiento(panelId, "Limpieza", "Desc", "Pedro", DateTime.Today.AddDays(1), "Admin");
        var idOriginal = tarea.Id;
        var panelOriginal = tarea.PanelId;
        var fechaCreacionOriginal = tarea.FechaCreacion;
        tarea.CambiarEstado("EnProceso");
        tarea.ActualizarDatos("Revision", "Desc2", "Pedro", DateTime.Today.AddDays(5));
        Assert.Equal(idOriginal, tarea.Id);
        Assert.Equal(panelOriginal, tarea.PanelId);
        Assert.Equal(fechaCreacionOriginal, tarea.FechaCreacion);
    }
}
