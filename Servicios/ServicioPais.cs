using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Interfaces.Repositorios;
using ElectronicaVallarta.Interfaces.Servicios;

namespace ElectronicaVallarta.Servicios;

public class ServicioPais(IRepositorioPais repositorioPais) : IServicioPais
{
    public Task<IReadOnlyCollection<Pais>> ObtenerPaisesAsync() => repositorioPais.ObtenerTodosAsync();
    public Task<IReadOnlyCollection<Pais>> ObtenerPaisesActivosAsync() => repositorioPais.ObtenerActivosAsync();
    public Task<Pais?> ObtenerPaisPorIdAsync(int id, bool soloLectura = true) => repositorioPais.ObtenerPorIdAsync(id, soloLectura);

    public async Task CrearAsync(Pais pais)
    {
        if (await repositorioPais.ExisteNombreDuplicadoAsync(pais.Nombre))
        {
            throw new InvalidOperationException("Ya existe un pais con ese nombre.");
        }

        pais.Nombre = pais.Nombre.Trim();
        pais.CodigoMoneda = pais.CodigoMoneda.Trim().ToUpper();
        pais.SimboloMoneda = pais.SimboloMoneda.Trim();
        pais.FechaCreacion = DateTime.UtcNow;
        await repositorioPais.AgregarAsync(pais);
    }

    public async Task ActualizarAsync(Pais pais)
    {
        var paisActual = await repositorioPais.ObtenerPorIdAsync(pais.Id, false)
                         ?? throw new InvalidOperationException("El pais solicitado no existe.");

        if (await repositorioPais.ExisteNombreDuplicadoAsync(pais.Nombre, pais.Id))
        {
            throw new InvalidOperationException("Ya existe un pais con ese nombre.");
        }

        paisActual.Nombre = pais.Nombre.Trim();
        paisActual.CodigoMoneda = pais.CodigoMoneda.Trim().ToUpper();
        paisActual.SimboloMoneda = pais.SimboloMoneda.Trim();
        paisActual.EstaActivo = pais.EstaActivo;
        paisActual.FechaActualizacion = DateTime.UtcNow;
        await repositorioPais.ActualizarAsync(paisActual);
    }

    public async Task EliminarAsync(int id)
    {
        var pais = await repositorioPais.ObtenerPorIdAsync(id, false)
                   ?? throw new InvalidOperationException("El pais solicitado no existe.");

        await repositorioPais.EliminarAsync(pais);
    }
}
