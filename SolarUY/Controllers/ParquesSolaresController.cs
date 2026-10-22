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
}
