using ElectronicaVallarta.Modelos.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElectronicaVallarta.ViewModels;

public class CalculadoraViewModel
{
    public SolicitudCalculoDto Solicitud { get; set; } = new();
    public IReadOnlyCollection<SelectListItem> Paises { get; set; } = [];
    public IReadOnlyCollection<OpcionSucursalViewModel> Sucursales { get; set; } = [];
    public IReadOnlyCollection<TasaActivaViewModel> TasasActivas { get; set; } = [];
}
