using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KUtilitiesCore.Data.Converter.Exceptions;
using KUtilitiesCore.Data.Converter.Types;

namespace KUtilitiesCore.Data.Converter
{
    /// <summary>
    /// Proveedor interno de convertidores de tipo. Permite registrar y resolver convertidores para tipos individuales y colecciones.
    /// </summary>
    internal class TypeConverterProvider : ITypeConverterProvider
    {
        /// <summary>
        /// Diccionario que almacena los convertidores registrados, indexados por su tipo de destino.
        /// </summary>
        private readonly IDictionary<Type, ITypeConverter> typeConverters;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="TypeConverterProvider"/> y registra los convertidores estándar.
        /// </summary>
        public TypeConverterProvider()
        {
            typeConverters = new Dictionary<Type, ITypeConverter>();

            // Registro de convertidores individuales y de colecciones.
            Add(new BoolConverter());
            Add(new ByteConverter());
            Add(new DateTimeConverter());
            Add(new DecimalConverter());
            Add(new DoubleConverter());
            Add(new GuidConverter());
            Add(new Int16Converter());
            Add(new Int32Converter());
            Add(new Int64Converter());
            Add(new NullableBoolConverter());
            Add(new NullableByteConverter());
            Add(new NullableDateTimeConverter());
            Add(new NullableDecimalConverter());
            Add(new NullableDoubleConverter());
            Add(new NullableGuidConverter());
            Add(new NullableInt16Converter());
            Add(new NullableInt32Converter());
            Add(new NullableInt64Converter());
            Add(new NullableSByteConverter());
            Add(new NullableSingleConverter());
            Add(new NullableTimeSpanConverter());
            Add(new NullableUInt16Converter());
            Add(new NullableUInt32Converter());
            Add(new NullableUInt64Converter());
            Add(new SByteConverter());
            Add(new SingleConverter());
            Add(new StringConverter());
            Add(new TimeSpanConverter());
            Add(new UInt16Converter());
            Add(new UInt32Converter());
            Add(new UInt64Converter());

            Add(new ArrayConverter<bool>(new BoolConverter()));
            Add(new ArrayConverter<byte>(new ByteConverter()));
            Add(new ArrayConverter<DateTime>(new DateTimeConverter()));
            Add(new ArrayConverter<decimal>(new DecimalConverter()));
            Add(new ArrayConverter<double>(new DoubleConverter()));
            Add(new ArrayConverter<Guid>(new GuidConverter()));
            Add(new ArrayConverter<short>(new Int16Converter()));
            Add(new ArrayConverter<int>(new Int32Converter()));
            Add(new ArrayConverter<long>(new Int64Converter()));
            Add(new ArrayConverter<bool?>(new NullableBoolConverter()));
            Add(new ArrayConverter<byte?>(new NullableByteConverter()));
            Add(new ArrayConverter<DateTime?>(new NullableDateTimeConverter()));
            Add(new ArrayConverter<decimal?>(new NullableDecimalConverter()));
            Add(new ArrayConverter<double?>(new NullableDoubleConverter()));
            Add(new ArrayConverter<Guid?>(new NullableGuidConverter()));
            Add(new ArrayConverter<short?>(new NullableInt16Converter()));
            Add(new ArrayConverter<int?>(new NullableInt32Converter()));
            Add(new ArrayConverter<long?>(new NullableInt64Converter()));
            Add(new ArrayConverter<sbyte?>(new NullableSByteConverter()));
            Add(new ArrayConverter<float?>(new NullableSingleConverter()));
            Add(new ArrayConverter<TimeSpan?>(new NullableTimeSpanConverter()));
            Add(new ArrayConverter<ushort?>(new NullableUInt16Converter()));
            Add(new ArrayConverter<uint?>(new NullableUInt32Converter()));
            Add(new ArrayConverter<ulong?>(new NullableUInt64Converter()));
            Add(new ArrayConverter<sbyte>(new SByteConverter()));
            Add(new ArrayConverter<float>(new SingleConverter()));
            Add(new ArrayConverter<string>(new StringConverter()));
            Add(new ArrayConverter<TimeSpan>(new TimeSpanConverter()));
            Add(new ArrayConverter<ushort>(new UInt16Converter()));
            Add(new ArrayConverter<uint>(new UInt32Converter()));
            Add(new ArrayConverter<ulong>(new UInt64Converter()));
        }

        /// <summary>
        /// Delegado opcional para obtener un convertidor personalizado basado en el tipo.
        /// </summary>
        public ConverterByType? GetCustomConverter { get; set; }

        /// <summary>
        /// Agrega un convertidor de tipo específico al proveedor.
        /// </summary>
        /// <typeparam name="TTargetType">Tipo de destino que manejará el convertidor.</typeparam>
        /// <param name="typeConverter">Instancia del convertidor a registrar.</param>
        /// <returns>La instancia actual para encadenar llamadas.</returns>
        /// <exception cref="TypeConverterNotRegisteredException">Se lanza si ya existe un convertidor registrado para el tipo.</exception>
        public ITypeConverterProvider Add<TTargetType>(ITypeConverter<TTargetType> typeConverter)
        {
            if (typeConverters.ContainsKey(typeConverter.TargetType))
            {
                throw new TypeConverterNotRegisteredException($"Duplicate TypeConverter registration for Type {typeConverter.TargetType}");
            }

            typeConverters[typeConverter.TargetType] = typeConverter;

            return this;
        }

