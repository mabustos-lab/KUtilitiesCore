﻿using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace KUtilitiesCore.DataAccess.DAL
{
    /// <summary>
    /// Interfaz para operaciones básicas de ejecución SQL sobre un contexto de acceso a datos.
    /// Permite ejecutar comandos SQL, consultas escalares y operaciones asincrónicas,
    /// así como la gestión de parámetros y transacciones.
    /// </summary>
    public interface ISqlExecutorContext : IDalContext
    {
        /// <summary>
        /// Obtiene la conexión de base de datos asociada al contexto.
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        /// Crea una nueva colección de parámetros para comandos SQL.
        /// </summary>
        /// <returns>Instancia de <see cref="IDbParameterCollection"/> para agregar parámetros.</returns>
        IDbParameterCollection CreateParameterCollection();

        /// <summary>
        /// Ejecuta un comando SQL que no retorna resultados (por ejemplo, INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="sql">Cadena SQL a ejecutar.</param>
        /// <param name="parameters">Colección de parámetros para el comando.</param>
        /// <param name="commandType">Tipo de comando (Texto, Procedimiento almacenado, etc.).</param>
        /// <param name="transaction">Transacción opcional en la que ejecutar el comando.</param>
        /// <returns>Número de filas afectadas.</returns>
        int ExecuteNonQuery(string sql, IDbParameterCollection parameters = null,
                           CommandType commandType = CommandType.Text, ITransaction transaction = null);

        /// <summary>
        /// Ejecuta asincrónicamente un comando SQL que no retorna resultados.
        /// </summary>
        /// <param name="sql">Cadena SQL a ejecutar.</param>
        /// <param name="parameters">Colección de parámetros para el comando.</param>
        /// <param name="commandType">Tipo de comando.</param>
        /// <param name="transaction">Transacción opcional.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Número de filas afectadas.</returns>
        Task<int> ExecuteNonQueryAsync(string sql, IDbParameterCollection parameters = null,
                                      CommandType commandType = CommandType.Text, ITransaction transaction = null,
                                      CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta una consulta SQL y retorna el primer valor de la primera fila del resultado.
        /// </summary>
        /// <typeparam name="TResult">Tipo de dato esperado como resultado.</typeparam>
        /// <param name="sql">Cadena SQL a ejecutar.</param>
        /// <param name="parameters">Colección de parámetros para el comando.</param>
        /// <returns>Valor escalar obtenido de la consulta.</returns>
        TResult Scalar<TResult>(string sql, IDbParameterCollection parameters = null);

        /// <summary>
        /// Ejecuta asincrónicamente una consulta SQL y retorna el primer valor de la primera fila del resultado.
        /// </summary>
        /// <typeparam name="TResult">Tipo de dato esperado como resultado.</typeparam>
        /// <param name="sql">Cadena SQL a ejecutar.</param>
        /// <param name="parameters">Colección de parámetros para el comando.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Valor escalar obtenido de la consulta.</returns>
        Task<TResult> ScalarAsync<TResult>(string sql, IDbParameterCollection parameters = null,
                                          CancellationToken cancellationToken = default);
    }
}
