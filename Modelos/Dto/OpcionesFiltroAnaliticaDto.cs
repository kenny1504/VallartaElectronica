namespace ElectronicaVallarta.Modelos.Dto;

public class OpcionesFiltroAnaliticaDto
{
    public IReadOnlyCollection<OpcionFiltroAnaliticaDto> Paises { get; set; } = [];
    public IReadOnlyCollection<OpcionFiltroAnaliticaDto> Regiones { get; set; } = [];
    public IReadOnlyCollection<OpcionFiltroAnaliticaDto> Sucursales { get; set; } = [];
    public IReadOnlyCollection<OpcionFiltroAnaliticaDto> Rangos { get; set; } = [];
}
