namespace ElectronicaVallarta.Modelos.Dto;

public class RegistroConsultaAnaliticaListadoDto
{
    public int Id { get; set; }
    public DateTime FechaConsultaUtc { get; set; }
    public string? NombrePais { get; set; }
    public string? NombreRegion { get; set; }
    public string? NombreSucursal { get; set; }
    public string? DescripcionRango { get; set; }
    public decimal? MontoConsultadoUsd { get; set; }
    public decimal? ResultadoObtenido { get; set; }
    public decimal? TasaCambioAplicada { get; set; }
    public long TiempoRespuestaMs { get; set; }
    public bool EsExitosa { get; set; }
    public string? IpCliente { get; set; }
    public string? IdentificadorSesionAnonima { get; set; }
    public string? MensajeError { get; set; }
}
