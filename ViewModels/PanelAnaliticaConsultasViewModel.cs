using ElectronicaVallarta.Modelos.Dto;

namespace ElectronicaVallarta.ViewModels;

public class PanelAnaliticaConsultasViewModel
{
    public FiltroConsultaAnaliticaDto Filtro { get; set; } = new();
    public ResumenDashboardAnaliticaDto Resumen { get; set; } = new();
    public PaginacionConsultaAnaliticaDto Listado { get; set; } = new();
    public OpcionesFiltroAnaliticaDto OpcionesFiltro { get; set; } = new();
}
