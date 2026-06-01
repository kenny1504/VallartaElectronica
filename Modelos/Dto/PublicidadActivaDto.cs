namespace ElectronicaVallarta.Modelos.Dto;

public class PublicidadActivaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string TipoRecurso { get; set; } = string.Empty;
    public string UrlRecurso { get; set; } = string.Empty;
    public int DuracionSegundos { get; set; }
    public int Orden { get; set; }
}
