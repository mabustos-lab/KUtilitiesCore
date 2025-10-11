namespace KUtilitiesCore.Dal.Helpers
{
    /// <summary>
    /// Interfaz que almacena una colección de transformaciones para un IDataReader
    /// </summary>
    public interface IDataReaderConverter
    {

        /// <summary>
        /// Indica si el convertidor puede realizar conversiones
        /// </summary>
        bool RequiredConvert { get; }

        /// <summary>
        /// Prefijos a remover de los nombres de columna antes del mapeo
        /// </summary>
        IDataReaderConverter SetColumnPrefixesToRemove(params string[] args);

        /// <summary>
        /// Si es true, ignora propiedades que no existen en el resultado del reader
        /// </summary>
        IDataReaderConverter SetIgnoreMissingColumns(bool value);

        /// <summary>
        /// Si es true, lanza excepción cuando no se pueden mapear todas las propiedades requeridas
        /// </summary>
        IDataReaderConverter SetStrictMapping(bool value);

        /// <summary>
        /// Configura el convertidor para usar DataTable como transformación por defecto
        /// </summary>
        /// <returns>La instancia actual de IDataReaderConverter.</returns>
        IDataReaderConverter WithDefaultDataTable();

        /// <summary>
        /// Agrega una transformación de conjunto de resultados al convertidor.
        /// </summary>
        /// <typeparam name="TResult">El tipo del conjunto de resultados.</typeparam>
        /// <returns>La instancia actual de IDataReaderConverter.</returns>
        IDataReaderConverter WithResult<TResult>() where TResult : class, new();

    }
}