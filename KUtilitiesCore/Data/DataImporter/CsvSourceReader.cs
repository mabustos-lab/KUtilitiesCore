using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KUtilitiesCore.Data.DataImporter
{
    /// <summary>
    /// Permite obtener los datos de una fuente de datos de texto plano.
    /// </summary>
    public class CsvSourceReader : ICsvSourceReader
    {
        /// <inheritdoc/>
        public string FilePath { get; set; }
        /// <inheritdoc/>
        public string SpliterChar { get; set; }

        /// <inheritdoc/>
        public bool CanRead 
            => !string.IsNullOrEmpty(FilePath) && !string.IsNullOrEmpty(SpliterChar);
        /// <summary>
        /// Inicializa la instancia para importar los datos
        /// </summary>
        /// <param name="filePath"></param>
        public CsvSourceReader(string filePath)
        {
            FilePath = filePath;
            SpliterChar = ",";
        }
        /// <inheritdoc/>
        public DataTable ReadData()
        {
            if (!CanRead)
            {
                throw new InvalidOperationException("No se puede leer el archivo. Verifique la ruta y el separador.");
            }

            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException($"El archivo no existe: {FilePath}");
            }

            DataTable dataTable = new DataTable();
            List<string> columnNames = new List<string>();
            Dictionary<int, int> columnMapping = new Dictionary<int, int>();
            bool headerProcessed = false;

            using (StreamReader reader = new StreamReader(FilePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] values = line.Split(new string[] { SpliterChar }, StringSplitOptions.None);

                    if (!headerProcessed)
                    {
                        // Procesar encabezado
                        for (int i = 0; i < values.Length; i++)
                        {
                            string columnName = values[i].Trim();

                            // Si ya existe una columna con este nombre, ignorarla (mantener la primera)
                            if (!columnNames.Contains(columnName))
                            {
                                columnNames.Add(columnName);
                                dataTable.Columns.Add(columnName, typeof(string));

                                // Mapear índice original al índice único en DataTable
                                columnMapping[i] = columnNames.IndexOf(columnName);
                            }                            
                        }

                        headerProcessed = true;
                    }
                    else
                    {
                        // Procesar fila de datos
                        DataRow row = dataTable.NewRow();

                        for (int i = 0; i < values.Length; i++)
                        {
                            if (columnMapping.ContainsKey(i))
                            {
                                int targetColumnIndex = columnMapping[i];
                                row[targetColumnIndex] = values[i].Trim();
                            }
                        }

                        dataTable.Rows.Add(row);
                    }
                }
            }

            return dataTable;
        }
    }
}
