using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolarUY.Application.Services;

namespace SolarUY.Controllers;

/// <summary>
/// Consulta de solo lectura de las alertas de paneles. Acceso sin sesión (rol: Todos), como el Dashboard.
/// </summary>
public class AlertasController : Controller
{
    private readonly IAlertasService _alertasService;

    public AlertasController(IAlertasService alertasService)
    {
        _alertasService = alertasService ?? throw new ArgumentNullException(nameof(alertasService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var listado = await _alertasService.ObtenerListadoAsync(cancellationToken).ConfigureAwait(false);
        return View(listado);
    }

    [HttpGet]
    public async Task<IActionResult> CantidadActivas(CancellationToken cancellationToken)
    {
        var cantidadActivas = await _alertasService.ContarActivasAsync(cancellationToken).ConfigureAwait(false);
        return Json(new { cantidadActivas });
    }
}
