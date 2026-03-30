using System.ComponentModel.DataAnnotations;

namespace ElectronicaVallarta.ViewModels;

public class FormularioPaisViewModel
{
    public int Id { get; set; }

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre no puede superar 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Codigo moneda")]
    [Required(ErrorMessage = "El codigo de moneda es obligatorio.")]
    [StringLength(10, ErrorMessage = "El codigo de moneda no puede superar 10 caracteres.")]
    public string CodigoMoneda { get; set; } = string.Empty;

    [Display(Name = "Simbolo moneda")]
    [Required(ErrorMessage = "El simbolo de moneda es obligatorio.")]
    [StringLength(10, ErrorMessage = "El simbolo no puede superar 10 caracteres.")]
    public string SimboloMoneda { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool EstaActivo { get; set; } = true;
}
