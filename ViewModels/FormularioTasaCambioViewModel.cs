using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElectronicaVallarta.ViewModels;

public class FormularioTasaCambioViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Display(Name = "Pais")]
    [Required(ErrorMessage = "Selecciona un pais.")]
    public int? PaisId { get; set; }

    [Display(Name = "Sucursal o canal")]
    [Required(ErrorMessage = "Selecciona una sucursal o canal.")]
    public int? SucursalId { get; set; }

    [Display(Name = "Monto desde USD")]
    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "El monto desde debe ser mayor a cero.")]
    public decimal? MontoDesdeUsd { get; set; }

    [Display(Name = "Monto hasta USD")]
    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "El monto hasta debe ser mayor a cero.")]
    public decimal? MontoHastaUsd { get; set; }

    [Display(Name = "Tasa de cambio")]
    [Range(typeof(decimal), "0.000001", "999999999", ErrorMessage = "La tasa debe ser mayor a cero.")]
    public decimal? TasaCambio { get; set; }

    [Display(Name = "Fecha de tasa")]
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateTime FechaTasa { get; set; } = DateTime.Today;

    [Display(Name = "Activo")]
    public bool EstaActivo { get; set; } = true;

    public IReadOnlyCollection<SelectListItem> Paises { get; set; } = [];
    public IReadOnlyCollection<OpcionSucursalViewModel> Sucursales { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MontoDesdeUsd.HasValue && MontoHastaUsd.HasValue && MontoDesdeUsd > MontoHastaUsd)
        {
            yield return new ValidationResult("El monto desde no puede ser mayor que el monto hasta.", [nameof(MontoDesdeUsd), nameof(MontoHastaUsd)]);
        }
    }
}
