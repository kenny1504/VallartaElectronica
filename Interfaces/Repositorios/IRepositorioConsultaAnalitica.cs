using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Modelos.Dto;

namespace ElectronicaVallarta.Interfaces.Repositorios;

public interface IRepositorioConsultaAnalitica
{
    Task AgregarAsync(RegistroConsultaAnalitica registroConsultaAnalitica);
    Task<ResumenDashboardAnaliticaDto> ObtenerResumenAsync(FiltroConsultaAnaliticaDto filtro);
    Task<PaginacionConsultaAnaliticaDto> ObtenerListadoPaginadoAsync(FiltroConsultaAnaliticaDto filtro);
    Task<IReadOnlyCollection<RegistroConsultaAnaliticaListadoDto>> ObtenerExportacionAsync(FiltroConsultaAnaliticaDto filtro);
    Task<OpcionesFiltroAnaliticaDto> ObtenerOpcionesFiltroAsync();
}
