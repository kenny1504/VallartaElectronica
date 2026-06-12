using System.ComponentModel.DataAnnotations;

namespace ElectronicaVallarta.ViewModels;

public class CopiarTasasCambioViewModel
{
    public DateTime? FechaOrigen { get; set; }
    public int? PaisIdFiltro { get; set; }
    public bool CopiarTodas { get; set; }
    public int[] TasasSeleccionadas { get; set; } = [];

    [Required(ErrorMessage = "Selecciona la fecha destino.")]
    public DateTime FechaDestino { get; set; }
}
