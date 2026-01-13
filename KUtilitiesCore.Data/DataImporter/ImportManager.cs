using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Validation.Core;
using KUtilitiesCore.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter
{
    /// <summary>
    /// Clase principal (Independiente de UI). Gestiona el DataTable, la configuración de columnas y
    /// la orquestación.
    /// </summary>
    public class ImportManager : IDisposable
    {
        #region Fields

        private DataTable _rawDataSource;
        private bool disposedValue;

        #endregion Fields

        #region Constructors

        public ImportManager()
        {
            ColumnDefinitions = new FieldDefinitionCollection();
            ValidationErrors = new ValidationResult();
            DataSource = new DataTable();
            _rawDataSource = new DataTable();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Configuración de mapeo
        /// </summary>
        public FieldDefinitionCollection ColumnDefinitions { get; private set; }

        /// <summary>
        /// El DataTable mantiene todo como string para permitir edición en UI sin errores de cast inmediatos.
        /// </summary>
        public DataTable DataSource { get; }

        /// <summary>
        /// Almacena errores de validación actuales para que la UI pueda consultarlos (pintar celdas rojas)
        /// </summary>
        public ValidationResult ValidationErrors { get; private set; }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Obtiene las filas inválidas del DataTable
        /// </summary>
        /// <returns>Enumerable de filas inválidas</returns>
        public IEnumerable<DataRow> GetInvalidRows()
        {
            return DataSource.Rows.Cast<DataRow>()
                .Where(row => !(row["_IsValid"] is bool isValid && isValid));
        }

        /// <summary>
        /// Obtiene las filas válidas del DataTable
        /// </summary>
        /// <returns>Enumerable de filas válidas</returns>
        public IEnumerable<DataRow> GetValidRows()
        {
            return DataSource.Rows.Cast<DataRow>()
                .Where(row => row["_IsValid"] is bool isValid && isValid);
        }

        /// <summary>
        /// Carga datos desde una fuente externa (CSV, Excel, etc.)
        /// </summary>
        /// <param name="reader">Lector de datos</param>
        /// <exception cref="ArgumentNullException">Cuando reader es null</exception>
        /// <exception cref="InvalidOperationException">
        /// Cuando no hay definiciones de columnas configuradas
        /// </exception>
        /// <exception cref="ArgumentException">Cuando el lector no está configurado correctamente</exception>
        public void LoadData(IDataSourceReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader), "El lector de datos no puede ser nulo.");
            }

            if (ColumnDefinitions.Count == 0)
            {
                throw new InvalidOperationException("No se encuentra configurado las definiciones de columnas.");
            }

            if (!reader.CanRead)
            {
                throw new ArgumentException("No se encuentra configurado IDataSourceReader para importar datos.", nameof(reader));
            }

            try
            {
                var rawData = ReadDataWithValidation(reader);
                ReadData(rawData);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                // Re-lanzar excepciones conocidas
                throw;
            }
            catch (Exception ex)
            {
                throw new DataLoadException("Error al cargar los datos desde la fuente.", ex);
            }
        }

        /// <summary>
        /// Lee los datos de un datatable con todas las columnas tipo string
        /// </summary>
        /// <param name="rawData"></param>
        public void ReadData(DataTable rawData)
        {
            _rawDataSource.Reset();
            _rawDataSource = rawData.Clone();
            foreach (DataRow row in rawData.Rows)
                _rawDataSource.ImportRow(row);
            ConfigurarDataTable();
            DataSource.Clear();
            int rowIndex = 0;
            foreach (DataRow rawRow in _rawDataSource.Rows)
            {
                DataRow newRow = DataSource.NewRow();

                // Mapeo seguro: Si la columna origen existe en el lector, la usamos.
                foreach (var col in ColumnDefinitions)
                {
                    // Busqueda por case-insensitive
                    DataColumn dtColumn = _rawDataSource.Columns
                        .Cast<DataColumn>()
                        .FirstOrDefault(
                            x => string.Equals(
                                x.ColumnName,
                                col.SourceColumnName,
                                StringComparison.OrdinalIgnoreCase));
                    if (dtColumn != null)
                    {
                        newRow[col.FieldName] = rawRow[dtColumn.ColumnName]?.ToString();
                    }
                    else if (!col.AllowNull)
                    {
                        // Registrar advertencia para columnas requeridas faltantes
                        ValidationErrors.AddErrorMessage($"Columna requerida '{col.SourceColumnName}' no encontrada en el origen de datos.");
                    }
                }

                newRow["_RowIndex"] = rowIndex++;
                newRow["_IsValid"] = false; // Se validará explícitamente después
                DataSource.Rows.Add(newRow);
            }
        }

        /// <summary>
        /// Establece la definición de las columnas
        /// </summary>
        /// <param name="mapping">Configuración de mapeo</param>
        /// <exception cref="ArgumentNullException">Cuando mapping es null</exception>
        public void SetMapping(FieldDefinitionCollection mapping)
        {
            ColumnDefinitions = mapping ?? throw new ArgumentNullException(nameof(mapping), "La configuración de mapeo no puede ser nula.");

            // Reiniciar estado si hay definiciones nuevas
            if (DataSource.Columns.Count > 0 && mapping.Count > 0)
            {
                DataSource.Reset();
                ConfigurarDataTable();
            }
        }

        /// <summary>
        /// Valida los tipos de datos en C# (Int, DateTime, etc.). Este método debe llamarse al
        /// cargar y cada vez que el usuario edite una celda en la UI.
        /// </summary>
        /// <returns>True si todos los datos son válidos</returns>
        public bool ValidateDataTypes()
        {
            ValidationErrors.Errors.Clear();

            if (DataSource.Rows.Count == 0)
            {
                ValidationErrors.AddErrorMessage("No hay datos para validar.");
                return true;
            }

            bool allRowsValid = true;

            // Validar columnas requeridas en el origen
            ValidateRequiredColumns();

            // Validar cada fila
            foreach (DataRow row in DataSource.Rows)
            {
                bool rowValid = ValidateRow(row);
                row["_IsValid"] = rowValid;

                if (!rowValid)
                {
                    allRowsValid = false;
                }
            }

            return allRowsValid && ValidationErrors.IsValid;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DataSource.Dispose();
                    _rawDataSource.Dispose();
                }

                // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
                // TODO: establecer los campos grandes como NULL
                disposedValue = true;
            }
        }

        /// <summary>
        /// Lee datos del lector con validación previa
        /// </summary>
        private static DataTable ReadDataWithValidation(IDataSourceReader reader)
        {
            try
            {
                return reader.ReadData();
            }
            catch (FileNotFoundException ex)
            {
                throw new DataLoadException($"Archivo no encontrado: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                throw new DataLoadException($"Error de E/S al leer el archivo: {ex.Message}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DataLoadException($"Acceso no autorizado al archivo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Configura el DataTable con las definiciones de columna
        /// </summary>
        private void ConfigurarDataTable()
        {
            DataSource.Reset();

            // Agregamos columnas de negocio (todo string)
            foreach (var field in ColumnDefinitions)
            {
                DataColumn dc = DataSource.Columns.Add(field.FieldName, typeof(string));
                dc.Caption = field.SourceColumnName;
                //dc.AllowDBNull = field.AllowNull;
            }

            // Agregamos columnas de control internas (útiles para la UI)
            DataSource.Columns.Add("_RowIndex", typeof(int));
            DataSource.Columns.Add("_IsValid", typeof(bool));
        }

        /// <summary>
        /// Obtiene el nombre de columna interno por el nombre de columna fuente
        /// </summary>
        private string GetColumnNameBySource(string sourceColumnName)
        {
            return ColumnDefinitions
                .FirstOrDefault(x => x.SourceColumnName == sourceColumnName)?
                .FieldName ?? sourceColumnName;
        }

        /// <summary>
        /// Valida las columnas requeridas
        /// </summary>
        private void ValidateRequiredColumns()
        {
            var requiredColumns = ColumnDefinitions
                .Where(x => !x.AllowNull)
                .Select(x => x.SourceColumnName)
                .ToList();

            if (!requiredColumns.Any())
                return;

            // Verificar que las columnas requeridas tengan datos
            var emptyRequiredColumns = requiredColumns
                .Where(colName =>
                    DataSource.Rows.Cast<DataRow>()
                        .All(row => string.IsNullOrWhiteSpace(row[GetColumnNameBySource(colName)]?.ToString())))
                .ToList();

            if (emptyRequiredColumns.Any())
            {
                ValidationErrors.AddErrorMessage($"Columnas requeridas sin datos: {string.Join(", ", emptyRequiredColumns)}.");
            }
        }

        /// <summary>
        /// Valida una fila individual
        /// </summary>
        private bool ValidateRow(DataRow row)
        {
            bool rowValid = true;
            int rowIndex = (int)row["_RowIndex"];
            row.ClearErrors();
            _rawDataSource.Rows[rowIndex].ClearErrors();

            foreach (var def in ColumnDefinitions)
            {
                string value = row[def.FieldName]?.ToString() ?? string.Empty;
                DataColumn dcSource = _rawDataSource.Columns
                    .Cast<DataColumn>()
                    .FirstOrDefault(
                        x => x.ColumnName.Equals(def.SourceColumnName, StringComparison.InvariantCultureIgnoreCase));

                // Validar campo requerido
                if (!def.AllowNull && string.IsNullOrWhiteSpace(value))
                {
                    if (def.DefaultValue != null)
                    {
                        row[def.FieldName] = def.DefaultValue.ToString();
                        if (dcSource != null)
                            _rawDataSource.Rows[rowIndex][def.SourceColumnName] = def.DefaultValue.ToString();
                        continue;
                    }

                    string errMsg = $"El campo '{def.DisplayName}' es requerido.";
                    rowValid = false;
                    ValidationErrors.AddError(new ValidationFailure(def.SourceColumnName, errMsg, rowIndex));
                    // validamos si existe esa columna en el DataSource
                    if (dcSource != null)
                        _rawDataSource.Rows[rowIndex].SetColumnError(def.SourceColumnName, errMsg);
                    row.SetColumnError(def.FieldName, errMsg);
                    continue;
                }

                // Si está vacío y permite nulos, no validar tipo
                if(string.IsNullOrWhiteSpace(value) && def.AllowNull)
                {
                    if (def.DefaultValue != null)
                        row[def.FieldName]=def.DefaultValue.ToString();
                    continue;
                }

                // Validar tipo de dato
                if (!def.IsValidValueType(value))
                {
                    string errMsg = $"Se esperaba un valor de tipo [{def.TargetType?.Name ?? "desconocido"}] para el valor '{value}'.";
                    rowValid = false;
                    ValidationErrors.AddError(new ValidationFailure(def.SourceColumnName, errMsg, rowIndex, value));
                    if (dcSource != null)
                        _rawDataSource.Rows[rowIndex].SetColumnError(def.SourceColumnName, errMsg);
                    row.SetColumnError(def.FieldName, errMsg);
                }
                else
                {
                    RunBusinessRules(def, def.TypeConverter.TryConvert(value),rowIndex);
                }
            }

            return rowValid;
        }

        private void RunBusinessRules(IFieldDefinitionItem definition, object convertedValue, int currentRowIndex)
        {
            if (definition.ValidationRules != null && definition.ValidationRules.Count > 0)
            {
                foreach (var rule in definition.ValidationRules)
                {
                    var ruleFailures = rule.Validate(convertedValue, definition.FieldName);
                    if (ruleFailures != null)
                    {
                        foreach (var item in ruleFailures)
                        {
                            item.IndexRow = currentRowIndex;
                            ValidationErrors.Errors.Add(item);
                        }
                    }
                }
            }
        }

        #endregion Methods
    }
}