using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KUtilitiesCore.Data.WPF.Importer
{
    /// <summary>
    /// Fila del grid de mapeo del asistente WPF: asocia un campo destino de la definición
    /// con la columna origen seleccionada de los datos cargados.
    /// </summary>
    /// <remarks>
    /// Cuando <see cref="SelectedSourceColumn"/> es <see cref="IgnoreSourceColumn"/> (o
    /// vacío), el orquestador no mapea el campo y este se excluye del resultado tipado,
    /// igual que la columna "(Ignorar)" del grid de mapeo WinForms.
    /// </remarks>
    public sealed class FieldMappingItem : INotifyPropertyChanged
    {
        private string _selectedSourceColumn;

        /// <summary>
        /// Columna centinela que deshabilita el mapeo del campo hacia el resultado.
        /// </summary>
        public const string IgnoreSourceColumn = "(Ignorar)";

        /// <summary>
        /// Se dispara cuando cambia la columna origen seleccionada para el campo.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Inicializa la fila de mapeo con el campo destino y su columna origen seleccionada.
        /// </summary>
        /// <param name="fieldName">Nombre interno del campo destino.</param>
        /// <param name="displayName">Nombre visible del campo destino.</param>
        /// <param name="description">Descripción funcional del campo destino.</param>
        /// <param name="availableSourceColumns">Columnas origen disponibles para mapear.</param>
        /// <param name="selectedSourceColumn">Columna origen seleccionada inicialmente.</param>
        public FieldMappingItem(string fieldName, string displayName, string description,
            IReadOnlyList<string> availableSourceColumns, string selectedSourceColumn)
        {
            FieldName = fieldName;
            DisplayName = displayName;
            Description = description;
            AvailableSourceColumns = availableSourceColumns;
            _selectedSourceColumn = selectedSourceColumn;
        }

        /// <summary>
        /// Nombre interno del campo destino en la definición de campos.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Nombre visible del campo destino.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Descripción funcional del campo destino.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Columnas origen disponibles para el mapeo, incluida la centinela
        /// <see cref="IgnoreSourceColumn"/> como primera opción.
        /// </summary>
        public IReadOnlyList<string> AvailableSourceColumns { get; }

        /// <summary>
        /// Columna origen seleccionada para alimentar el campo destino.
        /// </summary>
        public string SelectedSourceColumn
        {
            get => _selectedSourceColumn;
            set
            {
                if (string.Equals(_selectedSourceColumn, value, System.StringComparison.Ordinal))
                    return;

                _selectedSourceColumn = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Notifica a los suscriptores que cambió el valor de la propiedad indicada.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad que cambió (inferido por el compilador).</param>
        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
