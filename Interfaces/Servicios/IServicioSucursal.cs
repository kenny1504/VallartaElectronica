using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioSucursal
{
    /// Obtains a read-only collection of all branches (sucursales) available in the system.
    /// The returned collection may include both active and inactive branches, depending
    /// on the business logic within the implementation.
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a read-only collection of branches.</returns>
    Task<IReadOnlyCollection<Sucursal>> ObtenerSucursalesAsync();

    /// Obteniene una colección de sucursales que se encuentran activas.
    /// Las sucursales activas son aquellas que cumplen con los criterios
    /// definidos para considerarse en funcionamiento o disponibles.
    /// <return>Una tarea que representa la operación asincrónica. La tarea contiene una colección de objetos del tipo Sucursal.</return>
    Task<IReadOnlyCollection<Sucursal>> ObtenerSucursalesActivasAsync();

    /// <summary>
    /// Obtiene una colección de sucursales activas asociadas a un país especificado.
    /// </summary>
    /// <param name="paisId">El identificador único del país para el cual se desean obtener las sucursales activas.</param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica. El valor de la tarea contiene una colección de
    /// sucursales activas correspondientes al país especificado.
    /// </returns>
    Task<IReadOnlyCollection<Sucursal>> ObtenerSucursalesActivasPorPaisAsync(int paisId);

    /// Obtiene una sucursal por su identificador.
    /// <param name="id">
    /// Identificador único de la sucursal a obtener.
    /// </param>
    /// <param name="soloLectura">
    /// Indica si la operación debe realizarse en un contexto de solo lectura.
    /// El valor predeterminado es true.
    /// </param>
    /// <return>
    /// Un objeto de tipo <see cref="Sucursal"/> si se encuentra una sucursal
    /// con el identificador especificado; de lo contrario, null.
    /// </return>
    Task<Sucursal?> ObtenerSucursalPorIdAsync(int id, bool soloLectura = true);

    /// <summary>
    /// Crea una nueva sucursal en el sistema.
    /// </summary>
    /// <param name="sucursal">La sucursal que se desea crear. Debe contener un nombre único en el país especificado.</param>
    /// <returns>Una tarea que representa la operación asincrónica de creación.</returns>
    /// <exception cref="InvalidOperationException">Se lanza si ya existe una sucursal con el mismo nombre en el país seleccionado.</exception>
    Task CrearAsync(Sucursal sucursal);

    /// Actualiza los datos de una sucursal existente.
    /// <param name="sucursal">
    /// Objeto de tipo <see cref="Sucursal"/> que contiene los datos actualizados de la sucursal.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica de actualización.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Se lanza si la sucursal especificada no existe en el sistema.
    /// </exception>
    Task ActualizarAsync(Sucursal sucursal);

    /// Elimina una sucursal en función de su identificador único.
    /// <param name="id">El identificador único de la sucursal que se desea eliminar.</param>
    /// <returns>Una tarea que representa la operación asincrónica.</returns>
    /// <exception cref="InvalidOperationException">Se produce si la sucursal especificada no existe.</exception>
    Task EliminarAsync(int id);
}
