namespace ElectronicaVallarta.Modelos.Dto;

public class FiltroConsultaAnaliticaDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? PaisId { get; set; }
    public string? Region { get; set; }
    public int? SucursalId { get; set; }
    public string? Rango { get; set; }
    public bool? EsExitosa { get; set; }
    public string? TextoLibre { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
}
