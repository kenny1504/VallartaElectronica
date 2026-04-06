namespace ElectronicaVallarta.Dominio.Entidades;

public class TasaCambioRango : EntidadBase
{
    public int PaisId { get; set; }
    public int SucursalId { get; set; }
    public decimal MontoDesdeUsd { get; set; }
    public decimal? MontoHastaUsd { get; set; }
    public decimal TasaCambio { get; set; }
    public DateTime FechaTasa { get; set; }
    public Pais? Pais { get; set; }
    public Sucursal? Sucursal { get; set; }
    public ICollection<RegistroConsultaAnalitica> RegistrosConsultasAnalitica { get; set; } = new List<RegistroConsultaAnalitica>();
}
