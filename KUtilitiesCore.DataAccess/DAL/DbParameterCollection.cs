using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace KUtilitiesCore.DataAccess.DAL
{
    /// <summary>
    /// Implementación decolección de parámetros de base de datos.
    /// </summary>
    class DbParameterCollection : IDbParameterCollection
    {
        private readonly Func<DbParameter> _parameterFactory;
        private readonly Dictionary<string, DbParameter> _parameters;
        private static readonly Dictionary<Type, DbType> _typeMappings;

        static DbParameterCollection()
        {
            _typeMappings = new Dictionary<Type, DbType>
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
        }

        public DbParameterCollection(Func<DbParameter> parameterFactory)
        {
            _parameterFactory = parameterFactory ?? throw new ArgumentNullException(nameof(parameterFactory));
            _parameters = [];
        }

        public int Count => _parameters.Count;
        public DbParameter this[string parameterName] => _parameters[parameterName];

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

        public void Clear() => _parameters.Clear();

        public bool Contains(string parameterName) => _parameters.ContainsKey(parameterName);

        public bool Remove(string parameterName) => _parameters.Remove(parameterName);

        public bool Remove(DbParameter param) => Remove(param.ParameterName);

        private void CreateAndAdd(Type type, Action<DbParameter> configure)
        {
            if (!_typeMappings.TryGetValue(type, out var dbType))
                throw new NotSupportedException($"Tipo '{type.Name}' no soportado.");

            var parameter = _parameterFactory();
            parameter.DbType = dbType;
            AddCore(parameter, configure);
        }

        private void AddCore(DbParameter parameter, Action<DbParameter> configure)
        {
            configure(parameter);

            if (parameter.DbType == DbType.String && parameter.Size == 0)
                parameter.Size = 500;

            _parameters[parameter.ParameterName] = parameter;
        }

        private static void ValidatePropertyExpression<TSource, TValue>(Expression<Func<TSource, TValue>> expression)
        {
            if (expression.Body is not MemberExpression member)
                throw new ArgumentException("La expresión debe referirse a una propiedad.");

            if (member.Member is not PropertyInfo prop)
                throw new ArgumentException("La expresión debe referirse a una propiedad.");
        }

        private static string GetPropertyName<T, TValue>(Expression<Func<T, TValue>> expression)
        {
            if (expression.Body is MemberExpression member)
                return member.Member.Name;

            throw new ArgumentException("La expresión debe referirse a una propiedad.");
        }

        /// <summary>
        /// Obtiene el valor de una propiedad específica de un objeto fuente utilizando una expresión lambda.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto fuente.</typeparam>
        /// <typeparam name="TValue">El tipo del valor de la propiedad a obtener.</typeparam>
        /// <param name="source">El objeto fuente del cual se obtendrá el valor de la propiedad.</param>
        /// <param name="expression">Una expresión lambda que especifica la propiedad cuyo valor se desea obtener.</param>
        /// <returns>El valor de la propiedad especificada.</returns>
        /// <exception cref="ArgumentException">Se lanza si la expresión no se refiere a una propiedad válida.</exception>
        private TValue GetValue<T, TValue>(T source, Expression<Func<T, TValue>> expression)
        {
            if (expression.Body is MemberExpression member)
            {
                var property = member.Member as PropertyInfo;
                return (TValue)property?.GetValue(source, null);
            }

            throw new ArgumentException("La expresión debe referirse a una propiedad.");
        }

        public IEnumerator<DbParameter> GetEnumerator()
        {
            return _parameters.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TValue GetParamValue<TValue>(string parameterName)
        {
           if(string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException(nameof(parameterName));
            if(Contains(parameterName))
                return (TValue)_parameters[parameterName].Value;
            return default;
        }
    }
}