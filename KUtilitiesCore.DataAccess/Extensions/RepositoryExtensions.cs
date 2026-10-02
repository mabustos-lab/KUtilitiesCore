using KUtilitiesCore.DataAccess.UOW.Interfaces;
using KUtilitiesCore.DataAccess.UOW.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.Extensions
{
    /// <summary>
    /// Extensiones de conveniencia sobre <see cref="IRepository{T}"/> para consultas puntuales por identidad,
    /// evitando repetir la construcción de especificaciones en el código de consumo.
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Obtiene la primera entidad que cumple el predicado de identidad indicado.
        /// Encapsula el patrón especificación para que el consumidor no la construya manualmente.
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad.</typeparam>
        /// <param name="repository">Repositorio sobre el que se realiza la consulta.</param>
        /// <param name="idPredicate">Predicado que identifica la entidad buscada.</param>
        /// <returns>La entidad encontrada; <c>null</c> si ninguna cumple el predicado.</returns>
        public static T GetById<T>(this IRepository<T> repository, Expression<Func<T, bool>> idPredicate)
            where T : class
        {
            EntityByIdSpecification<T> spec = new EntityByIdSpecification<T>(idPredicate);
            return repository.GetFirstOrDefault(spec);
        }

        /// <summary>
        /// Versión asíncrona de <see cref="GetById{T}"/>: obtiene la primera entidad
        /// que cumple el predicado de identidad sin bloquear el hilo llamador.
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad.</typeparam>
        /// <param name="repository">Repositorio sobre el que se realiza la consulta.</param>
        /// <param name="idPredicate">Predicado que identifica la entidad buscada.</param>
        /// <returns>La entidad encontrada; <c>null</c> si ninguna cumple el predicado.</returns>
        public static async Task<T> GetByIdAsync<T>(this IRepository<T> repository, Expression<Func<T, bool>> idPredicate)
            where T : class
        {
            var spec = new EntityByIdSpecification<T>(idPredicate);
            return await repository.GetFirstOrDefaultAsync(spec);
        }
    }
}
