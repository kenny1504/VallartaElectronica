using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Repositorios;

public interface IRepositorioUsuarioAdministrador
{
    Task<UsuarioAdministrador?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, bool soloLectura = true);
    Task<bool> ExisteAlgunAdministradorAsync();
    Task AgregarAsync(UsuarioAdministrador usuarioAdministrador);
}
