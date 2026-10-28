using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarUY.Application.Dtos;
using SolarUY.Application.Services;
using SolarUY.Domain.Exceptions;

namespace SolarUY.Controllers;

[Authorize(Roles = "Administrador")]
public class TareasMantenimientoController : Controller
{
    private readonly ITareaMantenimientoService _tareaMantenimientoService;
    private readonly IPanelService _panelService;

    public TareasMantenimientoController(ITareaMantenimientoService tareaMantenimientoService, IPanelService panelService)
    {
        _tareaMantenimientoService = tareaMantenimientoService;
        _panelService = panelService;
    }

    public async Task<IActionResult> Index(Guid panelId, CancellationToken cancellationToken)
    {
        try
        {
            var panel = await _panelService.ObtenerPanelAsync(panelId, cancellationToken);
            ViewBag.Panel = panel;
            var tareas = await _tareaMantenimientoService.ObtenerTareasPorPanelAsync(panelId, cancellationToken);
            return View(tareas);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    public async Task<IActionResult> Create(Guid panelId, CancellationToken cancellationToken)
    {
        try
        {
            var panel = await _panelService.ObtenerPanelAsync(panelId, cancellationToken);
            ViewBag.Panel = panel;
            var dto = new CrearTareaMantenimientoDto { PanelId = panelId, FechaVencimiento = DateTime.Today };
            return View(dto);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearTareaMantenimientoDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            try
            {
                var panel = await _panelService.ObtenerPanelAsync(dto.PanelId, cancellationToken);
                ViewBag.Panel = panel;
            }
            catch { }
            return View(dto);
        }

        try
        {
            await _tareaMantenimientoService.CrearTareaAsync(dto, cancellationToken);
            TempData["Mensaje"] = "Tarea de mantenimiento creada correctamente.";
            return RedirectToAction(nameof(Index), new { panelId = dto.PanelId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            try
            {
                var panel = await _panelService.ObtenerPanelAsync(dto.PanelId, cancellationToken);
                ViewBag.Panel = panel;
            }
            catch { }
            return View(dto);
        }
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var tarea = await _tareaMantenimientoService.ObtenerTareaAsync(id, cancellationToken);
            return View(tarea);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var tarea = await _tareaMantenimientoService.ObtenerTareaAsync(id, cancellationToken);
            if (string.Equals(tarea.Estado, "Completada", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tarea.Estado, "Cancelada", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "No se puede editar una tarea en estado Completada o Cancelada.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var dto = new EditarTareaMantenimientoDto
            {
                Id = tarea.Id,
                TipoTarea = tarea.TipoTarea,
                Descripcion = tarea.Descripcion,
                AsignadoA = tarea.AsignadoA,
                FechaVencimiento = tarea.FechaVencimiento,
                Observaciones = tarea.Observaciones
            };
            ViewBag.Tarea = tarea;
            return View(dto);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditarTareaMantenimientoDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            try { ViewBag.Tarea = await _tareaMantenimientoService.ObtenerTareaAsync(dto.Id, cancellationToken); } catch { }
            return View(dto);
        }

        try
        {
            await _tareaMantenimientoService.EditarTareaAsync(dto, cancellationToken);
            TempData["Mensaje"] = "Tarea de mantenimiento actualizada correctamente.";
            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            try { ViewBag.Tarea = await _tareaMantenimientoService.ObtenerTareaAsync(dto.Id, cancellationToken); } catch { }
            return View(dto);
        }
    }

    public async Task<IActionResult> CambiarEstado(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var tarea = await _tareaMantenimientoService.ObtenerTareaAsync(id, cancellationToken);
            if (string.Equals(tarea.Estado, "Completada", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tarea.Estado, "Cancelada", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "No se puede cambiar el estado de una tarea en estado Completada o Cancelada.";
                return RedirectToAction(nameof(Details), new { id });
            }
            ViewBag.Tarea = tarea;
            var dto = new CambiarEstadoTareaMantenimientoDto { Id = id };
            return View(dto);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(CambiarEstadoTareaMantenimientoDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            try { ViewBag.Tarea = await _tareaMantenimientoService.ObtenerTareaAsync(dto.Id, cancellationToken); } catch { }
            return View(dto);
        }

        try
        {
            await _tareaMantenimientoService.CambiarEstadoAsync(dto, cancellationToken);
            TempData["Mensaje"] = "Estado de la tarea actualizado correctamente.";
            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            try { ViewBag.Tarea = await _tareaMantenimientoService.ObtenerTareaAsync(dto.Id, cancellationToken); } catch { }
            return View(dto);
        }
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var tarea = await _tareaMantenimientoService.ObtenerTareaAsync(id, cancellationToken);
            return View(tarea);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var tarea = await _tareaMantenimientoService.ObtenerTareaAsync(id, cancellationToken);
            var panelId = tarea.PanelId;
            await _tareaMantenimientoService.EliminarTareaAsync(id, cancellationToken);
            TempData["Mensaje"] = "Tarea de mantenimiento eliminada correctamente.";
            return RedirectToAction(nameof(Index), new { panelId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Delete), new { id });
        }
    }
}
