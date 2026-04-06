namespace ElectronicaVallarta.Modelos.Dto;

public class ArchivoExportacionDto
{
    public string NombreArchivo { get; set; } = string.Empty;
    public string TipoContenido { get; set; } = string.Empty;
    public byte[] Contenido { get; set; } = [];
}
