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
public class NodosController : Controller
{
    private readonly INodoService _nodoService;
    private readonly IParqueSolarService _parqueSolarService;

    public NodosController(INodoService nodoService, IParqueSolarService parqueSolarService)
    {
        _nodoService = nodoService ?? throw new ArgumentNullException(nameof(nodoService));
        _parqueSolarService = parqueSolarService ?? throw new ArgumentNullException(nameof(parqueSolarService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid parqueId, CancellationToken cancellationToken)
    {
        var parque = await _parqueSolarService.ObtenerParqueAsync(parqueId, cancellationToken);
        if (parque is null)
            return NotFound();

        ViewBag.Parque = parque;
        var nodos = await _nodoService.ObtenerNodosPorParqueAsync(parqueId, cancellationToken);
        return View(nodos);
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid parqueId, CancellationToken cancellationToken)
    {
        var parque = await _parqueSolarService.ObtenerParqueAsync(parqueId, cancellationToken);
        if (parque is null)
            return NotFound();

        ViewBag.Parque = parque;
        return View(new CrearNodoInversorDto { ParqueSolarId = parqueId, Estado = "Activo" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearNodoInversorDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var parque = await _parqueSolarService.ObtenerParqueAsync(dto.ParqueSolarId, cancellationToken);
            ViewBag.Parque = parque;
            return View(dto);
        }

        try
        {
            await _nodoService.CrearNodoInversorAsync(dto, cancellationToken);
        }
        catch (ParqueSolarNoEncontradoException)
        {
            TempData["Error"] = "No se encontró el parque solar.";
            return RedirectToAction("Index", "ParquesSolares");
        }
        catch (CodigoNodoDuplicadoException)
        {
            ModelState.AddModelError(nameof(dto.Codigo), "Ya existe un nodo con ese código en el parque solar.");
            var parque = await _parqueSolarService.ObtenerParqueAsync(dto.ParqueSolarId, cancellationToken);
            ViewBag.Parque = parque;
            return View(dto);
        }

        TempData["Mensaje"] = $"El nodo '{dto.Codigo}' se registró correctamente.";
        return RedirectToAction(nameof(Index), new { parqueId = dto.ParqueSolarId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var nodo = await _nodoService.ObtenerNodoAsync(id, cancellationToken);
        return nodo is null ? NotFound() : View(nodo);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var nodo = await _nodoService.ObtenerNodoAsync(id, cancellationToken);
        if (nodo is null)
            return NotFound();

        var dto = new EditarNodoInversorDto
        {
            Id = nodo.Id,
            ParqueSolarId = nodo.ParqueSolarId,
            Codigo = nodo.Codigo,
            Descripcion = nodo.Descripcion,
            Estado = nodo.Estado,
            InversorId = nodo.Inversor?.Id ?? Guid.Empty,
            InversorMarca = nodo.Inversor?.Marca ?? string.Empty,
            InversorModelo = nodo.Inversor?.Modelo ?? string.Empty,
            InversorNumeroSerie = nodo.Inversor?.NumeroSerie ?? string.Empty,
            InversorPotenciaNominalkW = nodo.Inversor?.PotenciaNominalkW
        };

        ViewBag.ParqueSolarNombre = nodo.ParqueSolarNombre;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditarNodoInversorDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var nodo = await _nodoService.ObtenerNodoAsync(dto.Id, cancellationToken);
            ViewBag.ParqueSolarNombre = nodo?.ParqueSolarNombre;
            return View(dto);
        }

        try
        {
            await _nodoService.EditarNodoInversorAsync(dto, cancellationToken);
        }
        catch (NodoNoEncontradoException)
        {
            TempData["Error"] = "No se encontró el nodo que se quería editar.";
            return RedirectToAction("Index", "ParquesSolares");
        }
        catch (CodigoNodoDuplicadoException)
        {
            ModelState.AddModelError(nameof(dto.Codigo), "Ya existe un nodo con ese código en el parque solar.");
            var nodo = await _nodoService.ObtenerNodoAsync(dto.Id, cancellationToken);
            ViewBag.ParqueSolarNombre = nodo?.ParqueSolarNombre;
            return View(dto);
        }

        TempData["Mensaje"] = $"El nodo '{dto.Codigo}' se actualizó correctamente.";
        return RedirectToAction(nameof(Index), new { parqueId = dto.ParqueSolarId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var nodo = await _nodoService.ObtenerNodoAsync(id, cancellationToken);
        return nodo is null ? NotFound() : View(nodo);
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var nodo = await _nodoService.ObtenerNodoAsync(id, cancellationToken);
        if (nodo is null)
        {
            TempData["Error"] = "No se encontró el nodo que se quería eliminar.";
            return RedirectToAction("Index", "ParquesSolares");
        }

        try
        {
            await _nodoService.EliminarNodoAsync(id, cancellationToken);
        }
        catch (NodoNoEncontradoException)
        {
            TempData["Error"] = "No se encontró el nodo que se quería eliminar.";
            return RedirectToAction("Index", "ParquesSolares");
        }

        TempData["Mensaje"] = "El nodo se eliminó correctamente.";
        return RedirectToAction(nameof(Index), new { parqueId = nodo.ParqueSolarId });
    }
}
