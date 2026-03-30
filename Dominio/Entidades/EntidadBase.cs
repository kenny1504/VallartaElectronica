namespace ElectronicaVallarta.Dominio.Entidades;

public abstract class EntidadBase
{
    public int Id { get; set; }
    public bool EstaActivo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
}
