namespace ElectronicaVallarta.Dominio.Entidades;

public class Pais : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string CodigoMoneda { get; set; } = string.Empty;
    public string SimboloMoneda { get; set; } = string.Empty;
    public ICollection<Sucursal> Sucursales { get; set; } = new List<Sucursal>();
    public ICollection<TasaCambioRango> TasasCambioRango { get; set; } = new List<TasaCambioRango>();
    public ICollection<RegistroConsultaAnalitica> RegistrosConsultasAnalitica { get; set; } = new List<RegistroConsultaAnalitica>();
}
