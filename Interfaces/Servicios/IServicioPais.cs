using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Servicios;

public interface IServicioPais
{
    /// Obtiene una colección de todas las entidades de tipo Pais disponibles en el sistema.
    /// Los objetos se recuperan ordenados por su propiedad Nombre en orden ascendente.
    /// <returns>
    /// Una tarea que representa la operación asincrónica.
    /// El resultado contiene una colección de solo lectura de entidades de tipo Pais.
    /// </returns>
    Task<IReadOnlyCollection<Pais>> ObtenerPaisesAsync();

    /// Obtiene una colección de países activos.
    /// Esta operación se utiliza para recuperar únicamente los países que se encuentran en estado activo,
    /// lo cual es útil para casos donde se requiere un listado limitado a elementos en uso actual.
    /// <return>
    /// Una tarea que representa la operación asincrónica. El valor de retorno es una colección de solo lectura
    /// que contiene instancias de la entidad Pais correspondientes a los países activos.
    /// </return>
    Task<IReadOnlyCollection<Pais>> ObtenerPaisesActivosAsync();

    /// Obtiene un país por su identificador único.
    /// <param name="id">
    /// Identificador único del país a buscar.
    /// </param>
    /// <param name="soloLectura">
    /// Indica si la operación se debe realizar en modo de solo lectura. El valor predeterminado es true.
    /// </param>
    /// <return>
    /// Una instancia de <see cref="Pais"/> si se encuentra un país con el identificador especificado;
    /// de lo contrario, null.
    /// </return>
    Task<Pais?> ObtenerPaisPorIdAsync(int id, bool soloLectura = true);

    /// <summary>
    /// Crea un nuevo país en la base de datos después de realizar las validaciones necesarias.
    /// </summary>
    /// <param name="pais">
    /// Objeto de tipo <see cref="Pais"/> que contiene la información del país a crear.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Se lanza si ya existe un país con el mismo nombre en la base de datos.
    /// </exception>
    Task CrearAsync(Pais pais);

    /// Actualiza los datos de un país existente en el sistema.
    /// <param name="pais">
    /// Objeto de tipo <see cref="Pais"/> que contiene la información actualizada del país.
    /// Este objeto debe incluir un identificador válido y los campos actualizados.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica de actualización.
    /// Si el país especificado no existe, se lanza una excepción de tipo <see cref="InvalidOperationException"/>.
    /// </returns>
    Task ActualizarAsync(Pais pais);

    /// Asynchronously deletes a country based on its unique identifier.
    /// Before removal, it verifies the existence of the country.
    /// <param name="id">
    /// The unique identifier of the country to be deleted.
    /// </param>
    /// <returns>
    /// A Task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the country with the specified identifier does not exist.
    /// </exception>
    Task EliminarAsync(int id);
}
