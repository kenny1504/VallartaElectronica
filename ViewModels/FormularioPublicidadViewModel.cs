using System.ComponentModel.DataAnnotations;
using ElectronicaVallarta.Dominio.Enumeraciones;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElectronicaVallarta.ViewModels;

public class FormularioPublicidadViewModel
{
    public int Id { get; set; }

    [Display(Name = "Titulo")]
    [Required(ErrorMessage = "El titulo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El titulo no puede superar 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Display(Name = "Descripcion")]
    [StringLength(500, ErrorMessage = "La descripcion no puede superar 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Display(Name = "Tipo de recurso")]
    [Required(ErrorMessage = "Selecciona el tipo de recurso.")]
    public TipoRecursoPublicidad? TipoRecurso { get; set; }

    [Display(Name = "Archivo")]
    public IFormFile? Archivo { get; set; }

    public string? UrlRecursoActual { get; set; }

    [Display(Name = "Duracion en segundos")]
    [Range(1, 3600, ErrorMessage = "La duracion debe estar entre 1 y 3600 segundos.")]
    public int DuracionSegundos { get; set; } = 8;

    [Display(Name = "Orden")]
    [Range(0, 9999, ErrorMessage = "El orden debe estar entre 0 y 9999.")]
    public int Orden { get; set; }

    [Display(Name = "Activo")]
    public bool EstaActivo { get; set; } = true;

    [Display(Name = "Fecha inicio")]
    public DateTime? FechaInicio { get; set; }

    [Display(Name = "Fecha fin")]
    public DateTime? FechaFin { get; set; }

    public IReadOnlyCollection<SelectListItem> TiposRecurso { get; set; } = [];
}
