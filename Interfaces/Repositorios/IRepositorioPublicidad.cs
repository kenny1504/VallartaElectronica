using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Repositorios;

public interface IRepositorioPublicidad
{
    Task<IReadOnlyCollection<Publicidad>> ObtenerTodasAsync();
    Task<IReadOnlyCollection<Publicidad>> ObtenerActivasVigentesAsync(DateTime fechaActual);
    Task<Publicidad?> ObtenerPorIdAsync(int id, bool soloLectura = true);
    Task AgregarAsync(Publicidad publicidad);
    Task ActualizarAsync(Publicidad publicidad);
    Task EliminarAsync(Publicidad publicidad);
}
