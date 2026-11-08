using SolarUY.Simulador.Contracts;
using SolarUY.Simulador.Domain;

namespace SolarUY.Simulador.Transport;

/// <summary>
/// Convierte las lecturas del Dominio del simulador en el contrato JSON que consume la aplicación.
/// </summary>
public static class TelemetriaMapper
{
    private const int DecimalesEnergia = 4;

    public static TelemetriaDto Mapear(IReadOnlyList<LecturaParqueSolarSimulado> lecturas)
    {
        ArgumentNullException.ThrowIfNull(lecturas);

        if (lecturas.Count == 0)
        {
            return new TelemetriaDto
            {
                FechaHora = null,
                Parques = Array.Empty<TelemetriaParqueSolarDto>()
            };
        }

        return new TelemetriaDto
        {
            FechaHora = lecturas[0].FechaHora,
            Parques = lecturas.Select(MapearParque).ToList()
        };
    }

    private static TelemetriaParqueSolarDto MapearParque(LecturaParqueSolarSimulado parque)
    {
        return new TelemetriaParqueSolarDto
        {
            ParqueSolarId = parque.ParqueSolarId,
            NombreParque = parque.NombreParque,
            PotenciaGeneradaW = parque.PotenciaGeneradaW,
            EnergiaAcumuladaKWh = RedondearEnergia(parque.EnergiaAcumuladaKWh),
            FechaHora = parque.FechaHora,
            Nodos = parque.Nodos.Select(MapearNodo).ToList()
        };
    }

    private static TelemetriaNodoDto MapearNodo(LecturaNodoSimulado nodo)
    {
        return new TelemetriaNodoDto
        {
            NodoId = nodo.NodoId,
            CodigoNodo = nodo.CodigoNodo,
            PotenciaGeneradaW = nodo.PotenciaGeneradaW,
            EnergiaAcumuladaKWh = RedondearEnergia(nodo.EnergiaAcumuladaKWh),
            FechaHora = nodo.FechaHora,
            Paneles = nodo.Paneles.Select(MapearPanel).ToList()
        };
    }

    private static TelemetriaPanelDto MapearPanel(LecturaPanelSimulado panel)
    {
        return new TelemetriaPanelDto
        {
            PanelId = panel.PanelId,
            CodigoSerie = panel.CodigoSerie,
            PotenciaGeneradaW = panel.PotenciaGeneradaW,
            Estado = panel.Estado.ToString(),
            FechaHora = panel.FechaHora
        };
    }

    private static decimal RedondearEnergia(decimal energiaKWh)
    {
        return Math.Round(energiaKWh, DecimalesEnergia, MidpointRounding.AwayFromZero);
    }
}
