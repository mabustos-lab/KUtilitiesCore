using System;
using System.Linq;
using System.Linq.Expressions;

namespace KUtilitiesCore.OrderedInfo
{
    /// <summary>
    /// Contiene información de ordenamiento de una colección
    /// </summary>
    [Serializable]
    public class OrderedCollectionInfo
    {
        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="OrderedCollectionInfo"/> con propiedades
        /// ordenadas vacías
        /// </summary>
        public OrderedCollectionInfo()
            : this(new HashSet<OrderedQueryableInfo>())
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="OrderedCollectionInfo"/> con las
        /// propiedades ordenadas especificadas
        /// </summary>
        /// <param name="orderedProperties">Colección de propiedades que se utilizarán para ordenar</param>
        private OrderedCollectionInfo(HashSet<OrderedQueryableInfo> orderedProperties)
        {
            OrderedProperties = orderedProperties;
        }

        #endregion Constructors

        #region Properties

        // <summary>
        /// Colección de propiedades que ordenará la colección </summary>
        public HashSet<OrderedQueryableInfo> OrderedProperties { get; private set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Agrega una propiedad a la colección con la dirección de ordenamiento especificada
        /// </summary>
        /// <param name="info">Información de la propiedad a ordenar</param>
        /// <param name="direction">Dirección de ordenamiento</param>
        /// <exception cref="ArgumentNullException">Seleva si <paramref name="info"/> es null</exception>
        /// <exception cref="ArgumentException">
        /// Seleva si <paramref name="info.PropertyName"/> es nulo o vacío
        /// </exception>
        public void AddProperty(PNameInfo info, SortDirection direction)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (string.IsNullOrEmpty(info.PropertyName))
                throw new ArgumentException("El nombre de la propiedad no puede ser nulo ni vacío", nameof(info));

            OrderedProperties.Add(CreateOrderedQueryableInfo(info, direction));
        }

        /// <summary>
        /// Agrega una propiedad a la colección con la dirección de ordenamiento especificada
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad a ordenar</param>
        /// <param name="direction">Dirección de ordenamiento</param>
        /// <exception cref="ArgumentException">
        /// Seleva si <paramref name="propertyName"/> es nulo o vacío
        /// </exception>
        public void AddProperty(string propertyName, SortDirection direction)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("El nombre de la propiedad no puede ser nulo ni vacío", nameof(propertyName));

            OrderedProperties.Add(CreateOrderedQueryableInfo(
                new PNameInfo(propertyName, propertyName), direction));
        }

        /// <summary>
        /// Aplica el ordenamiento a una colección enumerable
        /// </summary>
        /// <typeparam name="T">Tipo de elementos en la colección</typeparam>
        /// <param name="source">Colección enumerable a ordenar</param>
        /// <returns>Una colección ordenada</returns>
        /// <exception cref="ArgumentNullException">
        /// Seleva si <paramref name="source"/> es null
        /// </exception>
        public IQueryable<T> Apply<T>(IEnumerable<T> source) where T : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var queryable = source.AsQueryable();
            bool isInitial = true;

            return OrderedProperties
            .OrderByDescending(p => p.Direction == SortDirection.Descending)
            .Aggregate(queryable,
                (currentQuery, propertyInfo) =>
                {
                    if (isInitial)
                    {
                        isInitial = false;
                        if (propertyInfo.Direction == SortDirection.Ascending)
                            return currentQuery.OrderBy(propertyInfo.Property.PropertyName);
                        return currentQuery.OrderByDescending(propertyInfo.Property.PropertyName);
                    }

                    if (propertyInfo.Direction == SortDirection.Ascending)
                        return currentQuery.ThenBy(propertyInfo.Property.PropertyName);
                    return currentQuery.ThenByDescending(propertyInfo.Property.PropertyName);
                });
        }

        /// <summary>
        /// Crea una nueva instancia de <see cref="OrderedQueryableInfo"/> con la información proporcionada
        /// </summary>
        /// <param name="property">Información de la propiedad</param>
        /// <param name="direction">Dirección de ordenamiento</param>
        /// <returns>Una nueva instancia de <see cref="OrderedQueryableInfo"/></returns>
        private OrderedQueryableInfo CreateOrderedQueryableInfo(PNameInfo property, SortDirection direction)
        {
            return new OrderedQueryableInfo
            {
                Property = property,
                Direction = direction
            };
        }

        #endregion Methods
    }
}