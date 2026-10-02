using System.Collections;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace KUtilitiesCore.Dal
{
    /// <summary>
    /// Implementación decolección de parámetros de base de datos.
    /// </summary>
    public sealed class DaoParameterCollection : IDaoParameterCollection
    {
        #region Fields

        private static readonly Dictionary<Type, DbType> _typeMappings = new()
        {
           { typeof(byte), DbType.Byte },
           { typeof(sbyte), DbType.SByte },
           { typeof(short), DbType.Int16 },
           { typeof(ushort), DbType.UInt16 },
           { typeof(int), DbType.Int32 },
           { typeof(uint), DbType.UInt32 },
           { typeof(long), DbType.Int64 },
           { typeof(ulong), DbType.UInt64 },
           { typeof(float), DbType.Single },
           { typeof(double), DbType.Double },
           { typeof(decimal), DbType.Decimal },
           { typeof(bool), DbType.Boolean },
           { typeof(string), DbType.String },
           { typeof(char), DbType.StringFixedLength },
           { typeof(Guid), DbType.Guid },
           { typeof(DateTime), DbType.DateTime },
           { typeof(DateTimeOffset), DbType.DateTimeOffset },
           { typeof(byte[]), DbType.Binary },
           { typeof(byte?), DbType.Byte },
           { typeof(sbyte?), DbType.SByte },
           { typeof(short?), DbType.Int16 },
           { typeof(ushort?), DbType.UInt16 },
           { typeof(int?), DbType.Int32 },
           { typeof(uint?), DbType.UInt32 },
           { typeof(long?), DbType.Int64 },
           { typeof(ulong?), DbType.UInt64 },
           { typeof(float?), DbType.Single },
           { typeof(double?), DbType.Double },
           { typeof(decimal?), DbType.Decimal },
           { typeof(bool?), DbType.Boolean },
           { typeof(char?), DbType.StringFixedLength },
           { typeof(Guid?), DbType.Guid },
           { typeof(DateTime?), DbType.DateTime },
           { typeof(DateTimeOffset?), DbType.DateTimeOffset }
        };

        private readonly Func<DbParameter> _parameterFactory;
        private readonly Dictionary<string, DbParameter> _parameters;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa la colección con la fábrica de parámetros subyacente del proveedor de datos.
        /// </summary>
        /// <param name="parameterFactory">
        /// Fábrica que crea las instancias de <see cref="DbParameter"/> nativas del proveedor;
        /// delegar en el proveedor evita acoplar esta colección a un driver concreto.
        /// </param>
        /// <exception cref="ArgumentNullException">Se lanza si <paramref name="parameterFactory"/> es nulo.</exception>
        public DaoParameterCollection(Func<DbParameter> parameterFactory)
        {
            _parameterFactory = parameterFactory ?? throw new ArgumentNullException(nameof(parameterFactory));
            _parameters = [];
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Obtiene el número de parámetros registrados en la colección.
        /// </summary>
        public int Count => _parameters.Count;

        #endregion Properties

        #region Indexers

        /// <summary>
        /// Obtiene el parámetro registrado bajo el nombre indicado.
        /// </summary>
        /// <param name="parameterName">Nombre del parámetro a recuperar.</param>
        public DbParameter this[string parameterName] => _parameters[parameterName];

        #endregion Indexers

        #region Methods

        /// <summary>
        /// Agrega un parámetro cuyo nombre y valor se extraen de una propiedad del objeto fuente,
        /// evitando cadenas mágicas y permitiendo validación en tiempo de compilación.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto fuente.</typeparam>
        /// <typeparam name="TValue">Tipo del valor de la propiedad.</typeparam>
        /// <param name="sourceObj">Objeto del cual se leerá la propiedad.</param>
        /// <param name="propertyExpression">Expresión que identifica la propiedad de origen.</param>
        /// <param name="direction">Dirección del parámetro en el comando SQL.</param>
        /// <exception cref="ArgumentException">
        /// Se lanza si la expresión no se refiere a una propiedad válida.
        /// </exception>
        public void Add<TSource, TValue>(TSource sourceObj,
            Expression<Func<TSource, TValue>> propertyExpression,
            ParameterDirection direction = ParameterDirection.Input)
        {
            ValidatePropertyExpression(propertyExpression);

            CreateAndAdd(
                typeof(TValue),
                p =>
                {
                    p.ParameterName = GetPropertyName(propertyExpression);
                    p.Value = GetValue(sourceObj, propertyExpression);
                    p.Direction = direction;
                });
        }

        /// <summary>
        /// Agrega un parámetro tipado con tamaño, escala y precisión explícitos, útil para
        /// columnas numéricas o de longitud fija donde el proveedor no puede inferirlos.
        /// </summary>
        /// <typeparam name="TType">Tipo CLR del valor del parámetro.</typeparam>
        /// <param name="parameterName">Nombre del parámetro.</param>
        /// <param name="value">Valor inicial del parámetro.</param>
        /// <param name="size">Tamaño máximo del parámetro.</param>
        /// <param name="scale">Escala decimal del parámetro.</param>
        /// <param name="precision">Precisión decimal del parámetro.</param>
        /// <param name="direction">Dirección del parámetro en el comando SQL.</param>
        /// <exception cref="NotSupportedException">
        /// Se lanza si el tipo <typeparamref name="TType"/> no tiene un mapeo a <see cref="DbType"/>.
        /// </exception>
        public void Add<TType>(string parameterName, TType value,
            int size, byte scale, byte precision,
            ParameterDirection direction = ParameterDirection.Input)
        {
            CreateAndAdd(
                typeof(TType),
                p =>
                {
                    p.ParameterName = parameterName;
                    p.Value = value;
                    p.Direction = direction;
                    p.Size = size;
                    p.Precision = precision;
                    p.Scale = scale;
                });
        }

        /// <summary>
        /// Agrega un parámetro tipado con valor inicial, infiriendo el <see cref="DbType"/>
        /// a partir del tipo CLR especificado.
        /// </summary>
        /// <typeparam name="TType">Tipo CLR del valor del parámetro.</typeparam>
        /// <param name="parameterName">Nombre del parámetro.</param>
        /// <param name="value">Valor inicial del parámetro.</param>
        /// <param name="direction">Dirección del parámetro en el comando SQL.</param>
        /// <exception cref="NotSupportedException">
        /// Se lanza si el tipo <typeparamref name="TType"/> no tiene un mapeo a <see cref="DbType"/>.
        /// </exception>
        public void Add<TType>(string parameterName, TType value,
            ParameterDirection direction = ParameterDirection.Input)
        {
            CreateAndAdd(
                typeof(TType),
                p =>
                {
                    p.ParameterName = parameterName;
                    p.Value = value;
                    p.Direction = direction;
                });
        }

        /// <summary>
        /// Agrega un parámetro sin genéricos, especificando el <see cref="DbType"/> y los
        /// metadatos de tamaño/escala/precisión manualmente.
        /// </summary>
        /// <param name="parameterName">Nombre del parámetro.</param>
        /// <param name="value">Valor inicial del parámetro.</param>
        /// <param name="dbType">Tipo de datos nativo del parámetro.</param>
        /// <param name="size">Tamaño máximo del parámetro.</param>
        /// <param name="scale">Escala decimal del parámetro.</param>
        /// <param name="precision">Precisión decimal del parámetro.</param>
        /// <param name="direction">Dirección del parámetro en el comando SQL.</param>
        public void Add(string parameterName, object value,
            DbType dbType, int size, byte scale, byte precision,
            ParameterDirection direction = ParameterDirection.Input)
        {
            AddCore(
                _parameterFactory(),
                p =>
                {
                    p.ParameterName = parameterName;
                    p.Value = value;
                    p.Direction = direction;
                    p.Size = size;
                    p.Precision = precision;
                    p.Scale = scale;
                    p.DbType = dbType;
                });
        }

        /// <summary>
        /// Agrega un parámetro de salida o de entrada/salida sin valor inicial,
        /// tipado genéricamente para recuperar luego su valor con <see cref="GetParamValue{TValue}(string)"/>.
        /// </summary>
        /// <typeparam name="TType">Tipo CLR esperado del valor de retorno del parámetro.</typeparam>
        /// <param name="parameterName">Nombre del parámetro.</param>
        /// <param name="direction">Dirección del parámetro en el comando SQL.</param>
        /// <exception cref="NotSupportedException">
        /// Se lanza si el tipo <typeparamref name="TType"/> no tiene un mapeo a <see cref="DbType"/>.
        /// </exception>
        public void Add<TType>(string parameterName,
            ParameterDirection direction = ParameterDirection.Input)
        {
            CreateAndAdd(
                typeof(TType),
                p =>
                {
                    p.ParameterName = parameterName;
                    p.Direction = direction;
                });
        }

        /// <summary>
        /// Agrega un parámetro de salida o de entrada/salida sin valor inicial,
        /// especificando el <see cref="DbType"/> nativo.
        /// </summary>
        /// <param name="parameterName">Nombre del parámetro.</param>
        /// <param name="dbType">Tipo de datos nativo del parámetro.</param>
        /// <param name="direction">Dirección del parámetro en el comando SQL.</param>
        public void Add(string parameterName, DbType dbType,
            ParameterDirection direction = ParameterDirection.Input)
        {
            AddCore(
                _parameterFactory(),
                p =>
                {
                    p.ParameterName = parameterName;
                    p.Direction = direction;
                    p.DbType = dbType;
                });
        }

        /// <summary>Elimina todos los parámetros de la colección.</summary>
        public void Clear() => _parameters.Clear();

        /// <summary>Determina si existe un parámetro con el nombre indicado.</summary>
        /// <param name="parameterName">Nombre del parámetro a buscar.</param>
        public bool Contains(string parameterName) => _parameters.ContainsKey(parameterName);

        /// <summary>
        /// Devuelve un enumerador sobre los parámetros registrados.
        /// </summary>
        public IEnumerator<DbParameter> GetEnumerator()
        {
            return _parameters.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Obtiene el valor de un parámetro de salida convertido al tipo solicitado.
        /// Si el parámetro no existe o el proveedor devolvió <see langword="null"/>,
        /// se devuelve el valor por defecto del tipo (incluida una referencia nula
        /// para tipos de referencia), por lo que el llamador debe validar el resultado
        /// según su contexto.
        /// </summary>
        /// <typeparam name="TValue">Tipo al que se convierte el valor del parámetro.</typeparam>
        /// <param name="parameterName">Nombre del parámetro cuyo valor se obtiene.</param>
        /// <exception cref="ArgumentNullException">Se lanza si <paramref name="parameterName"/> es nulo o vacío.</exception>
        public TValue? GetParamValue<TValue>(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException(nameof(parameterName));
            if (Contains(parameterName))
                return (TValue?)_parameters[parameterName].Value;
            return default;
        }

        /// <summary>Elimina de la colección el parámetro con el nombre indicado.</summary>
        /// <param name="parameterName">Nombre del parámetro a eliminar.</param>
        public bool Remove(string parameterName) => _parameters.Remove(parameterName);

        /// <summary>Elimina de la colección el parámetro indicado.</summary>
        /// <param name="param">Parámetro a eliminar; se identifica por su nombre.</param>
        public bool Remove(DbParameter param) => Remove(param.ParameterName);

        private static string GetPropertyName<T, TValue>(Expression<Func<T, TValue>> expression)
        {
            if (expression.Body is MemberExpression member)
                return member.Member.Name;

            throw new ArgumentException("La expresión debe referirse a una propiedad.");
        }

        private static void ValidatePropertyExpression<TSource, TValue>(Expression<Func<TSource, TValue>> expression)
        {
            if (expression.Body is not MemberExpression member)
                throw new ArgumentException("La expresión debe referirse a una propiedad.");

            if (member.Member is not PropertyInfo prop)
                throw new ArgumentException("La expresión debe referirse a una propiedad.");
        }

        private void AddCore(DbParameter parameter, Action<DbParameter> configure)
        {
            configure(parameter);

            if (parameter.DbType == DbType.String && parameter.Size == 0)
                parameter.Size = 500;

            _parameters[parameter.ParameterName] = parameter;
        }

        private void CreateAndAdd(Type type, Action<DbParameter> configure)
        {
            if (!_typeMappings.TryGetValue(type, out var dbType))
                throw new NotSupportedException($"Tipo '{type.Name}' no soportado.");

            var parameter = _parameterFactory();
            parameter.DbType = dbType;
            AddCore(parameter, configure);
        }

        /// <summary>
        /// Obtiene el valor de una propiedad específica de un objeto fuente utilizando una
        /// expresión lambda.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto fuente.</typeparam>
        /// <typeparam name="TValue">El tipo del valor de la propiedad a obtener.</typeparam>
        /// <param name="source">El objeto fuente del cual se obtendrá el valor de la propiedad.</param>
        /// <param name="expression">
        /// Una expresión lambda que especifica la propiedad cuyo valor se desea obtener.
        /// </param>
        /// <returns>El valor de la propiedad especificada; puede ser null si la propiedad vale null.</returns>
        /// <exception cref="ArgumentException">
        /// Se lanza si la expresión no se refiere a una propiedad válida.
        /// </exception>
        private TValue? GetValue<T, TValue>(T source, Expression<Func<T, TValue>> expression)
        {
            if (expression.Body is MemberExpression { Member: PropertyInfo property })
                return (TValue?)property.GetValue(source);

            throw new ArgumentException("La expresión debe referirse a una propiedad.");
        }

        #endregion Methods
    }
}