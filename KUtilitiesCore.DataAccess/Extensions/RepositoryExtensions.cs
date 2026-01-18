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
    public static class RepositoryExtensions
    {
        public static T GetById<T>(this IRepository<T> repository, Expression<Func<T, bool>> idPredicate) 
            where T : class
        {
            EntityByIdSpecification<T> spec = new EntityByIdSpecification<T>(idPredicate);
            return repository.GetFirstOrDefault(spec);
        }

        public static async Task<T> GetByIdAsync<T>(this IRepository<T> repository, Expression<Func<T, bool>> idPredicate) 
            where T : class
        {
            var spec = new EntityByIdSpecification<T>(idPredicate);
            return await repository.GetFirstOrDefaultAsync(spec);
        }
    }
}
