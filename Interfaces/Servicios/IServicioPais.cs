using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioPais
{
    Task<IReadOnlyCollection<Pais>> ObtenerPaisesAsync();
    Task<IReadOnlyCollection<Pais>> ObtenerPaisesActivosAsync();
    Task<Pais?> ObtenerPaisPorIdAsync(int id, bool soloLectura = true);
    Task CrearAsync(Pais pais);
    Task ActualizarAsync(Pais pais);
    Task EliminarAsync(int id);
}
