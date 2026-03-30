using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Repositorios;

public interface IRepositorioSucursal
{
    Task<IReadOnlyCollection<Sucursal>> ObtenerTodosAsync();
    Task<IReadOnlyCollection<Sucursal>> ObtenerActivasPorPaisAsync(int paisId);
    Task<Sucursal?> ObtenerPorIdAsync(int id, bool soloLectura = true);
    Task<bool> ExisteNombreDuplicadoAsync(int paisId, string nombre, int? idExcluir = null);
    Task AgregarAsync(Sucursal sucursal);
    Task ActualizarAsync(Sucursal sucursal);
    Task EliminarAsync(Sucursal sucursal);
}
