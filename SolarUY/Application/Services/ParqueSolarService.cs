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

/// <summary>
/// Casos de uso de alta y consulta de parques solares.
/// </summary>
public class ParqueSolarService : IParqueSolarService
{
    private readonly IParqueSolarRepository _parqueSolarRepository;

    /// <summary>
    /// Crea el servicio de parques solares.
    /// </summary>
    /// <param name="parqueSolarRepository">Repositorio de persistencia de parques solares.</param>
    public ParqueSolarService(IParqueSolarRepository parqueSolarRepository)
    {
        _parqueSolarRepository = parqueSolarRepository
            ?? throw new ArgumentNullException(nameof(parqueSolarRepository));
    }

    /// <inheritdoc />
    public async Task<Guid> CrearParqueSolarAsync(
        CrearParqueSolarDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var nombre = dto.Nombre.Trim();
        var existe = await _parqueSolarRepository
            .ExisteNombreAsync(nombre, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (existe)
        {
            throw new NombreParqueDuplicadoException(nombre);
        }

        var parque = new ParqueSolar(dto.Nombre, dto.Ubicacion, dto.CapacidadEstimadaMW);

        await _parqueSolarRepository.AgregarAsync(parque, cancellationToken).ConfigureAwait(false);
        await _parqueSolarRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return parque.Id;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ParqueSolarDto>> ObtenerParquesAsync(
        CancellationToken cancellationToken = default)
    {
        var parques = await _parqueSolarRepository
            .ObtenerTodosAsync(cancellationToken)
            .ConfigureAwait(false);

        return parques
            .Select(Mapear)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<ParqueSolarDto?> ObtenerParqueAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var parque = await _parqueSolarRepository
            .ObtenerPorIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return parque is null ? null : Mapear(parque);
    }

    /// <inheritdoc />
    public async Task EditarParqueSolarAsync(
        EditarParqueSolarDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var parque = await _parqueSolarRepository
            .ObtenerPorIdAsync(dto.Id, cancellationToken)
            .ConfigureAwait(false);

        if (parque is null)
        {
            throw new ParqueSolarNoEncontradoException(dto.Id);
        }

        var nombre = dto.Nombre.Trim();
        var existe = await _parqueSolarRepository
            .ExisteNombreAsync(nombre, dto.Id, cancellationToken)
            .ConfigureAwait(false);

        if (existe)
        {
            throw new NombreParqueDuplicadoException(nombre);
        }

        parque.ActualizarDatos(dto.Nombre, dto.Ubicacion, dto.CapacidadEstimadaMW);

        await _parqueSolarRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task EliminarParqueSolarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var parque = await _parqueSolarRepository
            .ObtenerPorIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (parque is null)
        {
            throw new ParqueSolarNoEncontradoException(id);
        }

        _parqueSolarRepository.Eliminar(parque);

        await _parqueSolarRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static ParqueSolarDto Mapear(ParqueSolar parque)
    {
        return new ParqueSolarDto
        {
            Id = parque.Id,
            Nombre = parque.Nombre,
            Ubicacion = parque.Ubicacion,
            CapacidadEstimadaMW = parque.CapacidadEstimadaMW,
            FechaCreacion = parque.FechaCreacion,
        };
    }
}
