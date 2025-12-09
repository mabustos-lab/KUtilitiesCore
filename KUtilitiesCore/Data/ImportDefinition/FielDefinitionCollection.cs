using KUtilitiesCore.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.ImportDefinition
{
    /// <summary>
    /// Contenedo de Definiciones de campo
    /// </summary>
    public class FielDefinitionCollection : IReadOnlyList<FieldDefinition>
    {
        #region Fields

        private readonly List<FieldDefinition> _fields = [];

        #endregion Fields

        #region Properties

        public int Count => _fields.Count;

        #endregion Properties

        #region Indexers

        /// <summary>
        /// Indexador que permite acceder a un FieldDefinition por nombre de columna
        /// </summary>
        public FieldDefinition this[string columnName]
        {
            get => _fields.First(x => x.ColumnName == columnName);
            set
            {
                var index = _fields.FindIndex(e => e.ColumnName == columnName);
                if (index >= 0)
                    _fields[index] = value;
            }
        }

        public FieldDefinition this[int index] => _fields[index];

        #endregion Indexers

        #region Methods

        /// <summary>
        /// Agrega una nueva definicion apartir de <see cref="FieldDefinition"/>
        /// </summary>
        /// <param name="fieldDefinition"></param>
        public void Add(FieldDefinition fieldDefinition)
        {
            if (!Contains(fieldDefinition.ColumnName))
                _fields.Add(fieldDefinition);
        }

        /// <summary>
        /// Agrega una nueva definicion apartir de <see cref="PropertyInfo"/>
        /// </summary>
        /// <param name="fieldProperty"></param>
        public void Add(PropertyInfo fieldProperty)
        {
            if (!Contains(fieldProperty.Name))
                _fields.Add(new FieldDefinition(fieldProperty));
        }

        /// <summary>
        /// Agrega una nueva definicion apartir de los parametros
        /// </summary>
        /// <param name="fieldName">Indica el nombre interno del campo</param>
        /// <param name="displayName"></param>
        /// <param name="description"></param>
        /// <param name="sourceColumnName"></param>
        /// <param name="fieldType"></param>
        /// <param name="allowNull"></param>
        public void Add(string fieldName, string displayName = "", string sourceColumnName = "",
            string description = "", Type? fieldType = null, bool allowNull = false)
        {
            if (!Contains(fieldName))
                _fields.Add(new FieldDefinition(fieldName, displayName, sourceColumnName, description, fieldType, allowNull));
        }

        /// <summary>
        /// Agrega las definiciones en base al tipo
        /// </summary>
        public void Add<T>()
        {
            var pi = typeof(T).GetPropertiesInfo();
            AddRange(pi);
        }

        /// <summary>
        /// Agrega una nueva definicion apartir de <see cref="FieldDefinition"/>
        /// </summary>
        /// <param name="fieldDefinition"></param>
        public void AddRange(IEnumerable<FieldDefinition> fieldDefinitions)
        {
            foreach (var item in fieldDefinitions)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Agrega una nueva definicion apartir de una colección de <see cref="PropertyInfo"/>
        /// </summary>
        /// <param name="fieldsProperty"></param>
        public void AddRange(IEnumerable<PropertyInfo> fieldsProperty)
        {
            foreach (var item in fieldsProperty)
            {
                Add(item);
            }
        }

        public bool Contains(string fieldName)
        {
            return _fields.Any(x => x.ColumnName == fieldName);
        }

        public IEnumerator<FieldDefinition> GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Elimina una definición por nombre de columna
        /// </summary>
        /// <param name="columnName">Nombre de la columna a eliminar</param>
        /// <returns>True si se eliminó, false si no se encontró</returns>
        public bool Remove(string columnName)
        {
            var index = _fields.FindIndex(e => e.ColumnName == columnName);
            if (index >= 0)
            {
                _fields.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Intenta obtener una definición por nombre de columna
        /// </summary>
        /// <param name="columnName">Nombre de la columna</param>
        /// <param name="definition">Definición encontrada (null si no existe)</param>
        /// <returns>True si se encontró, false en caso contrario</returns>
        public bool TryGetDefinition(string columnName, out FieldDefinition? definition)
        {
            definition = this.FirstOrDefault(x => x.ColumnName == columnName);
            return definition != null;
        }

        #endregion Methods
    }
}