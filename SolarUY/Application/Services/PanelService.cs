using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SolarUY.Application.Dtos;
using SolarUY.Domain.Entities;
using SolarUY.Domain.Exceptions;
using SolarUY.Domain.Repositories;

namespace SolarUY.Application.Services;

public class PanelService : IPanelService
{
    private readonly IPanelRepository _panelRepository;
    private readonly INodoRepository _nodoRepository;

    public PanelService(IPanelRepository panelRepository, INodoRepository nodoRepository)
    {
        _panelRepository = panelRepository ?? throw new ArgumentNullException(nameof(panelRepository));
        _nodoRepository = nodoRepository ?? throw new ArgumentNullException(nameof(nodoRepository));
    }

    public async Task<IReadOnlyList<PanelDto>> ObtenerPanelesPorNodoAsync(
        Guid nodoId,
        CancellationToken cancellationToken = default)
    {
        var paneles = await _panelRepository
            .ObtenerPorNodoAsync(nodoId, cancellationToken)
            .ConfigureAwait(false);

        return paneles.Select(MapearPanel).ToList();
    }

    public async Task<PanelDetalleDto?> ObtenerPanelAsync(
        Guid panelId,
        CancellationToken cancellationToken = default)
    {
        var panel = await _panelRepository
            .ObtenerPorIdAsync(panelId, cancellationToken)
            .ConfigureAwait(false);

        return panel is null ? null : MapearDetalle(panel);
    }

    public async Task<Guid> CrearPanelAsync(
        CrearPanelDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var nodo = await _nodoRepository
            .ObtenerPorIdAsync(dto.NodoId, cancellationToken)
            .ConfigureAwait(false);

        if (nodo is null)
            throw new NodoNoEncontradoException();

        var existeCodigo = await _panelRepository
            .ExisteCodigoAsync(dto.CodigoSerie.Trim(), dto.NodoId, null, cancellationToken)
            .ConfigureAwait(false);

        if (existeCodigo)
            throw new CodigoPanelDuplicadoException();

        var panel = new Panel(
            dto.NodoId,
            dto.CodigoSerie,
            dto.Marca,
            dto.Modelo,
            dto.PotenciaWp,
            dto.FechaInstalacion,
            dto.Observaciones);

        await _panelRepository.AgregarAsync(panel, cancellationToken).ConfigureAwait(false);
        await _panelRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return panel.Id;
    }

    public async Task EditarPanelAsync(
        EditarPanelDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var panel = await _panelRepository
            .ObtenerPorIdAsync(dto.Id, cancellationToken)
            .ConfigureAwait(false);

        if (panel is null)
            throw new PanelNoEncontradoException(dto.Id);

        var existeCodigo = await _panelRepository
            .ExisteCodigoAsync(dto.CodigoSerie.Trim(), panel.NodoId, dto.Id, cancellationToken)
            .ConfigureAwait(false);

        if (existeCodigo)
            throw new CodigoPanelDuplicadoException();

        panel.ActualizarDatos(
            dto.CodigoSerie,
            dto.Marca,
            dto.Modelo,
            dto.PotenciaWp,
            dto.FechaInstalacion,
            dto.Observaciones);

        await _panelRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task EliminarPanelAsync(
        Guid panelId,
        CancellationToken cancellationToken = default)
    {
        var panel = await _panelRepository
            .ObtenerPorIdAsync(panelId, cancellationToken)
            .ConfigureAwait(false);

        if (panel is null)
            throw new PanelNoEncontradoException(panelId);

        _panelRepository.Eliminar(panel);
        await _panelRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static PanelDto MapearPanel(Panel panel)
    {
        return new PanelDto
        {
            Id = panel.Id,
            NodoId = panel.NodoId,
            CodigoSerie = panel.CodigoSerie,
            Marca = panel.Marca,
            Modelo = panel.Modelo,
            PotenciaWp = panel.PotenciaWp,
            FechaInstalacion = panel.FechaInstalacion,
            FechaCreacion = panel.FechaCreacion,
        };
    }

    private static PanelDetalleDto MapearDetalle(Panel panel)
    {
        return new PanelDetalleDto
        {
            Id = panel.Id,
            NodoId = panel.NodoId,
            NodoCodigo = panel.Nodo?.Codigo ?? string.Empty,
            ParqueSolarId = panel.Nodo?.ParqueSolarId ?? Guid.Empty,
            ParqueSolarNombre = panel.Nodo?.ParqueSolar?.Nombre,
            CodigoSerie = panel.CodigoSerie,
            Marca = panel.Marca,
            Modelo = panel.Modelo,
            PotenciaWp = panel.PotenciaWp,
            FechaInstalacion = panel.FechaInstalacion,
            Observaciones = panel.Observaciones,
            FechaCreacion = panel.FechaCreacion,
        };
    }
}
