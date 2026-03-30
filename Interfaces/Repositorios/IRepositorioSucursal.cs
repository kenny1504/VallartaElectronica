using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Repositorios;

public interface IRepositorioSucursal
{
    /// Obtiene todas las sucursales registradas en el sistema.
    /// Este método devuelve una colección de solo lectura que contiene todas
    /// las instancias de sucursales disponibles en la base de datos, organizadas
    /// por los nombres de los países a los que pertenecen, seguidos de los nombres
    /// de las sucursales.
    /// <return>
    /// Una tarea que representa la operación asíncrona de obtención de datos.
    /// El resultado contiene una colección de solo lectura de objetos de tipo Sucursal.
    /// </return>
    Task<IReadOnlyCollection<Sucursal>> ObtenerTodosAsync();

    /// <summary>
    /// Obtiene una colección de sucursales activas asociadas a un país especificado.
    /// </summary>
    /// <param name="paisId">El identificador único del país para el cual se desean obtener las sucursales activas.</param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica. El valor de la tarea contiene una colección de
    /// sucursales activas correspondientes al país especificado.
    /// </returns>
    Task<IReadOnlyCollection<Sucursal>> ObtenerActivasPorPaisAsync(int paisId);

    /// Obtiene una entidad del tipo Sucursal utilizando su identificador único.
    /// <param name="id">
    /// Identificador único de la sucursal que se desea obtener.
    /// </param>
    /// <param name="soloLectura">
    /// Indica si la operación debe realizarse en un contexto de solo lectura.
    /// El valor predeterminado es true.
    /// </param>
    /// <return>
    /// Un objeto del tipo Sucursal si existe un registro con el identificador proporcionado;
    /// de lo contrario, retorna null.
    /// </return>
    Task<Sucursal?> ObtenerPorIdAsync(int id, bool soloLectura = true);

    /// Verifies if there is a duplicate name for a branch within the specified country,
    /// optionally excluding a specific branch by its identifier.
    /// <param name="paisId">The identifier of the country where the search will be conducted.</param>
    /// <param name="nombre">The name of the branch to check for duplicates.</param>
    /// <param name="idExcluir">
    /// The identifier of the branch to exclude from the duplicate verification.
    /// Pass null if no exclusion is needed.
    /// </param>
    /// <return>
    /// A task that represents the asynchronous operation. The task result contains
    /// a boolean value indicating whether a duplicate name exists. Returns true if
    /// the name already exists, otherwise false.
    /// </return>
    Task<bool> ExisteNombreDuplicadoAsync(int paisId, string nombre, int? idExcluir = null);

    /// Agrega de manera asíncrona una nueva sucursal al repositorio.
    /// <param name="sucursal">
    /// La instancia de la entidad <see cref="Sucursal"/> que se desea agregar.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación asíncrona.
    /// </returns>
    Task AgregarAsync(Sucursal sucursal);

    /// Actualiza la información de una sucursal existente en el sistema.
    /// <param name="sucursal">
    /// Objeto de tipo <see cref="Sucursal"/> que contiene los datos actualizados de la sucursal.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica.
    /// </returns>
    Task ActualizarAsync(Sucursal sucursal);

    /// Elimina una sucursal del almacenamiento persistente.
    /// <param name="sucursal">
    /// La instancia de la sucursal que se desea eliminar. Debe estar asociada a una entidad existente en el contexto.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica.
    /// Esta tarea se completa cuando la sucursal ha sido eliminada.
    /// </returns>
    Task EliminarAsync(Sucursal sucursal);
}
