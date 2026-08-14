using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElectronicaVallarta.ViewModels;

public class EdicionMasivaTasasCambioViewModel
{
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "Selecciona la fecha de las tasas.")]
    public DateTime FechaTasa { get; set; } = DateTime.Today;

    [Display(Name = "Pais")]
    [Required(ErrorMessage = "Selecciona un pais.")]
    public int? PaisId { get; set; }

    public string? NombrePais { get; set; }
    public IReadOnlyCollection<SelectListItem> Paises { get; set; } = [];
    public List<TasaCambioEdicionMasivaItemViewModel> Tasas { get; set; } = [];
}

public class TasaCambioEdicionMasivaItemViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingresa una tasa de cambio.")]
    [Range(typeof(decimal), "0.000001", "999999999", ErrorMessage = "La tasa debe ser mayor a cero.")]
    public decimal? TasaCambio { get; set; }

    public string NombreSucursal { get; set; } = string.Empty;
    public decimal MontoDesdeUsd { get; set; }
    public decimal? MontoHastaUsd { get; set; }
    public bool EstaActivo { get; set; }
}
