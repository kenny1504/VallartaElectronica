using ElectronicaVallarta.Dominio.Enumeraciones;

namespace ElectronicaVallarta.Dominio.Entidades;

public class Publicidad : EntidadBase
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public TipoRecursoPublicidad TipoRecurso { get; set; }
    public string UrlRecurso { get; set; } = string.Empty;
    public int DuracionSegundos { get; set; }
    public int Orden { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
}
