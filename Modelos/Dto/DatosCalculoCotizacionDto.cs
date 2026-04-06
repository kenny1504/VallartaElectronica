namespace ElectronicaVallarta.Modelos.Dto;

public class DatosCalculoCotizacionDto
{
    public decimal TasaCambio { get; set; }
    public DateTime FechaTasa { get; set; }
    public string CodigoMoneda { get; set; } = string.Empty;
    public string SimboloMoneda { get; set; } = string.Empty;
    public string NombrePais { get; set; } = string.Empty;
    public string NombreSucursal { get; set; } = string.Empty;
}
