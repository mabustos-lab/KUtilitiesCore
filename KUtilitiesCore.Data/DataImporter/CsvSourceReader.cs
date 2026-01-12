using KUtilitiesCore.Data.DataImporter.Infrastructure;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter
{
    /// <summary>
    /// Implementación refactorizada para lectura de archivos CSV
    /// </summary>
    public class CsvSourceReader : ICsvSourceReader
    {
        private readonly IDiskFileReader _fileReader;
        private readonly ICsvParser _csvParser;
        private readonly TextFileParsingOptions _parsingOptions;

        /// <inheritdoc/>
        public string FilePath { get; set; }

        /// <inheritdoc/>
        public string SpliterChar
        {
            get => _parsingOptions.Separator;
            set => _parsingOptions.Separator = value;
        }

        /// <inheritdoc/>
        public Encoding Encoding
        {
            get => _parsingOptions.Encoding;
            set => _parsingOptions.Encoding = value;
        }

        /// <inheritdoc/>
        public bool CanRead => !string.IsNullOrEmpty(FilePath) && !string.IsNullOrEmpty(SpliterChar);

        /// <summary>
        /// Constructor principal con inyección de dependencias
        /// </summary>
        /// <param name="filePath">Ruta del archivo CSV</param>
        /// <param name="fileReader">Implementación de acceso a archivos (opcional)</param>
        /// <param name="csvParser">Implementación de parser CSV (opcional)</param>
        /// <param name="parsingOptions">Opciones de parsing (opcional)</param>
        /// <exception cref="ArgumentNullException">Cuando filePath es null o vacío</exception>
        public CsvSourceReader(
            string filePath,
            IDiskFileReader fileReader = null,
            ICsvParser csvParser = null,
            TextFileParsingOptions parsingOptions = null)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _fileReader = fileReader ?? new DefaultDiskFileReader();
            _csvParser = csvParser ?? new DefaultCsvParser();
            _parsingOptions = parsingOptions?.Clone() ?? new TextFileParsingOptions();
        }
        /// <inheritdoc/>
        public async Task<DataTable> ReadDataAsync()
        {
            ValidatePreconditions();

            using var stream = _fileReader.OpenRead(FilePath);
            return await _csvParser.ParseAsync(stream, _parsingOptions)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public DataTable ReadData()
        {
            ValidatePreconditions();

            using var stream = _fileReader.OpenRead(FilePath);
            return _csvParser.Parse(stream, _parsingOptions);
        }

        /// <summary>
        /// Valida las precondiciones antes de leer el archivo
        /// </summary>
        /// <exception cref="InvalidOperationException">Cuando no se cumplen las precondiciones</exception>
        /// <exception cref="System.IO.FileNotFoundException">Cuando el archivo no existe</exception>
        private void ValidatePreconditions()
        {
            if (!CanRead)
            {
                throw new InvalidOperationException(
                    "No se puede leer el archivo. Verifique que la ruta y el separador estén configurados.");
            }

            if (!_fileReader.FileExists(FilePath))
            {
                throw new System.IO.FileNotFoundException(
                    $"El archivo no existe: {FilePath}", FilePath);
            }
        }
    }
}
