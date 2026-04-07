namespace ElectronicaVallarta.ViewModels;

public class TasaActivaViewModel
{
    public int PaisId { get; set; }
    public int SucursalId { get; set; }
    public DateTime FechaTasa { get; set; }
    public string NombreSucursal { get; set; } = string.Empty;
    public string NombrePais { get; set; } = string.Empty;
    public decimal MontoDesdeUsd { get; set; }
    public decimal? MontoHastaUsd { get; set; }
    public decimal TasaCambio { get; set; }
    public string CodigoMoneda { get; set; } = string.Empty;
}
