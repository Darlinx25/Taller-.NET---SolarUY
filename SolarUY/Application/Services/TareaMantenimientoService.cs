using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SolarUY.Application.Dtos;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Exceptions;
using SolarUY.Domain.Repositories;

namespace SolarUY.Application.Services;

public class TareaMantenimientoService : ITareaMantenimientoService
{
    private readonly ITareaMantenimientoRepository _tareaMantenimientoRepository;
    private readonly IPanelRepository _panelRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TareaMantenimientoService(ITareaMantenimientoRepository tareaMantenimientoRepository, IPanelRepository panelRepository, IHttpContextAccessor httpContextAccessor)
    {
        _tareaMantenimientoRepository = tareaMantenimientoRepository;
        _panelRepository = panelRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyList<TareaMantenimientoDto>> ObtenerTareasPorPanelAsync(Guid panelId, CancellationToken cancellationToken = default)
    {
        var tareas = await _tareaMantenimientoRepository.ObtenerPorPanelAsync(panelId, cancellationToken);
        return tareas.Select(MapearADto).ToList();
    }

    public async Task<TareaMantenimientoDetalleDto> ObtenerTareaAsync(Guid tareaId, CancellationToken cancellationToken = default)
    {
        var tarea = await _tareaMantenimientoRepository.ObtenerPorIdAsync(tareaId, cancellationToken);
        if (tarea is null)
            throw new TareaMantenimientoNoEncontradaException(tareaId);

        var dto = MapearADetalleDto(tarea);
        return dto;
    }

    public async Task<TareaMantenimientoDto> CrearTareaAsync(CrearTareaMantenimientoDto dto, CancellationToken cancellationToken = default)
    {
        var panel = await _panelRepository.ObtenerPorIdAsync(dto.PanelId, cancellationToken);
        if (panel is null)
            throw new PanelNoEncontradoException(dto.PanelId);

        var quienAsigna = ObtenerUsuarioActual() ?? "Administrador";
        var tarea = new TareaMantenimiento(
            dto.PanelId,
            dto.TipoTarea,
            dto.Descripcion,
            dto.AsignadoA,
            dto.FechaVencimiento,
            quienAsigna,
            dto.Observaciones);

        await _tareaMantenimientoRepository.AgregarAsync(tarea, cancellationToken);
        await _tareaMantenimientoRepository.SaveChangesAsync(cancellationToken);

        return MapearADto(tarea);
    }

    public async Task<TareaMantenimientoDto> EditarTareaAsync(EditarTareaMantenimientoDto dto, CancellationToken cancellationToken = default)
    {
        var tarea = await _tareaMantenimientoRepository.ObtenerPorIdAsync(dto.Id, cancellationToken);
        if (tarea is null)
            throw new TareaMantenimientoNoEncontradaException(dto.Id);

        if (string.Equals(tarea.Estado, "Completada", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tarea.Estado, "Cancelada", StringComparison.OrdinalIgnoreCase))
        {
            throw new TareaMantenimientoNoEditableException(tarea.Id);
        }

        tarea.ActualizarDatos(
            dto.TipoTarea,
            dto.Descripcion,
            dto.AsignadoA,
            dto.FechaVencimiento,
            dto.Observaciones);

        await _tareaMantenimientoRepository.SaveChangesAsync(cancellationToken);
        return MapearADto(tarea);
    }

    public async Task CambiarEstadoAsync(CambiarEstadoTareaMantenimientoDto dto, CancellationToken cancellationToken = default)
    {
        var tarea = await _tareaMantenimientoRepository.ObtenerPorIdAsync(dto.Id, cancellationToken);
        if (tarea is null)
            throw new TareaMantenimientoNoEncontradaException(dto.Id);

        var usuario = ObtenerUsuarioActual();
        string? quienCompleta = dto.QuienCompleta;
        if (string.Equals(dto.NuevoEstado, "Completada", StringComparison.OrdinalIgnoreCase))
        {
            quienCompleta = string.IsNullOrWhiteSpace(quienCompleta) ? usuario : quienCompleta;
        }

        tarea.CambiarEstado(dto.NuevoEstado, quienCompleta);
        await _tareaMantenimientoRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarTareaAsync(Guid tareaId, CancellationToken cancellationToken = default)
    {
        var tarea = await _tareaMantenimientoRepository.ObtenerPorIdAsync(tareaId, cancellationToken);
        if (tarea is null)
            throw new TareaMantenimientoNoEncontradaException(tareaId);

        _tareaMantenimientoRepository.Eliminar(tarea);
        await _tareaMantenimientoRepository.SaveChangesAsync(cancellationToken);
    }

    private string? ObtenerUsuarioActual()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true) return null;
        return user.Identity?.Name ?? user.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value;
    }

    private static TareaMantenimientoDto MapearADto(TareaMantenimiento tarea)
    {
        return new TareaMantenimientoDto
        {
            Id = tarea.Id,
            PanelId = tarea.PanelId,
            TipoTarea = tarea.TipoTarea,
            Descripcion = tarea.Descripcion,
            AsignadoA = tarea.AsignadoA,
            FechaCreacion = tarea.FechaCreacion,
            FechaAsignacion = tarea.FechaAsignacion,
            FechaVencimiento = tarea.FechaVencimiento,
            FechaInicio = tarea.FechaInicio,
            FechaFinalizacion = tarea.FechaFinalizacion,
            Estado = tarea.Estado,
            QuienAsigna = tarea.QuienAsigna,
            QuienCompleta = tarea.QuienCompleta,
            Observaciones = tarea.Observaciones
        };
    }

    private static TareaMantenimientoDetalleDto MapearADetalleDto(TareaMantenimiento tarea)
    {
        return new TareaMantenimientoDetalleDto
        {
            Id = tarea.Id,
            PanelId = tarea.PanelId,
            TipoTarea = tarea.TipoTarea,
            Descripcion = tarea.Descripcion,
            AsignadoA = tarea.AsignadoA,
            FechaCreacion = tarea.FechaCreacion,
            FechaAsignacion = tarea.FechaAsignacion,
            FechaVencimiento = tarea.FechaVencimiento,
            FechaInicio = tarea.FechaInicio,
            FechaFinalizacion = tarea.FechaFinalizacion,
            Estado = tarea.Estado,
            QuienAsigna = tarea.QuienAsigna,
            QuienCompleta = tarea.QuienCompleta,
            Observaciones = tarea.Observaciones,
            PanelCodigoSerie = tarea.Panel?.CodigoSerie,
            NodoId = tarea.Panel?.NodoId,
            NodoCodigo = tarea.Panel?.Nodo?.Codigo,
            ParqueSolarId = tarea.Panel?.Nodo?.ParqueSolarId,
            ParqueSolarNombre = tarea.Panel?.Nodo?.ParqueSolar?.Nombre
        };
    }
}
