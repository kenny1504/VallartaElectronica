using ElectronicaVallarta.Modelos.Dto;

namespace ElectronicaVallarta.ViewModels;

public class ListadoTasasCambioViewModel
{
    public DateTime? FechaFiltro { get; set; }
    public bool MostrarTodos { get; set; }
    public IReadOnlyCollection<RegistroTasaCambioListadoDto> Tasas { get; set; } = [];
}
