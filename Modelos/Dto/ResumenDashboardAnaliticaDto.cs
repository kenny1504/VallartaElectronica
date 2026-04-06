namespace ElectronicaVallarta.Modelos.Dto;

public class ResumenDashboardAnaliticaDto
{
    public int TotalConsultas { get; set; }
    public int TotalConsultasHoy { get; set; }
    public int TotalConsultasUltimosSieteDias { get; set; }
    public int TotalSesionesUnicas { get; set; }
    public int TotalConsultasExitosas { get; set; }
    public int TotalConsultasFallidas { get; set; }
    public int TotalConsultasMontoMayorMil { get; set; }
    public decimal PromedioTiempoRespuestaMs { get; set; }
    public decimal PorcentajeExito { get; set; }
    public decimal PromedioMontoConsultadoUsd { get; set; }
    public decimal PromedioResultadoObtenido { get; set; }
    public decimal PromedioTasaAplicada { get; set; }
    public string PaisMasConsultado { get; set; } = "Sin datos";
    public string RegionMasConsultada { get; set; } = "Sin datos";
    public string SucursalMasConsultada { get; set; } = "Sin datos";
    public string RangoMasConsultado { get; set; } = "Sin datos";
    public IReadOnlyCollection<SerieDiariaConsultaAnaliticaDto> SeriePorDia { get; set; } = [];
    public IReadOnlyCollection<ElementoMetricaAnaliticaDto> TopPaises { get; set; } = [];
    public IReadOnlyCollection<ElementoMetricaAnaliticaDto> TopRegiones { get; set; } = [];
    public IReadOnlyCollection<ElementoMetricaAnaliticaDto> TopSucursales { get; set; } = [];
    public IReadOnlyCollection<ElementoMetricaAnaliticaDto> TopRangos { get; set; } = [];
    public IReadOnlyCollection<ElementoMetricaAnaliticaDto> TopErrores { get; set; } = [];
    public IReadOnlyCollection<ElementoMetricaAnaliticaDto> TopCombinaciones { get; set; } = [];
}
