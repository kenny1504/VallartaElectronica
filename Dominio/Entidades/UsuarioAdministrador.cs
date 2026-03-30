namespace ElectronicaVallarta.Dominio.Entidades;

public class UsuarioAdministrador : EntidadBase
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string ClaveHash { get; set; } = string.Empty;
}
