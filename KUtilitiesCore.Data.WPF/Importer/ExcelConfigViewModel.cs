using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using System.Collections.ObjectModel;
using System.IO;

namespace KUtilitiesCore.Data.WPF.Importer
{
    /// <summary>
    /// ViewModel de configuración para libros de Excel. Refleja las opciones del control
    /// WinForms homónimo: hoja de trabajo, encabezados y rango de filas a leer.
    /// </summary>
    /// <remarks>
    /// <see cref="EndRow"/> usa la convención 0 = "leer hasta el final", que se proyecta
    /// como <see langword="null"/> en <see cref="ExcelParsingOptions.EndRow"/>.
    /// La carga de hojas se ejecuta en segundo plano: si el archivo no existe o no es
    /// accesible, la lista queda vacía de forma silenciosa (paridad con el control WinForms).
    /// </remarks>
    public sealed class ExcelConfigViewModel : ConfigViewModelBase
    {
        private string? _selectedSheet;
        private bool _hasHeader = true;
        private int _startRow = 1;
        private int _endRow;

        /// <summary>
        /// Nombres de las hojas de trabajo del libro seleccionado.
        /// </summary>
        public ObservableCollection<string> Sheets { get; } = [];

        /// <summary>
        /// Hoja de trabajo seleccionada para importar; <see langword="null"/> cuando el
        /// libro aún no se ha inspeccionado o no tiene hojas.
        /// </summary>
        public string? SelectedSheet
        {
            get => _selectedSheet;
            set => SetProperty(ref _selectedSheet, value);
        }

        /// <summary>
        /// Indica si la primera fila de la hoja contiene encabezados.
        /// </summary>
        public bool HasHeader
        {
            get => _hasHeader;
            set => SetProperty(ref _hasHeader, value);
        }

        /// <summary>
        /// Fila inicial a leer (base 1); nunca menor que 1.
        /// </summary>
        public int StartRow
        {
            get => _startRow;
            set => SetProperty(ref _startRow, Math.Max(1, value));
        }

        /// <summary>
        /// Fila final a leer; 0 significa "hasta el final" y se proyecta como
        /// <see langword="null"/> en las opciones de análisis.
        /// </summary>
        public int EndRow
        {
            get => _endRow;
            set => SetProperty(ref _endRow, Math.Max(0, value));
        }

        /// <summary>
        /// Descubre en segundo plano las hojas del libro indicado y restaura la selección
        /// a la primera hoja. La restauración no emite
        /// <see cref="ConfigViewModelBase.OptionsChanged"/>.
        /// </summary>
        /// <param name="fileName">Ruta del libro de Excel seleccionado.</param>
        public override void Initialize(string fileName)
        {
            LoadSheetsAsync(fileName);
        }

        /// <summary>
        /// Proyecta el estado actual del ViewModel hacia las opciones de análisis
        /// consumidas por el lector de Excel.
        /// </summary>
        /// <returns>Opciones de lectura de Excel construidas a partir de la selección del usuario.</returns>
        public override IParsingOptions GetParsingOptions()
            => new ExcelParsingOptions
            {
                SheetName = SelectedSheet,
                HasHeader = HasHeader,
                StartRow = StartRow,
                EndRow = EndRow > 0 ? EndRow : null
            };

        /// <summary>
        /// Inspecciona el libro en un hilo de fondo (ClosedXML no es libre de afinidad)
        /// y restaura la lista de hojas en el hilo de llamada al completarse.
        /// Diseñado como <c>async void</c> porque se dispara desde <see cref="Initialize"/>
        /// en modo fire-and-forget; los errores se tragan de forma deliberada para no
        /// romper la interacción del usuario cuando el archivo es inaccesible.
        /// </summary>
        /// <param name="fileName">Ruta del libro de Excel a inspeccionar.</param>
        private async void LoadSheetsAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                return;

            try
            {
                var sheets = await Task.Run(() => ExcelSourceReaderFactory.GetSheets(fileName));
                SuppressOptionsChanged(() =>
                {
                    Sheets.Clear();
                    if (sheets is null || sheets.Count == 0)
                    {
                        SelectedSheet = null;
                        return;
                    }

                    foreach (var sheet in sheets)
                        Sheets.Add(sheet);
                    SelectedSheet = sheets[0];
                });
            }
            catch
            {
                SuppressOptionsChanged(() =>
                {
                    Sheets.Clear();
                    SelectedSheet = null;
                });
            }
        }
    }
}
