using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.DAL
{
    public interface IDbParameterCollection<TParameter> : IReadOnlyCollection<TParameter>
        where TParameter : DbParameter
    {
        /// <summary>
        /// Da acceso a DbParameter por el nombre del parametro asignado
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        TParameter this[string parameterName] { get; }
        IDbParameterCollection<TParameter> Add<TSource, TValue>(TSource sourceObj,
        Expression<Func<TSource, TValue>> propertyExpression, 
        ParameterDirection direction = ParameterDirection.Input);
        /// <summary>
        /// Agrega un parametro dado el tipo de objeto y el valor inicial del parametro
        /// </summary>
        /// <typeparam name="TType">Tipo del valor del parametro</typeparam>
        /// <param name="parameterName">Nombre del parametro</param>
        /// <param name="value">Valor inicial del parametro</param>
        /// <param name="size">Establece el tamaño maximo en bytes de la columna</param>
        /// <param name="scale">Número de posiciones decimales hasta donde se resuelve DBParameter</param>
        /// <param name="precision">Número máximi de digitos utilizados para representar la propiedad</param>
        /// <param name="direction">Especifica del tipo de parametro dentro de una consulta</param>
        /// <returns></returns>
        IDbParameterCollection<TParameter> Add<TType>(
            string parameterName,
            TType value, int size, byte scale, byte precision,
            ParameterDirection direction = ParameterDirection.Input);
        IDbParameterCollection<TParameter> Add<TType>(
           string parameterName,
           TType value,
           ParameterDirection direction = ParameterDirection.Input);
        IDbParameterCollection<TParameter> Add(
           string parameterName,
           object value, DbType dbType, int size, byte scale, byte precision,
           ParameterDirection direction = ParameterDirection.Input);
        IDbParameterCollection<TParameter> Add<TType>(
       string parameterName,
       ParameterDirection direction = ParameterDirection.Input);
        IDbParameterCollection<TParameter> Add(
           string parameterName,
           DbType dbType,
           ParameterDirection direction = ParameterDirection.Input);
    }
}
