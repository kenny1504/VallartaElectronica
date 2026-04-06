using ElectronicaVallarta.Modelos.Dto;
using ElectronicaVallarta.ViewModels;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioAnaliticaConsultas
{
    Task RegistrarConsultaAsync(RegistroConsultaAnaliticaEntradaDto registroConsulta);
    Task<PanelAnaliticaConsultasViewModel> ObtenerPanelAsync(FiltroConsultaAnaliticaDto filtro);
    Task<ArchivoExportacionDto> ExportarAsync(FiltroConsultaAnaliticaDto filtro);
}
