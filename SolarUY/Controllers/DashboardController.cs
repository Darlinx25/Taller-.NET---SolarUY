using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolarUY.Application.Dtos;
using SolarUY.Application.Services;

namespace SolarUY.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var dashboard = await _dashboardService
            .ObtenerDashboardAsync(cancellationToken)
            .ConfigureAwait(false);
        var tendencia = await _dashboardService
            .ObtenerTendenciaEnergiaAsync(cancellationToken)
            .ConfigureAwait(false);

        return View(new DashboardPaginaDto
        {
            Dashboard = dashboard,
            Tendencia = tendencia
        });
    }

    [HttpGet]
    public async Task<IActionResult> Contenido(CancellationToken cancellationToken)
    {
        var modelo = await _dashboardService.ObtenerDashboardAsync(cancellationToken).ConfigureAwait(false);
        return PartialView("_DashboardContenido", modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Tendencia(CancellationToken cancellationToken)
    {
        var modelo = await _dashboardService
            .ObtenerTendenciaEnergiaAsync(cancellationToken)
            .ConfigureAwait(false);

        return Json(modelo);
    }
}
