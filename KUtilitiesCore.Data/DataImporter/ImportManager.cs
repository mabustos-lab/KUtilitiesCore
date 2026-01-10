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
    public class ImportManager
    {
        #region Constructors

        public ImportManager()
        {
            ColumnDefinitions = new FielDefinitionCollection();
            ValidationErrors = new ValidationResult();
            DataSource = new DataTable();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Configuración de mapeo
        /// </summary>
        public FielDefinitionCollection ColumnDefinitions { get; private set; }

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

        /// <summary>
        /// Carga datos desde una fuente externa (CSV, Excel, etc.)
        /// </summary>
        /// <param name="reader">Lector de datos</param>
        /// <exception cref="ArgumentNullException">Cuando reader es null</exception>
        /// <exception cref="InvalidOperationException">Cuando no hay definiciones de columnas configuradas</exception>
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
                ConfigurarDataTable();
                var rawData = ReadDataWithValidation(reader);
                
                DataSource.Clear();
                int rowIndex = 0;

                foreach (DataRow rawRow in rawData.Rows)
                {
                    DataRow newRow = DataSource.NewRow();

                    // Mapeo seguro: Si la columna origen existe en el lector, la usamos.
                    foreach (var col in ColumnDefinitions)
                    {
                        // Busqueda por case-insensitive
                        DataColumn dtColumn = rawData.Columns
                            .Cast<DataColumn>()
                            .FirstOrDefault(
                                x => string.Equals(
                                    x.ColumnName,
                                    col.SourceColumnName,
                                    StringComparison.OrdinalIgnoreCase));                        
                        if (dtColumn!=null)
                        {
                            newRow[col.ColumnName] = rawRow[dtColumn.ColumnName]?.ToString();
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
        /// Establece la definición de las columnas
        /// </summary>
        /// <param name="mapping">Configuración de mapeo</param>
        /// <exception cref="ArgumentNullException">Cuando mapping es null</exception>
        public void SetMapping(FielDefinitionCollection mapping)
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
            ValidationErrors.Clear();

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
        /// Obtiene las filas inválidas del DataTable
        /// </summary>
        /// <returns>Enumerable de filas inválidas</returns>
        public IEnumerable<DataRow> GetInvalidRows()
        {
            return DataSource.Rows.Cast<DataRow>()
                .Where(row => !(row["_IsValid"] is bool isValid && isValid));
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
                DataColumn dc = DataSource.Columns.Add(field.ColumnName, typeof(string));
                dc.Caption = field.SourceColumnName;
                //dc.AllowDBNull = field.AllowNull;
            }

            // Agregamos columnas de control internas (útiles para la UI)
            DataSource.Columns.Add("_RowIndex", typeof(int));
            DataSource.Columns.Add("_IsValid", typeof(bool));
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

            foreach (var def in ColumnDefinitions)
            {
                string value = row[def.ColumnName]?.ToString() ?? string.Empty;

                // Validar campo requerido
                if (!def.AllowNull && string.IsNullOrWhiteSpace(value))
                {
                    string errMsg = $"El campo '{def.DisplayName}' es requerido.";
                    rowValid = false;
                    ValidationErrors.AddFailure(new ValidationFailure(def.SourceColumnName, rowIndex, errMsg));
                    row.SetColumnError(def.ColumnName, errMsg);
                    continue;
                }

                // Si está vacío y permite nulos, no validar tipo
                if (string.IsNullOrWhiteSpace(value) && def.AllowNull)
                    continue;

                // Validar tipo de dato
                if (!def.IsValidValueType(value))
                {
                    string errMsg = $"Se esperaba un valor de tipo [{def.FieldType?.Name ?? "desconocido"}] para el valor '{value}'.";
                    rowValid = false;
                    ValidationErrors.AddFailure(new ValidationFailure(def.SourceColumnName, rowIndex, errMsg));
                    row.SetColumnError(def.ColumnName, errMsg);
                }
            }

            return rowValid;
        }

        /// <summary>
        /// Obtiene el nombre de columna interno por el nombre de columna fuente
        /// </summary>
        private string GetColumnNameBySource(string sourceColumnName)
        {
            return ColumnDefinitions
                .FirstOrDefault(x => x.SourceColumnName == sourceColumnName)?
                .ColumnName ?? sourceColumnName;
        }

        #endregion Methods
    }
}