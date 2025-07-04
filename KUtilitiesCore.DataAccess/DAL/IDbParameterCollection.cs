﻿using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace KUtilitiesCore.DataAccess.DAL
{
    /// <summary>
    /// Colección de parámetros de base de datos con funcionalidad de solo lectura y manipulación avanzada.
    /// </summary>
    public interface IDbParameterCollection : IReadOnlyCollection<DbParameter>
    {
        /// <summary>
        /// Acceso por nombre a los parámetros.
        /// </summary>
        DbParameter this[string parameterName] { get; }

        /// <summary>
        /// Agrega un parámetro basado en una propiedad.
        /// </summary>
        void Add<TSource, TValue>(
            TSource sourceObj,
            Expression<Func<TSource, TValue>> propertyExpression,
            ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Agrega un parámetro con valor inicial y configuraciones.
        /// </summary>
        void Add<TType>(
            string parameterName,
            TType value,
            int size,
            byte scale,
            byte precision,
            ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Agrega un parámetro con valor inicial.
        /// </summary>
        void Add<TType>(string parameterName, TType value, ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Agrega un parámetro con configuraciones avanzadas.
        /// </summary>
        void Add(
            string parameterName,
            object value,
            DbType dbType,
            int size,
            byte scale,
            byte precision,
            ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Agrega un parámetro sin valor inicial.
        /// </summary>
        void Add<TType>(string parameterName, ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Agrega un parámetro especificando solo el tipo de datos.
        /// </summary>
        void Add(string parameterName, DbType dbType, ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Elimina todos los parámetros.
        /// </summary>
        void Clear();

        /// <summary>
        /// Verifica existencia de un parámetro por nombre.
        /// </summary>
        bool Contains(string parameterName);

        /// <summary>
        /// Elimina un parámetro por nombre.
        /// </summary>
        bool Remove(string parameterName);

        /// <summary>
        /// Elimina un parámetro específico.
        /// </summary>
        bool Remove(DbParameter param);

        /// <summary>
        /// Obtiene el valor del parametro.
        /// </summary>
        /// <typeparam name="TValue">Especifica el tipo de conversion del objeto</typeparam>
        /// <param name="parameterName">Nombre del parametro</param>
        /// <returns></returns>
        TValue GetParamValue<TValue>(string parameterName);
    }
}