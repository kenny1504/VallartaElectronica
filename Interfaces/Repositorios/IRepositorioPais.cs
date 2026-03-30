using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Repositorios;

public interface IRepositorioPais
{
    Task<IReadOnlyCollection<Pais>> ObtenerTodosAsync();
    Task<IReadOnlyCollection<Pais>> ObtenerActivosAsync();
    Task<Pais?> ObtenerPorIdAsync(int id, bool soloLectura = true);
    Task<bool> ExisteNombreDuplicadoAsync(string nombre, int? idExcluir = null);
    Task AgregarAsync(Pais pais);
    Task ActualizarAsync(Pais pais);
    Task EliminarAsync(Pais pais);
}
