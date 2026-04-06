using ElectronicaVallarta.Dominio.Entidades;

namespace ElectronicaVallarta.Interfaces.Repositorios;

public interface IRepositorioPais
{
    /// Obtiene una colección de todas las entidades de tipo Pais disponibles en el sistema.
    /// Los objetos se recuperan ordenados por su propiedad Nombre en orden ascendente.
    /// <returns>
    /// Una tarea que representa la operación asincrónica.
    /// El resultado contiene una colección de solo lectura con las entidades de tipo Pais.
    /// </returns>
    Task<IReadOnlyCollection<Pais>> ObtenerTodosAsync();

    /// Obtiene una lista de entidades de tipo Pais que están marcadas como activas.
    /// Este método retorna solo los países cuyo atributo "EstaActivo" es verdadero.
    /// La lista se encuentra ordenada alfabéticamente por el nombre.
    /// <returns>
    /// Una colección de solo lectura de entidades de tipo Pais que están activas.
    /// Si no hay países activos, se devuelve una colección vacía.
    /// </returns>
    Task<IReadOnlyCollection<Pais>> ObtenerActivosAsync();

    /// <summary>
    /// Recupera un registro de tipo <see cref="Pais"/> utilizando su identificador único.
    /// </summary>
    /// <param name="id">
    /// El identificador único del país que se desea obtener.
    /// </param>
    /// <param name="soloLectura">
    /// Indica si la entidad debe ser recuperada en modo de solo lectura.
    /// Valor predeterminado es <c>true</c>.
    /// </param>
    /// <returns>
    /// Una instancia de <see cref="Pais"/> si se encuentra un país con el identificador especificado;
    /// de lo contrario, <c>null</c>.
    /// </returns>
    Task<Pais?> ObtenerPorIdAsync(int id, bool soloLectura = true);
    Task<bool> ExisteActivoAsync(int id);

    /// Verifica si existe un país con un nombre duplicado en el almacenamiento.
    /// <param name="nombre">
    /// El nombre del país que se desea verificar.
    /// </param>
    /// <param name="idExcluir">
    /// El identificador del país que debe excluirse de la comparación, si aplica.
    /// Este parámetro es opcional y puede ser null.
    /// </param>
    /// <return>
    /// Un valor booleano que indica si existe al menos un país con el mismo nombre
    /// en el almacenamiento, excluyendo el país especificado en el parámetro idExcluir.
    /// </return>
    Task<bool> ExisteNombreDuplicadoAsync(string nombre, int? idExcluir = null);

    /// Agrega un nuevo país al repositorio.
    /// <param name="pais">Instancia de la entidad <c>Pais</c> que será agregada al repositorio.</param>
    Task AgregarAsync(Pais pais);

    /// <summary>
    /// Actualiza los datos de una entidad de tipo <see cref="Pais"/> en el repositorio.
    /// </summary>
    /// <param name="pais">
    /// La entidad <see cref="Pais"/> que contiene los datos actualizados.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica.
    /// </returns>
    Task ActualizarAsync(Pais pais);

    /// <summary>
    /// Elimina de forma asincrónica una entidad de tipo <see cref="Pais"/> del repositorio.
    /// </summary>
    /// <param name="pais">La entidad <see cref="Pais"/> que será eliminada.</param>
    /// <returns>Una tarea que representa la operación asincrónica de eliminación.</returns>
    Task EliminarAsync(Pais pais);
}
