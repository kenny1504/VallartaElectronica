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

    public async Task<IReadOnlyCollection<Sucursal>> ObtenerActivasPorPaisAsync(int paisId) =>
        await contexto.Sucursales.AsNoTracking()
            .Where(x => x.PaisId == paisId && x.EstaActivo)
            .OrderBy(x => x.Nombre)
            .ToListAsync();

    public async Task<Sucursal?> ObtenerPorIdAsync(int id, bool soloLectura = true)
    {
        var consulta = contexto.Sucursales.Include(x => x.Pais).AsQueryable();
        if (soloLectura)
        {
            consulta = consulta.AsNoTracking();
        }

        return await consulta.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExisteNombreDuplicadoAsync(int paisId, string nombre, int? idExcluir = null)
    {
        var nombreNormalizado = nombre.Trim().ToUpper();
        return await contexto.Sucursales.AsNoTracking()
            .AnyAsync(x => x.PaisId == paisId && x.Nombre.ToUpper() == nombreNormalizado && (!idExcluir.HasValue || x.Id != idExcluir.Value));
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
