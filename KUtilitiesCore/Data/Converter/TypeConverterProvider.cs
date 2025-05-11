using System;
using System.Collections.Generic;
using KUtilitiesCore.Data.Converter.Exceptions;
using KUtilitiesCore.Data.Converter.Types;

namespace KUtilitiesCore.Data.Converter
{
    internal class TypeConverterProvider : ITypeConverterProvider
    {

        private readonly IDictionary<Type, ITypeConverter> typeConverters;

        public TypeConverterProvider()
        {
            typeConverters = new Dictionary<Type, ITypeConverter>();

            // Single Converters:
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

            // Collection Converters:
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

        public ConverterByType GetCustomConverter { get; set; }

        public ITypeConverterProvider Add<TTargetType>(ITypeConverter<TTargetType> typeConverter)
        {
            if (typeConverters.ContainsKey(typeConverter.TargetType))
            {
                throw new TypeConverterNotRegisteredException($"Duplicate TypeConverter registration for Type {typeConverter.TargetType}");
            }

            typeConverters[typeConverter.TargetType] = typeConverter;

            return this;
        }

        public ITypeConverterProvider Add<TTargetType>(IArrayTypeConverter<TTargetType> typeConverter)
        {
            if (typeConverters.ContainsKey(typeConverter.TargetType))
            {
                throw new TypeConverterAlreadyRegisteredException($"Duplicate TypeConverter registration for Type {typeConverter.TargetType}");
            }

            typeConverters[typeConverter.TargetType] = typeConverter;

            return this;
        }

        public bool ContainsConverterType(Type targetType)
            => typeConverters.ContainsKey(targetType);

        public ITypeConverter Resolve(Type targetType)
        {
            if (!typeConverters.ContainsKey(targetType) && GetCustomConverter != null)
            {
                ITypeConverter vtype= GetCustomConverter(targetType);
                if (vtype!=null) typeConverters[targetType] = vtype;
            }
            if (!typeConverters.TryGetValue(targetType, out ITypeConverter typeConverter))
            {
                
                throw new TypeConverterNotRegisteredException($"No TypeConverter registered for Type {targetType}, you can register your own converter provider.");
            }

            return typeConverter;
        }

        public ITypeConverter<TTargetType> Resolve<TTargetType>()
        {
            Type targetType = typeof(TTargetType);

            return Resolve(targetType) as ITypeConverter<TTargetType>;
        }

        public IArrayTypeConverter<TTargetType> ResolveCollection<TTargetType>()
        {
            Type targetType = typeof(TTargetType);

            return Resolve(targetType) as IArrayTypeConverter<TTargetType>;
        }

        ITypeConverterProvider ITypeConverterProvider.AddEnum<TTargetType>()
        {
            return Add(new EnumConverter<TTargetType>());
        }

    }
}