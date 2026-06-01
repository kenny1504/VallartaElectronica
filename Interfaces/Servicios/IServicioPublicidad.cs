using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Modelos.Dto;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioPublicidad
{
    Task<IReadOnlyCollection<Publicidad>> ObtenerPublicidadesAsync();
    Task<IReadOnlyCollection<PublicidadActivaDto>> ObtenerPublicidadesActivasAsync(DateTime fechaActual);
    Task<Publicidad?> ObtenerPublicidadPorIdAsync(int id, bool soloLectura = true);
    Task CrearAsync(Publicidad publicidad, IFormFile? archivo);
    Task ActualizarAsync(Publicidad publicidad, IFormFile? archivo);
    Task CambiarEstadoAsync(int id);
    Task EliminarAsync(int id);
}
