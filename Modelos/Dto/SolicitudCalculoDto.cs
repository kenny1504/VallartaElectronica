using System.ComponentModel.DataAnnotations;

namespace ElectronicaVallarta.Modelos.Dto;

public class SolicitudCalculoDto
{
    [Display(Name = "Pais destino")]
    [Required(ErrorMessage = "Selecciona un pais destino.")]
    public int? PaisId { get; set; }

    [Display(Name = "Sucursal o canal")]
    [Required(ErrorMessage = "Selecciona una sucursal o canal.")]
    public int? SucursalId { get; set; }

    [Display(Name = "Monto en USD")]
    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Ingresa un monto mayor a cero.")]
    public decimal? MontoUsd { get; set; }

    public string? FechaCliente { get; set; }
}
