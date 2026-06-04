namespace ElectronicaVallarta.Dominio.Entidades;

public class RegistroLogAplicacion
{
    public int Id { get; set; }
    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
    public string Nivel { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? DetalleExcepcion { get; set; }
    public string? Propiedades { get; set; }
    public string? Ruta { get; set; }
    public string? MetodoHttp { get; set; }
    public string? Usuario { get; set; }
    public string? TraceIdentifier { get; set; }
    public string? Ambiente { get; set; }
}
