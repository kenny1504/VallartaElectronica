using ElectronicaVallarta.Datos;
using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace ElectronicaVallarta.Repositorios;

public class RepositorioUsuarioAdministrador(ContextoAplicacion contexto) : IRepositorioUsuarioAdministrador
{
    public async Task<UsuarioAdministrador?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, bool soloLectura = true)
    {
        var consulta = contexto.UsuariosAdministradores.AsQueryable();
        if (soloLectura)
        {
            consulta = consulta.AsNoTracking();
        }

        var nombreNormalizado = nombreUsuario.Trim().ToUpper();
        return await consulta.FirstOrDefaultAsync(x => x.NombreUsuario.ToUpper() == nombreNormalizado && x.EstaActivo);
    }

    public Task<bool> ExisteAlgunAdministradorAsync()
    {
        return contexto.UsuariosAdministradores.AsNoTracking().AnyAsync();
    }

    public async Task AgregarAsync(UsuarioAdministrador usuarioAdministrador)
    {
        await contexto.UsuariosAdministradores.AddAsync(usuarioAdministrador);
        await contexto.SaveChangesAsync();
    }
}
