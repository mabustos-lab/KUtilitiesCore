using KUtilitiesCore.DataAccess.Http.Repositories;
using KUtilitiesCore.DataAccess.Http.UOW;
using KUtilitiesCore.DataAccess.UOW.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace KUtilitiesCore.DataAccess.Http.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKUtilitiesApiUow(this IServiceCollection services, string clientName, Action<IHttpClientBuilder> clientConfig)
        {
            // 1. Configuramos el HttpClient con nombre (BaseAddress, Tokens, etc.)
            var httpBuilder = services.AddHttpClient(clientName);
            clientConfig?.Invoke(httpBuilder);

            // 2. Registramos el UnitOfWork
            // Usamos una implementación que recibe el nombre del cliente
            services.AddScoped<IUnitOfWork>(provider =>
            {
                var factory = provider.GetRequiredService<IHttpClientFactory>();
                return new ApiUnitOfWork(factory, clientName);
            });

            // 3. Registramos el repo genérico (Ojo: esto puede chocar si tienes EF y API en el mismo proyecto DI container)
            // Si usas ambos, se recomienda usar resolvers delegados o contextos acotados.
            services.AddScoped(typeof(IRepository<>), typeof(ApiRepository<>));

            return services;
        }
    }
}
