using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Wizard;
using KUtilitiesCore.Data.WPF.Importer;
using KUtilitiesCore.Data.WPFTests.TestDoubles;
using KUtilitiesCore.MVVM.MessageService;
using System.Collections.ObjectModel;
using System.Data;

namespace KUtilitiesCore.Data.WPFTests.WPF
{
    /// <summary>
    /// Pruebas del ViewModel WPF del asistente de importación: verifica que los
    /// comandos delegan en el orquestador compartido, que los eventos del
    /// orquestador se traducen en notificaciones de propiedades y que el estado
    /// compuesto CanImport refleja tanto la obsolescencia como la validez de los datos.
    /// </summary>
    [TestClass]
    public sealed class ImportWizardViewModelTests
    {
        [TestMethod]
        public void ViewModel_Commands_ExecuteAgainstController()
        {
            var definitions = CreateSampleDefinitions();
            var table = CreateSampleTable();
            var debounceTimer = new FakeDebounceTimer();
            var reader = new FakeDataSourceReader(table);
            using var viewModel = new TestableImportWizardViewModel(definitions, debounceTimer: debounceTimer)
            {
                ReaderOverride = reader
            };
            var openFileRequestedCount = 0;
            var closeRequestedCount = 0;
            viewModel.OpenFileRequested += (s, e) => openFileRequestedCount++;
            viewModel.CloseRequested += (s, e) => closeRequestedCount++;

            Assert.IsTrue(viewModel.BrowseFileCommand.CanExecute(null), "Browse siempre debe poder ejecutarse.");
            viewModel.BrowseFileCommand.Execute(null);
            Assert.AreEqual(1, openFileRequestedCount, "Browse debe solicitar la apertura del dialogo al host.");

            viewModel.FileName = "datos.csv";
            Assert.IsInstanceOfType(viewModel.CurrentConfig, typeof(CsvConfigViewModel));

            Assert.IsTrue(viewModel.LoadDataCommand.CanExecute(null));
            viewModel.LoadDataCommand.Execute(null);

            Assert.IsNotNull(viewModel.LoadedDataView, "Load debe delegar en el orquestador y exponer la vista cargada.");
            Assert.AreSame(table, viewModel.LoadedDataView!.Table);
            Assert.AreEqual(1, reader.ReadCalls);

            Assert.IsTrue(viewModel.ImportCommand.CanExecute(null));
            viewModel.ImportCommand.Execute(null);

            Assert.IsNotNull(viewModel.ResultData, "Import debe delegar en el orquestador y materializar el resultado tipado.");
            Assert.AreEqual(2, viewModel.ResultData!.Rows.Count);
            Assert.AreEqual(1, closeRequestedCount, "Con AutoCloseOnSuccess el VM debe solicitar el cierre tras importar con exito.");
        }

        [TestMethod]
        public async Task ViewModel_RaisesPropertyChanged_OnControllerEvents()
        {
            var definitions = CreateSampleDefinitions();
            var table = CreateSampleTable();
            using var viewModel = new TestableImportWizardViewModel(definitions)
            {
                ReaderOverride = new FakeDataSourceReader(table)
            };
            var changedProperties = new List<string>();
            viewModel.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName ?? string.Empty);

            viewModel.FileName = "datos.csv";
            await viewModel.LoadDataAsync();

            CollectionAssert.Contains(changedProperties, nameof(ImportWizardViewModel.IsLoading), "IsLoading debe notificarse al iniciar la carga.");
            CollectionAssert.Contains(changedProperties, nameof(ImportWizardViewModel.LoadedDataView), "LoadCompleted debe notificar LoadedDataView.");
            CollectionAssert.Contains(changedProperties, nameof(ImportWizardViewModel.CanImport), "LoadCompleted debe notificar CanImport.");
            CollectionAssert.Contains(changedProperties, nameof(ImportWizardViewModel.LoadedRowCount), "LoadCompleted debe notificar LoadedRowCount.");
        }

