using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarUY.Application.Dtos;
using SolarUY.Application.Services;
using SolarUY.Domain.Exceptions;

namespace SolarUY.Controllers;

/// <summary>
/// Casos de uso de la presentación para la gestión de parques solares.
/// Solo invoca servicios de la capa Application: no contiene reglas de negocio.
/// </summary>
[Authorize(Roles = "Administrador")]
public class ParquesSolaresController : Controller
{
    private readonly IParqueSolarService _parqueSolarService;

    /// <summary>
    /// Crea el controlador de parques solares.
    /// </summary>
    /// <param name="parqueSolarService">Servicio de casos de uso de parques solares.</param>
    public ParquesSolaresController(IParqueSolarService parqueSolarService)
    {
        _parqueSolarService = parqueSolarService
            ?? throw new ArgumentNullException(nameof(parqueSolarService));
    }

    /// <summary>
    /// Muestra el listado de parques solares registrados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Vista con el listado de parques solares.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var parques = await _parqueSolarService.ObtenerParquesAsync(cancellationToken);
        return View(parques);
    }

    /// <summary>
    /// Muestra el formulario de alta de parque solar.
    /// </summary>
    /// <returns>Vista con el formulario de alta.</returns>
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CrearParqueSolarDto());
    }

    /// <summary>
    /// Registra un nuevo parque solar.
    /// </summary>
    /// <param name="dto">Datos del parque solar a registrar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Redirección al listado o el formulario con los errores de validación.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CrearParqueSolarDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _parqueSolarService.CrearParqueSolarAsync(dto, cancellationToken);
        }
        catch (NombreParqueDuplicadoException)
        {
            ModelState.AddModelError(nameof(dto.Nombre), "Ya existe un parque solar con ese nombre.");
            return View(dto);
        }

        TempData["Mensaje"] = $"El parque solar '{dto.Nombre}' se registró correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Muestra el detalle de un parque solar.
    /// </summary>
    /// <param name="id">Identificador del parque solar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Vista con el detalle del parque solar.</returns>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var parque = await _parqueSolarService.ObtenerParqueAsync(id, cancellationToken);

        return parque is null ? NotFound() : View(parque);
    }

    /// <summary>
    /// Muestra el formulario de edición de un parque solar.
    /// </summary>
    /// <param name="id">Identificador del parque solar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Vista con el formulario de edición.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var parque = await _parqueSolarService.ObtenerParqueAsync(id, cancellationToken);

        if (parque is null)
        {
            return NotFound();
        }

        var dto = new EditarParqueSolarDto
        {
            Id = parque.Id,
            Nombre = parque.Nombre,
            Ubicacion = parque.Ubicacion,
            CapacidadEstimadaMW = parque.CapacidadEstimadaMW,
        };

        return View(dto);
    }

    /// <summary>
    /// Actualiza los datos de un parque solar existente.
    /// </summary>
    /// <param name="dto">Datos del parque solar a actualizar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Redirección al listado o el formulario con los errores de validación.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditarParqueSolarDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _parqueSolarService.EditarParqueSolarAsync(dto, cancellationToken);
        }
        catch (NombreParqueDuplicadoException)
        {
            ModelState.AddModelError(nameof(dto.Nombre), "Ya existe un parque solar con ese nombre.");
            return View(dto);
        }
        catch (ParqueSolarNoEncontradoException)
        {
            TempData["Error"] = "No se encontró el parque solar que se quería editar.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Mensaje"] = $"El parque solar '{dto.Nombre}' se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Muestra la confirmación de baja de un parque solar.
    /// </summary>
    /// <param name="id">Identificador del parque solar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Vista de confirmación de baja.</returns>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var parque = await _parqueSolarService.ObtenerParqueAsync(id, cancellationToken);

        return parque is null ? NotFound() : View(parque);
    }

    /// <summary>
    /// Elimina un parque solar existente.
    /// </summary>
    /// <param name="id">Identificador del parque solar a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Redirección al listado de parques solares.</returns>
    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _parqueSolarService.EliminarParqueSolarAsync(id, cancellationToken);
        }
        catch (ParqueSolarNoEncontradoException)
        {
            TempData["Error"] = "No se encontró el parque solar que se quería eliminar.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Mensaje"] = "El parque solar se eliminó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
