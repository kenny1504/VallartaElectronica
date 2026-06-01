using ElectronicaVallarta.Datos;
using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace ElectronicaVallarta.Repositorios;

public class RepositorioPublicidad(ContextoAplicacion contexto) : IRepositorioPublicidad
{
    public async Task<IReadOnlyCollection<Publicidad>> ObtenerTodasAsync() =>
        await contexto.Publicidades.AsNoTracking()
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Titulo)
            .ToListAsync();

    public async Task<IReadOnlyCollection<Publicidad>> ObtenerActivasVigentesAsync(DateTime fechaActual) =>
        await contexto.Publicidades.AsNoTracking()
            .Where(x => x.EstaActivo
                        && (!x.FechaInicio.HasValue || x.FechaInicio.Value <= fechaActual)
                        && (!x.FechaFin.HasValue || x.FechaFin.Value >= fechaActual))
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Id)
            .ToListAsync();

    public async Task<Publicidad?> ObtenerPorIdAsync(int id, bool soloLectura = true)
    {
        IQueryable<Publicidad> consulta = contexto.Publicidades;
        consulta = soloLectura ? consulta.AsNoTracking() : consulta.AsTracking();
        return await consulta.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AgregarAsync(Publicidad publicidad)
    {
        await contexto.Publicidades.AddAsync(publicidad);
        await contexto.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Publicidad publicidad)
    {
        contexto.Publicidades.Update(publicidad);
        await contexto.SaveChangesAsync();
    }

    public async Task EliminarAsync(Publicidad publicidad)
    {
        contexto.Publicidades.Remove(publicidad);
        await contexto.SaveChangesAsync();
    }
}
