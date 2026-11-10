using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using System.IO;

namespace KUtilitiesCore.Data.WPF.Importer
{
    /// <summary>
    /// ViewModel de configuración para archivos de texto delimitados (CSV/TSV/PSV).
    /// Refleja las opciones del control WinForms homónimo para mantener la paridad de
    /// análisis entre ambos front-ends del asistente de importación.
    /// </summary>
    /// <remarks>
    /// La codificación del archivo no se expone en esta versión: se usa el valor por
    /// defecto de <see cref="TextFileParsingOptions"/> (UTF-8). Es un punto de extensión
    /// deliberadamente diferido porque la paridad exigida cubre únicamente
    /// <see cref="TrimValues"/> e <see cref="IgnoreEmptyLines"/>.
    /// </remarks>
    public sealed class CsvConfigViewModel : ConfigViewModelBase
    {
        private DelimiterOption? _selectedDelimiter;
        private bool _hasHeader = true;
        private bool _trimValues = true;
        private bool _ignoreEmptyLines = true;

        /// <summary>
        /// Opciones de separador ofrecidas por defecto, con etiquetas legibles en español.
        /// El combo de la interfaz enlaza esta lista y compara por referencia.
        /// </summary>
        public static IReadOnlyList<DelimiterOption> DefaultDelimiterOptions { get; } =
        [
            new DelimiterOption("Coma (,)", ","),
            new DelimiterOption("Punto y coma (;)", ";"),
            new DelimiterOption("Pipe (|)", "|"),
            new DelimiterOption("Tabulación", "\t")
        ];

        /// <summary>
        /// Separador de columnas seleccionado por el usuario.
        /// </summary>
        public DelimiterOption? SelectedDelimiter
        {
            get => _selectedDelimiter;
            set => SetProperty(ref _selectedDelimiter, value);
        }

        /// <summary>
        /// Indica si la primera fila del archivo contiene encabezados.
        /// </summary>
        public bool HasHeader
        {
            get => _hasHeader;
            set => SetProperty(ref _hasHeader, value);
        }

        /// <summary>
        /// Indica si se deben recortar los espacios al inicio y fin de los valores leídos.
        /// </summary>
        public bool TrimValues
        {
            get => _trimValues;
            set => SetProperty(ref _trimValues, value);
        }

        /// <summary>
        /// Indica si se deben ignorar las filas completamente vacías del archivo.
        /// </summary>
        public bool IgnoreEmptyLines
        {
            get => _ignoreEmptyLines;
            set => SetProperty(ref _ignoreEmptyLines, value);
        }

        /// <summary>
        /// Inicializa una instancia con el separador por defecto (coma).
        /// </summary>
        public CsvConfigViewModel()
        {
            _selectedDelimiter = DefaultDelimiterOptions[0];
        }

        /// <summary>
        /// Restaura la configuración a partir del archivo seleccionado, preconfigurando
        /// el separador según la extensión del archivo (.tsv → tabulación, .psv → pipe).
        /// La restauración no emite <see cref="ConfigViewModelBase.OptionsChanged"/>.
        /// </summary>
        /// <param name="fileName">Ruta del archivo seleccionado.</param>
        public override void Initialize(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            SuppressOptionsChanged(() =>
            {
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                SelectedDelimiter = extension switch
                {
                    ".tsv" => DefaultDelimiterOptions[3],
                    ".psv" => DefaultDelimiterOptions[2],
                    _ => SelectedDelimiter
                };
            });
        }

        /// <summary>
        /// Proyecta el estado actual del ViewModel hacia las opciones de análisis
        /// consumidas por el lector de archivos de texto.
        /// </summary>
        /// <returns>Opciones de parseo de archivos planos construidas a partir de la selección del usuario.</returns>
        public override IParsingOptions GetParsingOptions()
            => new TextFileParsingOptions
            {
                Separator = SelectedDelimiter?.Value ?? ",",
                HasHeader = HasHeader,
                TrimValues = TrimValues,
                IgnoreEmptyLines = IgnoreEmptyLines
            };
    }
}
