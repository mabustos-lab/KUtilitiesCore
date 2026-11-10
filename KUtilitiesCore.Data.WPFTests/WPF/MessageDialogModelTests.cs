using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.WPF.Importer;
using KUtilitiesCore.Data.WPF.MessageService;
using KUtilitiesCore.Data.WPFTests.TestDoubles;
using KUtilitiesCore.MVVM.MessageService;
using System.Data;

namespace KUtilitiesCore.Data.WPFTests.WPF
{
    /// <summary>
    /// Pruebas del modelo puro del diálogo de mensajes de WPF (<see cref="MessageDialogModel"/>):
    /// mapeo de <see cref="MessageArgs"/>, colapso de secciones opcionales y enrutamiento
    /// de fallos de carga del ViewModel hacia el servicio de mensajes.
    /// </summary>
    [TestClass]
    public sealed class MessageDialogModelTests
    {
        [TestMethod]
        public void MessageDialogModel_FromArgs_MapsHeadingFootnoteExpander()
        {
            var args = new MessageArgs
            {
                Caption = "Titulo",
                Heading = "Encabezado grande",
                Message = "Mensaje principal.",
                Footnote = "Pie de pagina",
                Expander = "Detalles tecnicos",
                MessageType = MessageType.Warning,
                MessageIcon = MessageIcon.Warning
            };

            var model = MessageDialogModel.FromArgs(args);

            Assert.AreEqual("Titulo", model.Caption);
            Assert.AreEqual("Encabezado grande", model.Heading);
            Assert.AreEqual("Mensaje principal.", model.Message);
            Assert.AreEqual("Pie de pagina", model.Footnote);
            Assert.AreEqual("Detalles tecnicos", model.Expander);
            Assert.AreEqual(MessageType.Warning, model.MessageType);
            Assert.AreEqual(MessageIcon.Warning, model.MessageIcon);
            Assert.IsTrue(model.ShowHeading);
            Assert.IsTrue(model.ShowFootnote);
            Assert.IsTrue(model.ShowExpander);
        }

        [TestMethod]
        public void MessageDialogModel_EmptyOptionals_AreCollapsed()
        {
            var args = new MessageArgs
            {
                Caption = "Titulo",
                Message = "Mensaje principal."
            };

            var model = MessageDialogModel.FromArgs(args);

            Assert.IsFalse(model.ShowHeading, "Sin encabezado la seccion debe omitirse.");
            Assert.IsFalse(model.ShowFootnote, "Sin pie de pagina la seccion debe omitirse.");
            Assert.IsFalse(model.ShowExpander, "Sin expander la seccion de detalles debe omitirse.");
        }

        [TestMethod]
        public async Task ViewModel_LoadFailed_RoutesToMessageService()
        {
            var definitions = new FieldDefinitionCollection();
            definitions.Add("Nombre", displayName: "Nombre");
            var fakeService = new FakeMessageService();
            var reader = new FakeDataSourceReader(CreateSampleTable())
            {
                ThrowOnRead = new InvalidOperationException("boom de lectura")
            };

            using var viewModel = new TestableImportWizardViewModel(definitions, messageService: fakeService)
            {
                ReaderOverride = reader
            };

            viewModel.FileName = "datos.csv";
            await viewModel.LoadDataAsync();

            Assert.AreEqual(1, fakeService.ShowAsyncCalls, "El fallo de carga debe notificarse una sola vez.");
            Assert.IsNotNull(fakeService.LastMessage);
            Assert.AreEqual("Error al leer el archivo.", fakeService.LastMessage!.Heading);
            Assert.AreEqual("boom de lectura", fakeService.LastMessage.Message);
            StringAssert.Contains(fakeService.LastMessage.Expander, "boom de lectura");
            Assert.AreEqual(MessageType.Error, fakeService.LastMessage.MessageType);
            Assert.AreEqual(MessageIcon.Error, fakeService.LastMessage.MessageIcon);
        }

        private static DataTable CreateSampleTable()
        {
            var table = new DataTable();
            table.Columns.Add("Nombre", typeof(string));
            table.Rows.Add("Ana");
            return table;
        }

        private sealed class TestableImportWizardViewModel : ImportWizardViewModel
        {
            public TestableImportWizardViewModel(
                FieldDefinitionCollection fieldDefinitions,
                ImportManager? importManager = null,
                ISupportMessageService? messageService = null)
                : base(fieldDefinitions, importManager, messageService: messageService)
            {
            }

            public IDataSourceReader? ReaderOverride { get; set; }

            protected override IDataSourceReader? CreateReader() => ReaderOverride;
        }
    }
}
