using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KUtilitiesCore.DataAccess.UOW.Interfaces;
using KUtilitiesCore.DataAccess.EfCore.Repositories;
using KUtilitiesCore.DataAccess.EfCore.UOW;

namespace KUtilitiesCore.DataAccess.EfCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registra los servicios de UnitOfWork y Repositorios de KUtilitiesCore para un DbContext específico.
        /// </summary>
        /// <typeparam name="TContext">El tipo concreto de tu DbContext (ej. AppDbContext).</typeparam>
        /// <param name="services">La colección de servicios de DI.</param>
        /// <returns>La misma colección para encadenamiento.</returns>
        public static IServiceCollection AddKUtilitiesEfCore<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            // 1. Alias para DbContext:
            // Permite que EfRepository (que pide DbContext) reciba la instancia de TContext ya registrada.
            // Esto asume que la aplicación usa un contexto principal por Scope.
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<TContext>());

            // 2. Registrar el UnitOfWork
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            // 3. Registrar el Repositorio Genérico (Open Generic)
            // Permite inyectar IRepository<User> directamente en los constructores.
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

            return services;
        }
    }
}
