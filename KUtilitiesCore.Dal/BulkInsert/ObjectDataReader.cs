using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.BulkInsert
{
    /// <summary> Convierte una secuencia IEnumerable<T> en un IDataReader. Esto permite hacer
    /// "Streaming" de datos a SqlBulkCopy sin cargar todo en memoria (DataTable). </summary>
    /// <typeparam name="T">El tipo de objeto a leer.</typeparam>
    public class ObjectDataReader<T> : IDataReader
    {

        private readonly Dictionary<string, int> _nameToIndex;
        private readonly PropertyInfo[] _properties;
        private T _current;
        private IEnumerator<T> _enumerator;
        private bool _isClosed = false;

        public ObjectDataReader(IEnumerable<T> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _enumerator = data.GetEnumerator();

            // Obtenemos propiedades de lectura.
            // NOTA: Para producción, cachear esto en una variable estática para mejorar performance.
            _properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                   .Where(p => p.CanRead) // Filtros opcionales: && !p.IsDefined(typeof(NotMappedAttribute))
                                   .ToArray();

            _nameToIndex = new Dictionary<string, int>();
            for (int i = 0; i < _properties.Length; i++)
            {
                _nameToIndex[_properties[i].Name] = i;
            }
        }

        /// <inheritdoc/>
        public int FieldCount => _properties.Length;
        /// <inheritdoc/>
        public object this[int i] => GetValue(i);
        /// <inheritdoc/>
        public object this[string name] => GetValue(GetOrdinal(name));
        /// <inheritdoc/>
        public void Close()
        {
            Dispose();
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_isClosed)
            {
                if (_enumerator != null)
                {
                    _enumerator.Dispose();
                    _enumerator = null;
                }
                _isClosed = true;
            }
        }
        /// <inheritdoc/>
        public string GetName(int i) => _properties[i].Name;
        /// <inheritdoc/>
        public int GetOrdinal(string name)
        {
            if (_nameToIndex.TryGetValue(name, out int index))
            {
                return index;
            }
            throw new IndexOutOfRangeException($"La columna '{name}' no se encontró en el objeto '{typeof(T).Name}'.");
        }
        /// <inheritdoc/>
        public object GetValue(int i)
        {
            if (_current == null) throw new InvalidOperationException("No hay datos para leer. Llame a Read() primero.");

            var value = _properties[i].GetValue(_current);
            return value ?? DBNull.Value;
        }

        // IDataReader Implementation
        /// <inheritdoc/>
        public bool Read()
        {
            if (_isClosed) throw new ObjectDisposedException(nameof(ObjectDataReader<T>));

            bool hasMore = _enumerator.MoveNext();
            if (hasMore)
            {
                _current = _enumerator.Current;
            }
            else
            {
                _current = default(T);
            }
            return hasMore;
        }

        // Métodos de IDataRecord (Implementación básica delegando a GetValue)
        // La mayoría de estos no son estrictamente necesarios para SqlBulkCopy si GetValue funciona bien,
        // pero se implementan para cumplir el contrato.
        /// <inheritdoc/>
        public bool IsDBNull(int i) => GetValue(i) == DBNull.Value;
        /// <inheritdoc/>
        public string GetString(int i) => (string)GetValue(i);
        /// <inheritdoc/>
        public int GetInt32(int i) => (int)GetValue(i);
        /// <inheritdoc/>
        public long GetInt64(int i) => (long)GetValue(i);
        /// <inheritdoc/>
        public decimal GetDecimal(int i) => (decimal)GetValue(i);
        /// <inheritdoc/>
        public double GetDouble(int i) => (double)GetValue(i);
        /// <inheritdoc/>
        public bool GetBoolean(int i) => (bool)GetValue(i);
        /// <inheritdoc/>
        public DateTime GetDateTime(int i) => (DateTime)GetValue(i);
        /// <inheritdoc/>
        public Guid GetGuid(int i) => (Guid)GetValue(i);

        // No implementados o default para tipos complejos
        /// <inheritdoc/>
        public byte GetByte(int i) => Convert.ToByte(GetValue(i));
        /// <inheritdoc/>
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => 0;
        /// <inheritdoc/>
        public char GetChar(int i) => Convert.ToChar(GetValue(i));
        /// <inheritdoc/>
        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length) => 0;
        /// <inheritdoc/>
        public string GetDataTypeName(int i) => _properties[i].PropertyType.Name;
        /// <inheritdoc/>
        public float GetFloat(int i) => (float)GetValue(i);
        /// <inheritdoc/>
        public short GetInt16(int i) => (short)GetValue(i);
        /// <inheritdoc/>
        public Type GetFieldType(int i) => _properties[i].PropertyType;

        /// <inheritdoc/>
        public IDataReader GetData(int i) => null; // No soportamos nested readers
        /// <inheritdoc/>
        public int Depth => 0;
        /// <inheritdoc/>
        public int RecordsAffected => -1;
    }
}