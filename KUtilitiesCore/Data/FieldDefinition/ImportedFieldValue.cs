using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.FieldDefinition
{
    public class ImportedFieldValue<TValue>
        where TValue : struct
    {
        /// <summary>
        /// La definición del campo al que pertenece este valor.
        /// </summary>
        public FieldDefinition Definition { get; }
        /// <summary>
        /// El valor importado (puede ser de un tipo diferente inicialmente, ej: string).
        /// La conversión y validación se harían en pasos posteriores.
        /// </summary>
        public object? RawValue { get; }

        /// <summary>
        /// El valor convertido al tipo esperado (TValue).
        /// Se establecería después de un intento de conversión.
        /// </summary>
        public virtual bool TryGetValue(out TValue? value)
        {
            value = default;
            string strValue = RawValue?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(strValue) && Definition.AllowNull)
                return true;

            if(Definition.Converter is null)
                throw new InvalidOperationException("No se encuentra asociado el convertidor de tipo al campo.");

            value = (TValue?)Definition.Converter.TryConvert(strValue);
            return value is not null;
        }

        public ImportedFieldValue(FieldDefinition definition, object rawValue)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            RawValue = rawValue;
        }
    }
}
