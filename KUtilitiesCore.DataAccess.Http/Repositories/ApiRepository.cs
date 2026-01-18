using KUtilitiesCore.DataAccess.UOW.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.Http.Repositories
{
    /// <summary>
    /// Implementación de IRepository que consume una API REST usando HttpClient y JSON nativo.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad (DTO).</typeparam>
    public class ApiRepositoryReadonly<T> : IRepositoryReadOnly<T> where T : class
    {

        protected readonly string _baseEndpoint;
        protected readonly HttpClient _httpClient;

        // Permite inyectar una estrategia manual para obtener el ID si la reflexión no es suficiente
        protected Func<T, object> _keySelector;

        /// <summary>
        /// Constructor del repositorio API.
        /// </summary>
        /// <param name="httpClient">Cliente HTTP configurado.</param>
        /// <param name="endpointNamingStrategy">Estrategia opcional para definir la ruta base del recurso (ej. Type => "api/v1/productos").</param>
        public ApiRepositoryReadonly(HttpClient httpClient, Func<Type, string> endpointNamingStrategy = null)
        {
            _httpClient = httpClient;

            // 1. Estrategia de Nombres Inyectable
            if (endpointNamingStrategy != null)
            {
                _baseEndpoint = endpointNamingStrategy(typeof(T));
            }
            else
            {
                // Convención por defecto: Pluralización simple en minúsculas
                _baseEndpoint = $"{typeof(T).Name.ToLower()}s";
            }
        }
        /// <summary>
        /// Configura manualmente cómo obtener la clave primaria de la entidad.
        /// Útil cuando la entidad no usa el atributo [Key] ni la convención de nombre "Id".
        /// </summary>
        public void SetKeySelector(Func<T, object> keySelector)
        {
            _keySelector = keySelector;
        }
        /// <inheritdoc/>
        public virtual int Count(ISpecification<T> spec) { return CountAsync(spec).GetAwaiter().GetResult(); }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(ISpecification<T> spec)
        {
            var url = AddQueryString($"{_baseEndpoint}/count", spec);
            return await _httpClient.GetFromJsonAsync<int>(url);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<T> GetEntities(ISpecification<T> spec)
        { return GetEntitiesAsync(spec).GetAwaiter().GetResult(); }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> GetEntitiesAsync(ISpecification<T> spec)
        {
            var url = AddQueryString(_baseEndpoint, spec);
            return await _httpClient.GetFromJsonAsync<IEnumerable<T>>(url);
        }

        /// <inheritdoc/>
        public virtual T GetFirstOrDefault(ISpecification<T> spec)
        { return GetFirstOrDefaultAsync(spec).GetAwaiter().GetResult(); }

        /// <inheritdoc/>
        public virtual async Task<T> GetFirstOrDefaultAsync(ISpecification<T> spec)
        {
            // 1. Construcción de URL base
            var url = $"{_baseEndpoint}/single";

            // 2. Manejo especial de ID (Ruta REST estándar)
            if(spec.Parameters.ContainsKey("Id"))
            {
                url = $"{_baseEndpoint}/{spec.Parameters["Id"]}";
            }

            // 3. Agregar Query String
            url = AddQueryString(url, spec);

            return await _httpClient.GetFromJsonAsync<T>(url);
        }

        /// <summary>
        /// Agrega los parámetros de la especificación como Query String usando WebUtilities.
        /// </summary>
        protected virtual string AddQueryString(string uri, ISpecification<T> spec)
        {
            if(spec.Parameters == null || spec.Parameters.Count == 0)
                return uri;

            var queryParams = new Dictionary<string, string>();

            foreach(var param in spec.Parameters)
            {
                // Ignoramos el ID si ya lo usamos en la ruta, o lo incluimos si la API lo soporta
                if(param.Key != "Id" && param.Value != null)
                {
                    queryParams.Add(param.Key, param.Value.ToString());
                }
            }

            return QueryHelpers.AddQueryString(uri, queryParams);
        }
        /// <summary>
        /// Intenta obtener el valor de la clave primaria de la entidad usando selectores manuales, atributos o convenciones.
        /// </summary>
        protected virtual object GetIdFromEntity(T entity)
        {
            // 1. Selector Manual (Prioridad Alta)
            if (_keySelector != null)
            {
                return _keySelector(entity);
            }

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 2. Atributo [Key] (Data Annotations)
            var keyProperty = properties.FirstOrDefault(p => Attribute.IsDefined(p, typeof(KeyAttribute)));

            // 3. Convención de nombre "Id" (Prioridad Baja)
            if (keyProperty == null)
            {
                keyProperty = properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
            }

            if (keyProperty != null)
            {
                return keyProperty.GetValue(entity);
            }

            return null;
        }
    }

    /// <summary>
    /// Implementación de IRepository que consume una API REST usando HttpClient y JSON nativo.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad (DTO).</typeparam>
    public class ApiRepository<T> : ApiRepositoryReadonly<T>, IRepository<T> where T : class
    {
        public ApiRepository(HttpClient httpClient) : base(httpClient)
        {
        }

        public virtual void AddEntity(T entity)
        {
            var response = _httpClient.PostAsJsonAsync(_baseEndpoint, entity).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public virtual void DeleteEntity(T entity)
        {
            // Estrategia para identificar la clave primaria
            var id = GetIdFromEntity(entity);

            if (id == null)
            {
                throw new InvalidOperationException($"No se pudo determinar el ID de la entidad '{typeof(T).Name}' para eliminarla. " +
                                                    $"Asegúrese de usar el atributo [Key], tener una propiedad 'Id' o configurar 'SetKeySelector'.");
            }

            var response = _httpClient.DeleteAsync($"{_baseEndpoint}/{id}").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public virtual void UpdateEntity(T entity)
        {
            var response = _httpClient.PutAsJsonAsync(_baseEndpoint, entity).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
    }
}