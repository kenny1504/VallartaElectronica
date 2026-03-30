using ElectronicaVallarta.Datos;
using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace ElectronicaVallarta.Repositorios;

public class RepositorioPais(ContextoAplicacion contexto) : IRepositorioPais
{
    public async Task<IReadOnlyCollection<Pais>> ObtenerTodosAsync() =>
        await contexto.Paises.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();

    public async Task<IReadOnlyCollection<Pais>> ObtenerActivosAsync() =>
        await contexto.Paises.AsNoTracking().Where(x => x.EstaActivo).OrderBy(x => x.Nombre).ToListAsync();

    public async Task<Pais?> ObtenerPorIdAsync(int id, bool soloLectura = true)
    {
        var consulta = contexto.Paises.AsQueryable();
        if (soloLectura)
        {
            consulta = consulta.AsNoTracking();
        }

        return await consulta.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExisteNombreDuplicadoAsync(string nombre, int? idExcluir = null)
    {
        var nombreNormalizado = nombre.Trim().ToUpper();
        return await contexto.Paises.AsNoTracking()
            .AnyAsync(x => x.Nombre.ToUpper() == nombreNormalizado && (!idExcluir.HasValue || x.Id != idExcluir.Value));
    }

    public async Task AgregarAsync(Pais pais)
    {
        await contexto.Paises.AddAsync(pais);
        await contexto.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Pais pais)
    {
        contexto.Paises.Update(pais);
        await contexto.SaveChangesAsync();
    }

    public async Task EliminarAsync(Pais pais)
    {
        contexto.Paises.Remove(pais);
        await contexto.SaveChangesAsync();
    }
}
