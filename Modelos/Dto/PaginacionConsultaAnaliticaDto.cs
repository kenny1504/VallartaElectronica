namespace ElectronicaVallarta.Modelos.Dto;

public class PaginacionConsultaAnaliticaDto
{
    public int TotalRegistros { get; set; }
    public int PaginaActual { get; set; }
    public int TamanoPagina { get; set; }
    public IReadOnlyCollection<RegistroConsultaAnaliticaListadoDto> Registros { get; set; } = [];
}
