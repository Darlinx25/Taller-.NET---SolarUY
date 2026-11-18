using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using SolarUY.Domain.Enums;

namespace SolarUY.Models;

public static class EstadoNodoOpciones
{
    public static IReadOnlyList<SelectListItem> Obtener()
    {
        return new List<SelectListItem>
        {
            new(Texto(EstadoNodo.Activo), nameof(EstadoNodo.Activo)),
            new(Texto(EstadoNodo.Inactivo), nameof(EstadoNodo.Inactivo)),
            new(Texto(EstadoNodo.EnMantenimiento), nameof(EstadoNodo.EnMantenimiento))
        };
    }

    public static string Texto(EstadoNodo estado)
    {
        return estado switch
        {
            EstadoNodo.Activo => "Activo",
            EstadoNodo.Inactivo => "Inactivo",
            EstadoNodo.EnMantenimiento => "En mantenimiento",
            _ => estado.ToString()
        };
    }
}
