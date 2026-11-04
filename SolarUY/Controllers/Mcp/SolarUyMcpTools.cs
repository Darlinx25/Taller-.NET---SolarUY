using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using SolarUY.Application.Dtos;
using SolarUY.Application.Services;
using SolarUY.Domain.Enums;
using SolarUY.Domain.Exceptions;

namespace SolarUY.Controllers.Mcp;

[McpServerToolType]
public sealed class SolarUyMcpTools
{
    private readonly IParqueSolarService _parqueSolarService;
    private readonly ITopologiaSimulacionService _topologiaSimulacionService;
    private readonly IDashboardService _dashboardService;
    private readonly IHistoricoService _historicoService;
    private readonly IAlertasService _alertasService;
    private readonly ITareaMantenimientoService _tareaMantenimientoService;

    public SolarUyMcpTools(
        IParqueSolarService parqueSolarService,
        ITopologiaSimulacionService topologiaSimulacionService,
        IDashboardService dashboardService,
        IHistoricoService historicoService,
        IAlertasService alertasService,
        ITareaMantenimientoService tareaMantenimientoService)
    {
        _parqueSolarService = parqueSolarService ?? throw new ArgumentNullException(nameof(parqueSolarService));
        _topologiaSimulacionService = topologiaSimulacionService ?? throw new ArgumentNullException(nameof(topologiaSimulacionService));
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _historicoService = historicoService ?? throw new ArgumentNullException(nameof(historicoService));
        _alertasService = alertasService ?? throw new ArgumentNullException(nameof(alertasService));
        _tareaMantenimientoService = tareaMantenimientoService ?? throw new ArgumentNullException(nameof(tareaMantenimientoService));
    }

    [McpServerTool(Name = "listar_parques", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description("Lista los parques solares registrados con su identificador, nombre, ubicación, capacidad estimada en MW y fecha de alta en UTC.")]
    public Task<IReadOnlyList<ParqueSolarDto>> ListarParques(CancellationToken cancellationToken = default)
    {
        return _parqueSolarService.ObtenerParquesAsync(cancellationToken);
    }

    [McpServerTool(Name = "obtener_topologia", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description("Devuelve la topología registrada: parques, nodos, inversores y paneles. No incluye telemetría.")]
    public Task<TopologiaSimulacionDto> ObtenerTopologia(CancellationToken cancellationToken = default)
    {
        return _topologiaSimulacionService.ObtenerAsync(cancellationToken);
    }

    [McpServerTool(Name = "obtener_estado_actual", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description("Devuelve el estado actual del sistema a partir de la última lectura de telemetría: potencia, energía y estado por parque, nodo y panel. Si no hay telemetría, lo indica en MensajeSinDatos.")]
    public Task<DashboardTelemetriaViewDto> ObtenerEstadoActual(CancellationToken cancellationToken = default)
    {
        return _dashboardService.ObtenerDashboardAsync(cancellationToken);
    }

    [McpServerTool(Name = "obtener_historico", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description("Consulta los resúmenes históricos de generación. tipoEntidad: Parque, Nodo o Panel. desde y hasta: fechas en formato yyyy-MM-dd. agrupacion: Dia, Mes o Anio. Las fechas se interpretan en la zona local UTC-3.")]
    public async Task<HistoricoGeneracionDto> ObtenerHistorico(
        [Description("Tipo de entidad: Parque, Nodo o Panel.")] string tipoEntidad,
        [Description("Fecha inicial en formato yyyy-MM-dd.")] string desde,
        [Description("Fecha final en formato yyyy-MM-dd.")] string hasta,
        [Description("Agrupación: Dia, Mes o Anio.")] string agrupacion,
        [Description("Identificador de la entidad. Si se omite, se consultan todas las entidades del tipo indicado.")] Guid? entidadId = null,
        CancellationToken cancellationToken = default)
    {
        var filtro = new FiltroHistoricoDto
        {
            TipoEntidad = ParsearTipoEntidad(tipoEntidad),
            EntidadId = entidadId,
            Desde = ParsearFecha(desde, "desde"),
            Hasta = ParsearFecha(hasta, "hasta"),
            Agrupacion = ParsearAgrupacion(agrupacion)
        };

        try
        {
            return await _historicoService.ConsultarAsync(filtro, cancellationToken).ConfigureAwait(false);
        }
        catch (FiltroHistoricoInvalidoException ex)
        {
            throw new McpException(ex.Message, ex);
        }
    }

    [McpServerTool(Name = "obtener_alertas", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description("Devuelve las alertas activas y las 50 cerradas más recientes, con parque, nodo, panel, fechas en UTC y duración.")]
    public Task<ListadoAlertasDto> ObtenerAlertas(CancellationToken cancellationToken = default)
    {
        return _alertasService.ObtenerListadoAsync(cancellationToken);
    }

    [McpServerTool(Name = "listar_tareas_mantenimiento", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description("Lista las tareas de mantenimiento. Opcionalmente filtra por estado (Pendiente, EnProceso, Completada o Cancelada) y por panelId.")]
    public Task<IReadOnlyList<TareaMantenimientoDto>> ListarTareasMantenimiento(
        [Description("Estado a filtrar: Pendiente, EnProceso, Completada o Cancelada.")] string? estado = null,
        [Description("Identificador del panel para filtrar sus tareas.")] Guid? panelId = null,
        CancellationToken cancellationToken = default)
    {
        return _tareaMantenimientoService.ObtenerTareasAsync(estado, panelId, cancellationToken);
    }

    private static TipoEntidadHistorico ParsearTipoEntidad(string tipoEntidad)
    {
        if (Enum.TryParse(tipoEntidad, ignoreCase: true, out TipoEntidadHistorico tipo) && Enum.IsDefined(tipo))
        {
            return tipo;
        }

        throw new McpException("El tipo de entidad debe ser Parque, Nodo o Panel.");
    }

    private static AgrupacionHistorico ParsearAgrupacion(string agrupacion)
    {
        if (Enum.TryParse(agrupacion, ignoreCase: true, out AgrupacionHistorico valor) && Enum.IsDefined(valor))
        {
            return valor;
        }

        throw new McpException("La agrupación debe ser Dia, Mes o Anio.");
    }

    private static DateOnly ParsearFecha(string fecha, string nombre)
    {
        if (DateOnly.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly valor))
        {
            return valor;
        }

        throw new McpException($"La fecha '{nombre}' debe tener el formato yyyy-MM-dd.");
    }
}
