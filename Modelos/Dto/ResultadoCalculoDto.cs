namespace ElectronicaVallarta.Modelos.Dto;

public class ResultadoCalculoDto
{
    public bool EsExitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public int? TasaCambioRangoId { get; set; }
    public decimal MontoUsd { get; set; }
    public decimal MontoRecibe { get; set; }
    public decimal TasaCambioAplicada { get; set; }
    public decimal? RangoMontoDesdeUsd { get; set; }
    public decimal? RangoMontoHastaUsd { get; set; }
    public string? DescripcionRangoAplicado { get; set; }
    public string MonedaDestino { get; set; } = string.Empty;
    public string SimboloMoneda { get; set; } = string.Empty;
    public string NombrePais { get; set; } = string.Empty;
    public string NombreSucursal { get; set; } = string.Empty;
    public DateTime FechaTasa { get; set; }
}
