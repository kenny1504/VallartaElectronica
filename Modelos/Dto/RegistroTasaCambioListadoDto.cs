namespace ElectronicaVallarta.Modelos.Dto;

public class RegistroTasaCambioListadoDto
{
    public int Id { get; set; }
    public DateTime FechaTasa { get; set; }
    public string NombrePais { get; set; } = string.Empty;
    public string NombreSucursal { get; set; } = string.Empty;
    public decimal MontoDesdeUsd { get; set; }
    public decimal? MontoHastaUsd { get; set; }
    public decimal TasaCambio { get; set; }
    public bool EstaActivo { get; set; }
}
