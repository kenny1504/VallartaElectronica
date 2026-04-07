using ElectronicaVallarta.Dominio.Entidades;
using ElectronicaVallarta.Modelos.Dto;

namespace ElectronicaVallarta.Interfaces.Repositorios;

public interface IRepositorioTasaCambio
{
    /// Obtiene un listado de rangos de tasas de cambio existentes en el sistema.
    /// Este método devuelve una colección inmutable de objetos del tipo TasaCambioRango,
    /// que representa los detalles de las tasas de cambio configuradas, tales como:
    /// país, sucursal, montos en USD y la tasa de cambio correspondiente.
    /// La colección resultante está ordenada de forma descendente por la fecha de la tasa,
    /// y luego de forma ascendente por el nombre del país, el nombre de la sucursal
    /// y el monto mínimo en USD.
    /// <return>
    /// Una tarea que representa la operación asincrónica.
    /// El valor de la tarea contiene una colección inmutable de objetos TasaCambioRango.
    /// </return>
    Task<IReadOnlyCollection<TasaCambioRango>> ObtenerTodosAsync(DateTime? fechaFiltro = null);
    Task<IReadOnlyCollection<RegistroTasaCambioListadoDto>> ObtenerListadoAsync(DateTime? fechaFiltro = null, int? paisIdFiltro = null);

    /// <summary>
    /// Obtiene una entidad <see cref="TasaCambioRango"/> por su identificador único.
    /// </summary>
    /// <param name="id">El identificador único de la entidad a buscar.</param>
    /// <param name="soloLectura">
    /// Un valor booleano que indica si la entidad debe ser consultada en modo de solo lectura.
    /// Si es <c>true</c>, la entidad no será rastreada por el contexto de la base de datos.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica, conteniendo la entidad <see cref="TasaCambioRango"/> si se encuentra; de lo contrario, <c>null</c>.
    /// </returns>
    Task<TasaCambioRango?> ObtenerPorIdAsync(int id, bool soloLectura = true);

    /// <summary>
    /// Obtiene la tasa de cambio aplicable según los parámetros proporcionados.
    /// </summary>
    /// <param name="paisId">Identificador único del país al que pertenece la tasa de cambio.</param>
    /// <param name="sucursalId">Identificador único de la sucursal asociada a la tasa de cambio.</param>
    /// <param name="fechaTasa">Fecha específica para la cual se desea consultar la tasa de cambio.</param>
    /// <param name="montoUsd">Monto en dólares estadounidenses que será utilizado para determinar el rango aplicable.</param>
    /// <returns>
    /// Una instancia de <see cref="TasaCambioRango"/> que representa la tasa de cambio aplicable
    /// basada en los parámetros proporcionados, o <see langword="null"/> si no existe una tasa aplicable.
    /// </returns>
    Task<TasaCambioRango?> ObtenerTasaAplicableAsync(int paisId, int sucursalId, DateTime fechaTasa, decimal montoUsd);
    Task<DatosCalculoCotizacionDto?> ObtenerDatosCalculoAsync(int paisId, int sucursalId, DateTime fechaTasa, decimal montoUsd);

    /// <summary>
    /// Verifica si existe traslape entre el rango de tasas de cambio especificado y otros registros existentes.
    /// </summary>
    /// <param name="tasaCambioRango">
    /// Objeto que representa el rango de tasa de cambio que se desea validar.
    /// </param>
    /// <param name="idExcluir">
    /// Identificador opcional del registro a excluir de la validación, útil cuando se está actualizando un rango existente.
    /// </param>
    /// <returns>
    /// Un valor booleano que indica si existe traslape con otros rangos de tasa de cambio.
    /// Devuelve <c>true</c> si existe un traslape, de lo contrario <c>false</c>.
    /// </returns>
    Task<bool> ExisteTraslapeAsync(TasaCambioRango tasaCambioRango, int? idExcluir = null);

    /// Agrega un nuevo rango de tasa de cambio al repositorio.
    /// <param name="tasaCambioRango">
    /// El rango de tasa de cambio que se desea agregar. Contiene información como el país, sucursal,
    /// rango de montos en USD, la tasa de cambio aplicable y la fecha.
    /// </param>
    Task AgregarAsync(TasaCambioRango tasaCambioRango);

    /// Actualiza un rango de tasa de cambio existente en el repositorio.
    /// <param name="tasaCambioRango">
    /// El objeto de tipo <see cref="TasaCambioRango"/> que contiene los datos actualizados del rango de tasa de cambio.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica.
    /// </returns>
    Task ActualizarAsync(TasaCambioRango tasaCambioRango);

    /// Elimina una entidad de tipo TasaCambioRango del repositorio.
    /// <param name="tasaCambioRango">
    /// La instancia de TasaCambioRango que se desea eliminar del repositorio.
    /// </param>
    /// <returns>
    /// Una tarea representando la operación asincrónica.
    /// </returns>
    Task EliminarAsync(TasaCambioRango tasaCambioRango);
}
