using KUtilitiesCore.Dal.Helpers;
using System.Data;
using System.Data.Common;

namespace KUtilitiesCore.Dal
{
    /// <summary>
    /// Define una interfaz para interactuar con el contexto de acceso a datos, proporcionando
    /// métodos para la creación de comandos, ejecución de consultas y manejo de transacciones.
    /// </summary>
    public interface IDaoContext : ISqlExecutorContext
    {

        /// <summary>
        /// Crea un adaptador de datos basado en un comando de base de datos.
        /// </summary>
        /// <param name="command">El comando de base de datos para el cual se creará el adaptador.</param>
        /// <returns>Una instancia de <see cref="DbDataAdapter"/>.</returns>
        DbDataAdapter CreateAdapter(DbCommand command);

        /// <summary>
        /// Crea un nuevo comando de base de datos con los parámetros especificados.
        /// </summary>
        /// <param name="sql">La consulta SQL a ejecutar.</param>
        /// <param name="parameters">La colección de parámetros para la consulta SQL.</param>
        /// <param name="commandType">El tipo de comando (Texto, Stored Procedure, etc.).</param>
        /// <param name="transaction">La transacción asociada al comando, si existe.</param>
        /// <returns>Un nuevo objeto <see cref="DbCommand"/>.</returns>
        DbCommand CreateCommand(string sql,
            IDaoParameterCollection parameters = null, CommandType commandType = CommandType.Text, ITransaction transaction = null);

        /// <summary>
        /// Crea un generador de comandos para construir automáticamente comandos de inserción,
        /// actualización y eliminación.
        /// </summary>
        /// <returns>Una instancia de <see cref="DbCommandBuilder"/>.</returns>
        DbCommandBuilder CreateCommandBuilder();

        /// <summary>
        /// Ejecuta una consulta y devuelve una colección de objetos mapeados.
        /// </summary>
        /// <param name="sql">
        /// La consulta SQL que debe retornar resultados del tipo del objeto a mapear.
        /// </param>
        /// <param name="translate">
        /// Convierte datos de un <see cref="IDataReader"/> en conjuntos de resultados fuertemente tipados.
        /// </param>
        /// <param name="parameters">La colección de parámetros para la consulta SQL.</param>
        /// <param name="commandType">El tipo de comando (Texto, Stored Procedure, etc.).</param>
        /// <returns>Una colección de conjuntos de resultados recuperados de un lector de datos.</returns>
        IReaderResultSet ExecuteReader(string sql, IDataReaderConverter translate,
            IDaoParameterCollection parameters = null, CommandType commandType = CommandType.StoredProcedure);

        /// <summary>
        /// Ejecuta una consulta y devuelve los resultados en un <see cref="DataSet"/>.
        /// </summary>
        /// <param name="sql">
        /// La consulta SQL que debe retornar resultados del tipo del objeto a mapear.
        /// </param>
        /// <param name="parameters">La colección de parámetros para la consulta SQL.</param>
        /// <param name="commandType">El tipo de comando (Texto, Stored Procedure, etc.).</param>
        DataSet ExecuteReader(
            string sql,
            IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.StoredProcedure);
        /// <summary>
        /// Ejecuta de manera asincrona una consulta y devuelve una colección de objetos mapeados.
        /// </summary>
        /// <param name="sql">
        /// La consulta SQL que debe retornar resultados del tipo del objeto a mapear.
        /// </param>
        /// <param name="translate">
        /// Convierte datos de un <see cref="IDataReader"/> en conjuntos de resultados fuertemente tipados.
        /// </param>
        /// <param name="parameters">La colección de parámetros para la consulta SQL.</param>
        /// <param name="commandType">El tipo de comando (Texto, Stored Procedure, etc.).</param>
        /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
        /// <returns>Una colección de conjuntos de resultados recuperados de un lector de datos.</returns>
        Task<IReaderResultSet> ExecuteReaderAsync(string sql, IDataReaderConverter translate,
            IDaoParameterCollection parameters = null, CommandType commandType = CommandType.StoredProcedure,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta una consulta SELECT y rellena un DataSet con la información.
        /// </summary>
        /// <param name="sql">
        /// La consulta SQL a ejecutar. Comando SELECT: se requieren las columnas clave.
        /// </param>
        /// <param name="srcTable">
        /// Nombre de la tabla de origen que se va a utilizar para la asignación de tabla.
        /// </param>
        /// <param name="ds">
        /// Objeto <see cref="DataSet"/> que se va a rellenar con registros y, si es necesario, con esquema.
        /// </param>
        /// <param name="parameters">La colección de parámetros para la consulta SQL.</param>
        void FillDataSet(string sql, string srcTable, DataSet ds, IDaoParameterCollection parameters = null);

        /// <summary>
        /// Actualiza los cambios realizados en el DataSet para una tabla determinada.
        /// </summary>
        /// <param name="ds">El DataSet que contiene los cambios a actualizar.</param>
        /// <param name="selectCommandText">
        /// El comando SELECT con las columnas clave, utilizado para recuperar los datos originales.
        /// </param>
        /// <param name="tableName">El nombre de la tabla en el DataSet.</param>
        /// <param name="transaction">La transacción asociada, si existe.</param>
        /// <returns>El número de filas afectadas por la actualización.</returns>
        int UpdateDataSet(DataSet ds, string selectCommandText, string tableName, ITransaction transaction = null);

    }
}