using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Modelos.Dto;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioTasaCambio
{
    /// Obtiene una colección de objetos de tipo TasaCambioRango que cumplen con los criterios establecidos.
    /// <param name="fechaFiltro">
    /// Fecha que se utiliza como filtro para obtener las tasas de cambio. Si es nulo, se obtienen todas las tasas.
    /// </param>
    /// <return>
    /// Una tarea que representa la operación asincrónica y contiene una colección de TasaCambioRango como resultado.
    /// </return>
    Task<IReadOnlyCollection<TasaCambioRango>> ObtenerTasasAsync(DateTime? fechaFiltro = null);

    /// <summary>
    /// Obtiene un listado de tasas de cambio filtrado opcionalmente por fecha.
    /// </summary>
    /// <param name="fechaFiltro">
    /// Fecha específica para filtrar las tasas de cambio. Si no se proporciona, se obtendrán todas las tasas disponibles.
    /// </param>
    /// <returns>
    /// Una colección de objetos <see cref="RegistroTasaCambioListadoDto"/> que representa las tasas de cambio obtenidas.
    /// </returns>
    Task<IReadOnlyCollection<RegistroTasaCambioListadoDto>> ObtenerListadoTasasAsync(DateTime? fechaFiltro = null, int? paisIdFiltro = null);

    /// Obtains the exchange rate range record corresponding to the specified identifier.
    /// <param name="id">
    /// The unique identifier of the exchange rate range to be retrieved.
    /// </param>
    /// <param name="soloLectura">
    /// A boolean indicating whether the entity should be retrieved in a read-only mode.
    /// Defaults to true if not specified.
    /// </param>
    /// <returns>
    /// A task that resolves to the <see cref="TasaCambioRango"/> corresponding to the specified
    /// identifier, or null if no matching record is found.
    /// </returns>
    Task<TasaCambioRango?> ObtenerTasaPorIdAsync(int id, bool soloLectura = true);

    /// Crea un nuevo registro de tipo TasaCambioRango en el sistema.
    /// <param name="tasaCambioRango">
    /// Objeto de tipo TasaCambioRango que contiene los datos a persistir, incluyendo
    /// propiedades como el país, sucursal, montos, tasa de cambio, y fecha.
    /// </param>
    /// <return>
    /// Una tarea que representa la operación asincrónica de creación del registro.
    /// </return>
    Task CrearAsync(TasaCambioRango tasaCambioRango);

    /// <summary>
    /// Actualiza una instancia existente de <see cref="TasaCambioRango"/> en el repositorio.
    /// </summary>
    /// <param name="tasaCambioRango">La entidad <see cref="TasaCambioRango"/> que contiene la información actualizada.</param>
    /// <returns>Una tarea que representa la operación asincrónica.</returns>
    /// <remarks>
    /// Si la entidad especificada no existe en el repositorio, se lanzará una excepción.
    /// </remarks>
    Task ActualizarAsync(TasaCambioRango tasaCambioRango);

    /// Elimina una tasa de cambio existente mediante su identificador.
    /// <param name="id">
    /// Identificador único de la tasa de cambio que se desea eliminar.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación de eliminación. La tarea se completa cuando la eliminación es exitosa.
    /// Si no se encuentra la tasa de cambio con el identificador proporcionado, se genera una excepción.
    /// </returns>
    Task EliminarAsync(int id);
}
