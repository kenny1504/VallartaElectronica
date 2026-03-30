using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElectronicaVallarta.ViewModels;

public class FormularioSucursalViewModel
{
    public int Id { get; set; }

    [Display(Name = "Pais")]
    [Required(ErrorMessage = "Selecciona un pais.")]
    public int? PaisId { get; set; }

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool EstaActivo { get; set; } = true;

    public IReadOnlyCollection<SelectListItem> Paises { get; set; } = [];
}
