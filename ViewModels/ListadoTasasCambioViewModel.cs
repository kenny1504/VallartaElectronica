using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.ViewModels;

public class ListadoTasasCambioViewModel
{
    public DateTime? FechaFiltro { get; set; }
    public bool MostrarTodos { get; set; }
    public IReadOnlyCollection<TasaCambioRango> Tasas { get; set; } = [];
}
