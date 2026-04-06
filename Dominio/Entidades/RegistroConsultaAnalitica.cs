namespace ElectronicaVallarta.Dominio.Entidades;

public class RegistroConsultaAnalitica : EntidadBase
{
    public DateTime FechaConsultaUtc { get; set; }
    public int? PaisId { get; set; }
    public string? NombrePais { get; set; }
    public string? RegionId { get; set; }
    public string? NombreRegion { get; set; }
    public int? SucursalId { get; set; }
    public string? NombreSucursal { get; set; }
    public int? TasaCambioRangoId { get; set; }
    public decimal? RangoMontoDesdeUsd { get; set; }
    public decimal? RangoMontoHastaUsd { get; set; }
    public string? DescripcionRango { get; set; }
    public decimal? MontoConsultadoUsd { get; set; }
    public decimal? ResultadoObtenido { get; set; }
    public decimal? TasaCambioAplicada { get; set; }
    public bool EsExitosa { get; set; }
    public string? MensajeError { get; set; }
    public string? IpCliente { get; set; }
    public string? UserAgent { get; set; }
    public string? IdiomaNavegador { get; set; }
    public string RutaOrigen { get; set; } = string.Empty;
    public string? Referer { get; set; }
    public long TiempoRespuestaMs { get; set; }
    public string? IdentificadorSesionAnonima { get; set; }
    public string? MetodoHttp { get; set; }
    public Pais? Pais { get; set; }
    public Sucursal? Sucursal { get; set; }
    public TasaCambioRango? TasaCambioRango { get; set; }
}
