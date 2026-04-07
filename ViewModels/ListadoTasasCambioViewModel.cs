using ElectronicaVallarta.Modelos.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElectronicaVallarta.ViewModels;

public class ListadoTasasCambioViewModel
{
    public DateTime? FechaFiltro { get; set; }
    public int? PaisIdFiltro { get; set; }
    public bool MostrarTodos { get; set; }
    public IReadOnlyCollection<SelectListItem> Paises { get; set; } = [];
    public IReadOnlyCollection<RegistroTasaCambioListadoDto> Tasas { get; set; } = [];
}
