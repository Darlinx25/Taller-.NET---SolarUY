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
public class PanelesController : Controller
{
    private readonly IPanelService _panelService;
    private readonly INodoService _nodoService;

    public PanelesController(IPanelService panelService, INodoService nodoService)
    {
        _panelService = panelService ?? throw new ArgumentNullException(nameof(panelService));
        _nodoService = nodoService ?? throw new ArgumentNullException(nameof(nodoService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid nodoId, CancellationToken cancellationToken)
    {
        if (!await CargarNodoAsync(nodoId, cancellationToken))
            return NotFound();

        var paneles = await _panelService.ObtenerPanelesPorNodoAsync(nodoId, cancellationToken);
        return View(paneles);
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid nodoId, CancellationToken cancellationToken)
    {
        if (!await CargarNodoAsync(nodoId, cancellationToken))
            return NotFound();

        return View(new CrearPanelDto { NodoId = nodoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearPanelDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (!await CargarNodoAsync(dto.NodoId, cancellationToken))
                return RedirectToAction("Index", "ParquesSolares");

            return View(dto);
        }

        try
        {
            await _panelService.CrearPanelAsync(dto, cancellationToken);
        }
        catch (NodoNoEncontradoException)
        {
            TempData["Error"] = "No se encontró el nodo.";
            return RedirectToAction("Index", "ParquesSolares");
        }
        catch (CodigoPanelDuplicadoException)
        {
            ModelState.AddModelError(
                nameof(dto.CodigoSerie),
                "Ya existe un panel con ese código/serie en el nodo.");

            await CargarNodoAsync(dto.NodoId, cancellationToken);
            return View(dto);
        }

        TempData["Mensaje"] = $"El panel '{dto.CodigoSerie}' se registró correctamente.";
        return RedirectToAction(nameof(Index), new { nodoId = dto.NodoId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var panel = await _panelService.ObtenerPanelAsync(id, cancellationToken);
        return panel is null ? NotFound() : View(panel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var panel = await _panelService.ObtenerPanelAsync(id, cancellationToken);
        if (panel is null)
            return NotFound();

        ViewBag.Panel = panel;

        return View(new EditarPanelDto
        {
            Id = panel.Id,
            NodoId = panel.NodoId,
            CodigoSerie = panel.CodigoSerie,
            Marca = panel.Marca,
            Modelo = panel.Modelo,
            PotenciaWp = panel.PotenciaWp,
            FechaInstalacion = panel.FechaInstalacion,
            Observaciones = panel.Observaciones,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditarPanelDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (!await CargarPanelAsync(dto.Id, cancellationToken))
                return RedirectToAction("Index", "ParquesSolares");

            return View(dto);
        }

        try
        {
            await _panelService.EditarPanelAsync(dto, cancellationToken);
        }
        catch (PanelNoEncontradoException)
        {
            TempData["Error"] = "No se encontró el panel que se quería editar.";
            return RedirectToAction("Index", "ParquesSolares");
        }
        catch (CodigoPanelDuplicadoException)
        {
            ModelState.AddModelError(
                nameof(dto.CodigoSerie),
                "Ya existe un panel con ese código/serie en el nodo.");

            await CargarPanelAsync(dto.Id, cancellationToken);
            return View(dto);
        }

        TempData["Mensaje"] = $"El panel '{dto.CodigoSerie}' se actualizó correctamente.";
        return RedirectToAction(nameof(Index), new { nodoId = dto.NodoId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var panel = await _panelService.ObtenerPanelAsync(id, cancellationToken);
        return panel is null ? NotFound() : View(panel);
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var panel = await _panelService.ObtenerPanelAsync(id, cancellationToken);
        if (panel is null)
        {
            TempData["Error"] = "No se encontró el panel que se quería eliminar.";
            return RedirectToAction("Index", "ParquesSolares");
        }

        try
        {
            await _panelService.EliminarPanelAsync(id, cancellationToken);
        }
        catch (PanelNoEncontradoException)
        {
            TempData["Error"] = "No se encontró el panel que se quería eliminar.";
            return RedirectToAction(nameof(Index), new { nodoId = panel.NodoId });
        }

        TempData["Mensaje"] = "El panel se eliminó correctamente.";
        return RedirectToAction(nameof(Index), new { nodoId = panel.NodoId });
    }

    private async Task<bool> CargarNodoAsync(Guid nodoId, CancellationToken cancellationToken)
    {
        var nodo = await _nodoService.ObtenerNodoAsync(nodoId, cancellationToken);
        ViewBag.Nodo = nodo;
        return nodo is not null;
    }

    private async Task<bool> CargarPanelAsync(Guid panelId, CancellationToken cancellationToken)
    {
        var panel = await _panelService.ObtenerPanelAsync(panelId, cancellationToken);
        ViewBag.Panel = panel;
        return panel is not null;
    }
}
