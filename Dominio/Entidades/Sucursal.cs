namespace ElectronicaVallarta.Dominio.Entidades;

public class Sucursal : EntidadBase
{
    public int PaisId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Pais? Pais { get; set; }
    public ICollection<TasaCambioRango> TasasCambioRango { get; set; } = new List<TasaCambioRango>();
    public ICollection<RegistroConsultaAnalitica> RegistrosConsultasAnalitica { get; set; } = new List<RegistroConsultaAnalitica>();
}