        /// <summary>
        /// Agrega un convertidor de tipo para colecciones al proveedor.
        /// </summary>
        /// <typeparam name="TTargetType">Tipo de destino que manejará el convertidor de colección.</typeparam>
        /// <param name="typeConverter">Instancia del convertidor de colección a registrar.</param>
        /// <returns>La instancia actual para encadenar llamadas.</returns>
        /// <exception cref="TypeConverterAlreadyRegisteredException">Se lanza si ya existe un convertidor registrado para el tipo.</exception>
        public ITypeConverterProvider Add<TTargetType>(IArrayTypeConverter<TTargetType> typeConverter)
        {
            if (typeConverters.ContainsKey(typeConverter.TargetType))
            {
                throw new TypeConverterAlreadyRegisteredException($"TypeConverter registro duplicado para el tipo {typeConverter.TargetType}");
            }

            typeConverters[typeConverter.TargetType] = typeConverter;

            return this;
        }

        /// <summary>
        /// Verifica si existe un convertidor registrado para el tipo de destino especificado.
        /// </summary>
        /// <param name="targetType">Tipo de destino a verificar.</param>
        /// <returns>True si existe un convertidor registrado; de lo contrario, false.</returns>
        public bool ContainsConverterType(Type targetType)
            => typeConverters.ContainsKey(targetType);

        /// <summary>
        /// Resuelve un convertidor para el tipo de destino especificado.
        /// Si no existe y hay un delegado personalizado, lo intenta registrar dinámicamente.
        /// </summary>
        /// <param name="targetType">Tipo de destino a resolver.</param>
        /// <returns>Instancia de <see cref="ITypeConverter"/> correspondiente.</returns>
        /// <exception cref="TypeConverterNotRegisteredException">Si no se encuentra un convertidor registrado.</exception>
        public ITypeConverter Resolve(Type targetType)
        {
            if (!typeConverters.ContainsKey(targetType) && GetCustomConverter != null)
            {
                ITypeConverter vtype= GetCustomConverter(targetType);
                if (vtype!=null) typeConverters[targetType] = vtype;
            }
            if (!typeConverters.TryGetValue(targetType, out ITypeConverter? typeConverter))
            {
                throw new TypeConverterNotRegisteredException($"No se encuentra registrado TypeConverter para el tipo {targetType}, ustd puede registrar su propio TypeConverter.");
            }

            return typeConverter;
        }

        /// <summary>
        /// Resuelve un convertidor fuertemente tipado para el tipo de destino especificado.
        /// </summary>
        /// <typeparam name="TTargetType">Tipo de destino a resolver.</typeparam>
        /// <returns>Instancia de <see cref="ITypeConverter{TTargetType}"/> correspondiente.</returns>
        /// <exception cref="TypeConverterNotRegisteredException">Si no se encuentra un convertidor registrado.</exception>
        public ITypeConverter<TTargetType> Resolve<TTargetType>()
        {
            Type targetType = typeof(TTargetType);
            var converter = Resolve(targetType) as ITypeConverter<TTargetType>;
            if (converter is null)
            {
                throw new TypeConverterNotRegisteredException($"No se encuentra registrado TypeConverter para el tipo {targetType}, ustd puede registrar su propio TypeConverter.");
            }
            return converter;
        }

        /// <summary>
        /// Resuelve un convertidor de colecciones fuertemente tipado para el tipo de destino especificado.
        /// </summary>
        /// <typeparam name="TTargetType">Tipo de destino a resolver.</typeparam>
        /// <returns>Instancia de <see cref="IArrayTypeConverter{TTargetType}"/> correspondiente.</returns>
        /// <exception cref="TypeConverterNotRegisteredException">Si no se encuentra un convertidor registrado.</exception>
        public IArrayTypeConverter<TTargetType> ResolveCollection<TTargetType>()
        {
            Type targetType = typeof(TTargetType);
            var converter = Resolve(targetType) as IArrayTypeConverter<TTargetType>;
            if (converter is null)
            {
                throw new TypeConverterNotRegisteredException($"No se encuentra registrado TypeConverter para el tipo {targetType}, ustd puede registrar su propio TypeConverter.");
            }
            return converter;
        }

        /// <summary>
        /// Agrega un convertidor para enumeraciones al proveedor.
        /// </summary>
        /// <typeparam name="TTargetType">Tipo de enumeración a registrar.</typeparam>
        /// <returns>La instancia actual para encadenar llamadas.</returns>
        ITypeConverterProvider ITypeConverterProvider.AddEnum<TTargetType>()
        {
            return Add(new EnumConverter<TTargetType>());
        }

    }
}