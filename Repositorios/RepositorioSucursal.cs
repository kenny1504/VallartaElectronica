using ElectronicaVallarta.Datos;
using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace ElectronicaVallarta.Repositorios;

public class RepositorioSucursal(ContextoAplicacion contexto) : IRepositorioSucursal
{
    public async Task<IReadOnlyCollection<Sucursal>> ObtenerTodosAsync() =>
        await contexto.Sucursales.AsNoTracking()
            .Include(x => x.Pais)
            .OrderBy(x => x.Pais!.Nombre)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

    public async Task<IReadOnlyCollection<Sucursal>> ObtenerActivasAsync() =>
        await contexto.Sucursales.AsNoTracking()
            .Where(x => x.EstaActivo && x.Pais != null && x.Pais.EstaActivo)
            .OrderBy(x => x.PaisId)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

    public async Task<IReadOnlyCollection<Sucursal>> ObtenerActivasPorPaisAsync(int paisId) =>
        await contexto.Sucursales.AsNoTracking()
            .Where(x => x.PaisId == paisId && x.EstaActivo)
            .OrderBy(x => x.Nombre)
            .ToListAsync();

    public async Task<Sucursal?> ObtenerPorIdAsync(int id, bool soloLectura = true)
    {
        IQueryable<Sucursal> consulta = contexto.Sucursales;
        if (soloLectura)
        {
            consulta = consulta.AsNoTracking().Include(x => x.Pais);
        }
        else
        {
            consulta = consulta.AsTracking();
        }

        return await consulta.FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<bool> ExisteActivaEnPaisAsync(int sucursalId, int paisId) =>
        contexto.Sucursales.AsNoTracking()
            .AnyAsync(x => x.Id == sucursalId && x.PaisId == paisId && x.EstaActivo && x.Pais != null && x.Pais.EstaActivo);

    public async Task<bool> ExisteNombreDuplicadoAsync(int paisId, string nombre, int? idExcluir = null)
    {
        // Evita funciones sobre la columna para aprovechar el índice compuesto de pais y nombre.
        var nombreNormalizado = nombre.Trim();
        return await contexto.Sucursales.AsNoTracking()
            .AnyAsync(x => x.PaisId == paisId && x.Nombre == nombreNormalizado && (!idExcluir.HasValue || x.Id != idExcluir.Value));
    }

    public async Task AgregarAsync(Sucursal sucursal)
    {
        await contexto.Sucursales.AddAsync(sucursal);
        await contexto.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Sucursal sucursal)
    {
        contexto.Sucursales.Update(sucursal);
        await contexto.SaveChangesAsync();
    }

    public async Task EliminarAsync(Sucursal sucursal)
    {
        contexto.Sucursales.Remove(sucursal);
        await contexto.SaveChangesAsync();
    }
}
