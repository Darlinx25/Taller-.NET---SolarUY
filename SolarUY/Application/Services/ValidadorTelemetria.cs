using System;
using System.Collections.Generic;
using SolarUY.Application.Dtos;
using SolarUY.Domain.Entities;

namespace SolarUY.Application.Services;

/// <summary>
/// Valida el payload de telemetría que envía el simulador. No valida las sumas de potencia ni de energía:
/// el emisor es un componente interno confiable.
/// </summary>
public class ValidadorTelemetria
{
    private const string EstadoOperativo = "Operativo";
    private const string EstadoEnFalla = "EnFalla";

    public IReadOnlyList<string> Validar(TelemetriaDto? lectura)
    {
        if (lectura is null)
            return new[] { "La lectura es obligatoria." };

        if (lectura.Parques is null)
            return new[] { "La lista de parques es obligatoria." };

        var errores = new List<string>();
        ValidarFechaHoraRaiz(lectura, errores);

        for (var i = 0; i < lectura.Parques.Count; i++)
            ValidarParque(lectura.Parques[i], $"parques[{i}]", errores);

        return errores;
    }

    private static void ValidarFechaHoraRaiz(TelemetriaDto lectura, List<string> errores)
    {
        if (lectura.Parques.Count == 0)
        {
            if (lectura.FechaHora is not null)
                errores.Add("fechaHora debe ser nula cuando no hay parques.");

            return;
        }

        if (lectura.FechaHora is null || lectura.FechaHora == default(DateTime))
            errores.Add("fechaHora es obligatoria cuando hay parques.");
    }

    private static void ValidarParque(TelemetriaParqueSolarDto? parque, string ruta, List<string> errores)
    {
        if (parque is null)
        {
            errores.Add($"{ruta} es obligatorio.");
            return;
        }

        ValidarId(parque.ParqueSolarId, $"{ruta}.parqueSolarId", errores);
        ValidarTexto(parque.NombreParque, ParqueSolar.NombreMaxLength, $"{ruta}.nombreParque", errores);
        ValidarNoNegativo(parque.PotenciaGeneradaW, $"{ruta}.potenciaGeneradaW", errores);
        ValidarNoNegativo(parque.EnergiaAcumuladaKWh, $"{ruta}.energiaAcumuladaKWh", errores);
        ValidarFechaHora(parque.FechaHora, $"{ruta}.fechaHora", errores);

        if (parque.Nodos is null)
        {
            errores.Add($"{ruta}.nodos es obligatorio.");
            return;
        }

        for (var i = 0; i < parque.Nodos.Count; i++)
            ValidarNodo(parque.Nodos[i], $"{ruta}.nodos[{i}]", errores);
    }

    private static void ValidarNodo(TelemetriaNodoDto? nodo, string ruta, List<string> errores)
    {
        if (nodo is null)
        {
            errores.Add($"{ruta} es obligatorio.");
            return;
        }

        ValidarId(nodo.NodoId, $"{ruta}.nodoId", errores);
        ValidarTexto(nodo.CodigoNodo, Nodo.CodigoMaxLength, $"{ruta}.codigoNodo", errores);
        ValidarNoNegativo(nodo.PotenciaGeneradaW, $"{ruta}.potenciaGeneradaW", errores);
        ValidarNoNegativo(nodo.EnergiaAcumuladaKWh, $"{ruta}.energiaAcumuladaKWh", errores);
        ValidarFechaHora(nodo.FechaHora, $"{ruta}.fechaHora", errores);

        if (nodo.Paneles is null)
        {
            errores.Add($"{ruta}.paneles es obligatorio.");
            return;
        }

        for (var i = 0; i < nodo.Paneles.Count; i++)
            ValidarPanel(nodo.Paneles[i], $"{ruta}.paneles[{i}]", errores);
    }

    private static void ValidarPanel(TelemetriaPanelDto? panel, string ruta, List<string> errores)
    {
        if (panel is null)
        {
            errores.Add($"{ruta} es obligatorio.");
            return;
        }

        ValidarId(panel.PanelId, $"{ruta}.panelId", errores);
        ValidarTexto(panel.CodigoSerie, Panel.CodigoSerieMaxLength, $"{ruta}.codigoSerie", errores);
        ValidarNoNegativo(panel.PotenciaGeneradaW, $"{ruta}.potenciaGeneradaW", errores);
        ValidarEstado(panel.Estado, $"{ruta}.estado", errores);
        ValidarFechaHora(panel.FechaHora, $"{ruta}.fechaHora", errores);
    }

    private static void ValidarId(Guid id, string ruta, List<string> errores)
    {
        if (id == Guid.Empty)
            errores.Add($"{ruta} es obligatorio.");
    }

    private static void ValidarTexto(string? texto, int longitudMaxima, string ruta, List<string> errores)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            errores.Add($"{ruta} es obligatorio.");
            return;
        }

        if (texto.Trim().Length > longitudMaxima)
            errores.Add($"{ruta} no puede superar los {longitudMaxima} caracteres.");
    }

    private static void ValidarNoNegativo(decimal valor, string ruta, List<string> errores)
    {
        if (valor < 0)
            errores.Add($"{ruta} no puede ser negativa.");
    }

    private static void ValidarFechaHora(DateTime fechaHora, string ruta, List<string> errores)
    {
        if (fechaHora == default)
            errores.Add($"{ruta} es obligatoria.");
    }

    private static void ValidarEstado(string? estado, string ruta, List<string> errores)
    {
        if (estado is not (EstadoOperativo or EstadoEnFalla))
            errores.Add($"{ruta} debe ser '{EstadoOperativo}' o '{EstadoEnFalla}'.");
    }
}
