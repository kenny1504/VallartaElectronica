using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Repositorios;

public interface IRepositorioUsuarioAdministrador
{
    /// <summary>
    /// Obtiene un administrador por su nombre de usuario.
    /// </summary>
    /// <param name="nombreUsuario">
    /// El nombre de usuario del administrador que se desea buscar.
    /// Este parámetro no es sensible a mayúsculas y minúsculas.
    /// </param>
    /// <param name="soloLectura">
    /// Indica si la consulta se realizará en modo solo lectura.
    /// Esto puede mejorar el rendimiento al evitar el rastreo de cambios.
    /// Por defecto, está configurado en <c>true</c>.
    /// </param>
    /// <return>
    /// Un objeto <see cref="UsuarioAdministrador"/> si se encuentra un administrador
    /// cuyo nombre de usuario coincide con el proporcionado; de lo contrario, retorna <c>null</c>.
    /// </return>
    Task<UsuarioAdministrador?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, bool soloLectura = true);

    /// Comprueba si existe al menos un administrador en el sistema.
    /// Este método realiza una consulta a la base de datos
    /// para determinar si hay algún usuario administrador registrado.
    /// <return>
    /// Devuelve un valor booleano que indica si existe al menos un usuario administrador.
    /// </return>
    Task<bool> ExisteAlgunAdministradorAsync();

    /// Asynchronously adds a new UsuarioAdministrador entity to the data store.
    /// <param name="usuarioAdministrador">
    /// The UsuarioAdministrador entity to be added. It must include all required properties populated.
    /// </param>
    /// <returns>
    /// A Task representing the asynchronous operation.
    /// </returns>
    Task AgregarAsync(UsuarioAdministrador usuarioAdministrador);
}
