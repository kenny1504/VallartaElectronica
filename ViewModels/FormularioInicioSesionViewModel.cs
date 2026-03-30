using System.ComponentModel.DataAnnotations;

namespace ElectronicaVallarta.ViewModels;

public class FormularioInicioSesionViewModel
{
    [Display(Name = "Usuario")]
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Display(Name = "Clave")]
    [Required(ErrorMessage = "La clave es obligatoria.")]
    [DataType(DataType.Password)]
    public string Clave { get; set; } = string.Empty;

    public string? UrlRetorno { get; set; }
}