        [TestMethod]
        public async Task ViewModel_CanImport_MirrorsStaleAndValidState()
        {
            var definitions = CreateSampleDefinitions();
            var table = CreateSampleTable();
            using var viewModel = new TestableImportWizardViewModel(definitions)
            {
                ReaderOverride = new FakeDataSourceReader(table)
            };
            viewModel.FileName = "datos.csv";
            await viewModel.LoadDataAsync();
            Assert.IsTrue(viewModel.CanImport, "Datos cargados y sin validar: import habilitado.");
            Assert.IsTrue(viewModel.ImportCommand.CanExecute(null));

            Assert.IsInstanceOfType(viewModel.CurrentConfig, typeof(CsvConfigViewModel));
            var csv = (CsvConfigViewModel)viewModel.CurrentConfig!;
            csv.TrimValues = !csv.TrimValues;
            Assert.IsFalse(viewModel.CanImport, "Opciones cambiadas marcan los datos como obsoletos: import deshabilitado.");
            Assert.IsFalse(viewModel.ImportCommand.CanExecute(null), "El comando Import debe deshabilitarse con datos obsoletos.");

            await viewModel.LoadDataAsync();
            Assert.IsTrue(viewModel.CanImport, "Recargar restaura el estado vigente.");

            table.Rows[0]["Edad"] = "abc";
            viewModel.Revalidate();
            Assert.IsFalse(viewModel.CanImport, "Datos invalidos tras revalidar: import deshabilitado.");
            Assert.IsFalse(viewModel.ImportCommand.CanExecute(null), "El comando Import debe deshabilitarse con datos invalidos.");
            Assert.IsTrue(viewModel.RowErrors.Count > 0, "La exposicion neutral de errores debe reflejar los errores de validacion.");

            table.Rows[0]["Edad"] = "42";
            viewModel.Revalidate();
            Assert.IsTrue(viewModel.CanImport, "Corregir y revalidar rehabilita la importacion.");
        }

        [TestMethod]
        public void CsvOptions_ExposeTrimAndIgnoreEmptyLines()
        {
            using var viewModel = new TestableImportWizardViewModel(CreateSampleDefinitions());
            viewModel.FileName = "datos.csv";
            Assert.IsInstanceOfType(viewModel.CurrentConfig, typeof(CsvConfigViewModel));
            var csv = (CsvConfigViewModel)viewModel.CurrentConfig!;

            Assert.IsTrue(csv.TrimValues, "TrimValues inicia activado por defecto.");
            Assert.IsTrue(csv.IgnoreEmptyLines, "IgnoreEmptyLines inicia activado por defecto.");

            csv.TrimValues = false;
            csv.IgnoreEmptyLines = false;
            Assert.IsInstanceOfType(csv.GetParsingOptions(), typeof(TextFileParsingOptions));
            var options = (TextFileParsingOptions)csv.GetParsingOptions();
            Assert.IsFalse(options.TrimValues);
            Assert.IsFalse(options.IgnoreEmptyLines);
        }

        [TestMethod]
        public void ExcelOptions_ExposeSheetAndRowRange()
        {
            using var viewModel = new TestableImportWizardViewModel(CreateSampleDefinitions());
            viewModel.FileName = "libro.xlsx";
            Assert.IsInstanceOfType(viewModel.CurrentConfig, typeof(ExcelConfigViewModel));
            var excel = (ExcelConfigViewModel)viewModel.CurrentConfig!;

            excel.Sheets.Add("Hoja1");
            excel.Sheets.Add("Hoja2");
            excel.SelectedSheet = "Hoja2";
            excel.StartRow = 2;
            excel.EndRow = 0;
            Assert.IsInstanceOfType(excel.GetParsingOptions(), typeof(ExcelParsingOptions));
            var options = (ExcelParsingOptions)excel.GetParsingOptions();
            Assert.AreEqual("Hoja2", options.SheetName);
            Assert.AreEqual(2, options.StartRow);
            Assert.IsNull(options.EndRow, "EndRow 0 en la UI significa 'hasta el final' y debe mapear a null.");

            excel.EndRow = 10;
            Assert.IsInstanceOfType(excel.GetParsingOptions(), typeof(ExcelParsingOptions));
            options = (ExcelParsingOptions)excel.GetParsingOptions();
            Assert.AreEqual(10, options.EndRow);
        }

        private static FieldDefinitionCollection CreateSampleDefinitions()
        {
            var definitions = new FieldDefinitionCollection();
            definitions.Add("Nombre", displayName: "Nombre");
            definitions.Add("Edad", displayName: "Edad", fieldType: typeof(int));
            return definitions;
        }

        private static DataTable CreateSampleTable()
        {
            var table = new DataTable();
            table.Columns.Add("Nombre", typeof(string));
            table.Columns.Add("Edad", typeof(string));
            table.Rows.Add("Ana", "30");
            table.Rows.Add("Luis", "25");
            return table;
        }

        /// <summary>
        /// Subclase de prueba del ViewModel que sustituye la creacion del lector de datos
        /// por un doble de prueba, evitando archivos reales en disco.
        /// </summary>
        private sealed class TestableImportWizardViewModel : ImportWizardViewModel
        {
            public TestableImportWizardViewModel(
                FieldDefinitionCollection fieldDefinitions,
                ImportManager? importManager = null,
                IWizardDebounceTimer? debounceTimer = null,
                ISupportMessageService? messageService = null)
                : base(fieldDefinitions, importManager, debounceTimer, messageService)
            {
            }

            public IDataSourceReader? ReaderOverride { get; set; }

            protected override IDataSourceReader? CreateReader() => ReaderOverride;
        }
    }
}
