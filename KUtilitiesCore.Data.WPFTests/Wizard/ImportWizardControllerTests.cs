using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Wizard;
using System.Data;

namespace KUtilitiesCore.Data.WPFTests.Wizard;

[TestClass]
public class ImportWizardControllerTests
{
    [TestMethod]
    public void ProjectReferences_Resolve()
    {
        Assert.IsNotNull(typeof(ImportManager));
    }

    [TestMethod]
    public async Task Controller_AfterLoad_ExposesLoadedDataView_AndStopsLoading()
    {
        var table = CreateSampleTable();
        var reader = new TestDoubles.FakeDataSourceReader(table);
        using var controller = new ImportWizardController(CreateSampleDefinitions());

        var loaded = await controller.LoadAsync(reader);

        Assert.IsTrue(loaded, "La primera carga debe completarse sin rechazo.");
        Assert.IsFalse(controller.IsLoading, "Al terminar la carga IsLoading debe volver a false.");
        Assert.IsNotNull(controller.LoadedDataView, "La vista previa debe quedar observable tras la carga.");
        Assert.AreSame(table, controller.LoadedDataView.Table, "La tabla del lector se adopta sin clonar.");
        Assert.AreEqual(table.Rows.Count, controller.LoadedDataView.Count);
        Assert.IsTrue(controller.CanImport, "Con filas cargadas y datos vigentes se puede importar.");
    }

    [TestMethod]
    public async Task Controller_SecondLoadWhileLoading_IsRejected()
    {
        var table = CreateSampleTable();
        var gate = new TaskCompletionSource<DataTable>();
        var slowReader = new TestDoubles.FakeDataSourceReader(table, gate);
        var idleReader = new TestDoubles.FakeDataSourceReader(CreateSampleTable());
        using var controller = new ImportWizardController(CreateSampleDefinitions());

        var first = controller.LoadAsync(slowReader);
        Assert.IsTrue(controller.IsLoading, "Durante la primera carga IsLoading debe estar activo.");

        var secondStarted = await controller.LoadAsync(idleReader);

        Assert.IsFalse(secondStarted, "Una segunda carga mientras hay una en curso debe ser rechazada.");
        Assert.AreEqual(0, idleReader.ReadCalls, "El lector rechazado no debe iniciar ninguna lectura.");
        Assert.IsTrue(controller.IsLoading, "La primera carga sigue en curso tras el rechazo.");

        gate.SetResult(table);
        var firstCompleted = await first;

        Assert.IsTrue(firstCompleted, "La primera carga completa normalmente pese al rechazo de la segunda.");
        Assert.AreEqual(1, slowReader.ReadCalls, "Solo la primera carga lee la fuente.");
        Assert.IsFalse(controller.IsLoading);
        Assert.IsNotNull(controller.LoadedDataView, "La tabla de la primera carga es la que queda cargada.");
        Assert.AreSame(table, controller.LoadedDataView!.Table);
    }

    [TestMethod]
    public async Task Controller_EmptyFileName_ResetsState()
    {
        var reader = new TestDoubles.FakeDataSourceReader(CreateSampleTable());
        using var controller = new ImportWizardController(CreateSampleDefinitions())
        {
            FileName = "datos.csv"
        };
        await controller.LoadAsync(reader);
        Assert.IsTrue(controller.CanImport, "Estado previo: datos cargados y vigentes.");

        controller.FileName = string.Empty;

        Assert.AreEqual(string.Empty, controller.FileName);
        Assert.IsNull(controller.LoadedDataView, "La vista previa se limpia al resetear.");
        Assert.IsNull(controller.ResultData, "El resultado se limpia al resetear.");
        Assert.IsFalse(controller.CanImport, "El estado inicial no permite importar.");
        Assert.IsFalse(controller.IsLoading);
    }

    [TestMethod]
    public async Task Controller_ZeroRowTable_KeepsImportDisabled()
    {
        var emptyTable = new DataTable();
        emptyTable.Columns.Add("Nombre");
        var reader = new TestDoubles.FakeDataSourceReader(emptyTable);
        using var controller = new ImportWizardController(CreateSampleDefinitions());

        var loaded = await controller.LoadAsync(reader);

        Assert.IsTrue(loaded, "Cargar una tabla vacia no es un fallo de carga.");
        Assert.IsNotNull(controller.LoadedDataView, "La vista previa se expone incluso con cero filas.");
        Assert.AreEqual(0, controller.LoadedDataView.Count);
        Assert.IsFalse(controller.CanImport, "Con cero filas el import queda deshabilitado (guard de cero filas).");
    }

    [TestMethod]
    public async Task Controller_Load_UsesCallerOwnedTable_WithoutCloning()
    {
        var table = new DataTable();
        table.Columns.Add("Nombre");
        table.Columns.Add("Edad");
        table.Rows.Add("Ana", "abc");
        var reader = new TestDoubles.FakeDataSourceReader(table);
        var controller = new ImportWizardController(CreateSampleDefinitions());

        try
        {
            var loaded = await controller.LoadAsync(reader);

            Assert.IsTrue(loaded);
            Assert.AreSame(table, controller.LoadedDataView?.Table, "La tabla provista se adopta sin clonar.");

            var isValid = controller.Revalidate();

            Assert.IsFalse(isValid, "La fila con Edad='abc' no es convertible a Int32 y debe invalidar.");
            Assert.IsTrue(table.Rows[0].HasErrors, "Los errores de validacion deben quedar pintados sobre la tabla provista.");
            Assert.IsNull(controller.ResultData, "Sin validacion exitosa no hay resultado tipado.");

            var wasDisposed = false;
            table.Disposed += (_, _) => wasDisposed = true;

            controller.Dispose();

            Assert.IsFalse(wasDisposed, "Dispose del orquestador no debe disponer la tabla del llamador.");
        }
        finally
        {
            table.Dispose();
        }
    }

    [TestMethod]
    public async Task Debounce_ScheduleThenElapsed_RevalidatesOnceSilently()
    {
        var table = CreateSampleTable();
        var fakeTimer = new TestDoubles.FakeDebounceTimer();
        using var controller = new ImportWizardController(CreateSampleDefinitions(), debounceTimer: fakeTimer);
        await controller.LoadAsync(new TestDoubles.FakeDataSourceReader(table));
        Assert.IsTrue(controller.Revalidate(), "Estado previo: datos validos.");
        Assert.IsNotNull(controller.ResultData, "Estado previo: hay resultado tipado.");
        var loadFailedCount = 0;
        controller.LoadFailed += (_, _) => loadFailedCount++;

        table.Rows[0]["Edad"] = "abc";
        controller.ScheduleSilentRevalidation();

        Assert.IsTrue(fakeTimer.IsRunning, "El debounce debe quedar programado tras la edicion.");
        Assert.AreEqual(
            ImportWizardController.DefaultDebounceIntervalMilliseconds,
            fakeTimer.LastIntervalMilliseconds,
            "El intervalo por omision es el de 400 ms.");

        fakeTimer.Fire();

        Assert.AreEqual(1, fakeTimer.FireCount, "Una ventana de estabilizacion → un solo disparo.");
        Assert.IsTrue(table.Rows[0].HasErrors, "La revalidacion silenciosa pinta el error sobre la tabla.");
        Assert.IsNull(controller.ResultData, "El resultado tipado se retira al invalidarse los datos.");
        Assert.AreEqual(0, loadFailedCount, "La revalidacion silenciosa no reporta fallo de carga.");
        Assert.IsFalse(fakeTimer.IsRunning, "Tras el disparo no queda ventana activa (sin repeticiones).");
    }

    [TestMethod]
    public async Task Debounce_ExplicitValidation_CancelsPending()
    {
        var table = CreateSampleTable();
        var fakeTimer = new TestDoubles.FakeDebounceTimer();
        using var controller = new ImportWizardController(CreateSampleDefinitions(), debounceTimer: fakeTimer);
        await controller.LoadAsync(new TestDoubles.FakeDataSourceReader(table));
        Assert.IsTrue(controller.Revalidate(), "Estado previo: datos validos.");

        table.Rows[0]["Edad"] = "abc";
        controller.ScheduleSilentRevalidation();
        Assert.IsTrue(fakeTimer.IsRunning, "Estado previo: ventana pendiente programada.");

        var isValid = controller.Revalidate();

        Assert.IsFalse(isValid, "La edicion invalida los datos.");
        Assert.IsFalse(fakeTimer.IsRunning, "La validacion explicita cancela la ventana pendiente.");
        fakeTimer.Fire();
        Assert.AreEqual(0, fakeTimer.FireCount, "La ventana cancelada no dispara revalidacion alguna.");
    }

    [TestMethod]
    public async Task Debounce_SuccessiveSchedules_Coalesce()
    {
        var table = CreateSampleTable();
        var fakeTimer = new TestDoubles.FakeDebounceTimer();
        using var controller = new ImportWizardController(CreateSampleDefinitions(), debounceTimer: fakeTimer);
        await controller.LoadAsync(new TestDoubles.FakeDataSourceReader(table));
        Assert.IsTrue(controller.Revalidate(), "Estado previo: datos validos.");

        table.Rows[0]["Edad"] = "abc";
        controller.ScheduleSilentRevalidation();
        controller.ScheduleSilentRevalidation();
        controller.ScheduleSilentRevalidation();

        Assert.AreEqual(3, fakeTimer.StartCallCount, "Cada edicion sucesiva reinicia la ventana (coalescencia por reinicio).");
        Assert.IsTrue(fakeTimer.IsRunning, "La ultima edicion deja la ventana activa.");

        fakeTimer.Fire();

        Assert.AreEqual(1, fakeTimer.FireCount, "Las ediciones sucesivas coalescen en una sola revalidacion.");
        Assert.IsTrue(table.Rows[0].HasErrors);
        Assert.IsNull(controller.ResultData);
    }

    [TestMethod]
    public async Task OptionsChanged_MarksDataStale_AndDisablesImport()
    {
        var table = CreateSampleTable();
        var fakeTimer = new TestDoubles.FakeDebounceTimer();
        using var controller = new ImportWizardController(CreateSampleDefinitions(), debounceTimer: fakeTimer);
        await controller.LoadAsync(new TestDoubles.FakeDataSourceReader(table));
        Assert.IsTrue(controller.Revalidate(), "Estado previo: datos validos.");
        Assert.IsTrue(controller.CanImport, "Estado previo: importacion habilitada.");
        Assert.IsNotNull(controller.ResultData);

        controller.InvalidateData();

        Assert.IsTrue(controller.IsDataStale, "El cambio de configuracion marca los datos como obsoletos.");
        Assert.IsFalse(controller.CanImport, "Con datos obsoletos no se puede importar.");
        Assert.IsNull(controller.ResultData, "El resultado tipado se retira al invalidar.");
        Assert.IsFalse(fakeTimer.IsRunning, "La invalidacion no programa revalidacion: hay que recargar.");

        var reloaded = await controller.LoadAsync(new TestDoubles.FakeDataSourceReader(table));

        Assert.IsTrue(reloaded);
        Assert.IsFalse(controller.IsDataStale, "Recargar restablece la vigencia de los datos.");
        Assert.IsTrue(controller.CanImport, "Tras recargar vuelve a poder importarse.");
    }

    [TestMethod]
    public async Task Errors_AfterValidation_ExposeRowFieldMessage()
    {
        var table = CreateSampleTable();
        table.Rows[0]["Edad"] = "abc";
        using var controller = new ImportWizardController(CreateDisplayDefinitions());
        await controller.LoadAsync(new TestDoubles.FakeDataSourceReader(table));
        var errorsChangedCount = 0;
        controller.ErrorsChanged += (_, _) => errorsChangedCount++;

        var isValid = controller.Revalidate();

        Assert.IsFalse(isValid, "La fila con Edad='abc' es invalida.");
        var error = controller.RowErrors.Single(e => e.RowIndex == 0);
        Assert.AreEqual("Edad", error.FieldName, "La exposicion neutral transporta el FieldName interno.");
        Assert.AreEqual("Edad de la persona", error.FieldDisplayName, "El nombre visible se resuelve desde las definiciones sin mutarlas.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(error.Message), "El mensaje de validacion queda expuesto.");
        Assert.AreEqual(1, errorsChangedCount, "La revalidacion notifica el cambio en la exposicion de errores.");
    }

    [TestMethod]
    public async Task Errors_AfterCorrection_DisappearFromExposure()
    {
        var table = CreateSampleTable();
        table.Rows[0]["Edad"] = "abc";
        using var controller = new ImportWizardController(CreateDisplayDefinitions());
        await controller.LoadAsync(new TestDoubles.FakeDataSourceReader(table));
        var errorsChangedCount = 0;
        controller.ErrorsChanged += (_, _) => errorsChangedCount++;
        Assert.IsFalse(controller.Revalidate(), "Estado previo: hay errores.");
        Assert.AreEqual(1, controller.RowErrors.Count, "Estado previo: un error expuesto.");

        table.Rows[0]["Edad"] = "31";
        var isValid = controller.Revalidate();

        Assert.IsTrue(isValid, "Corregir el valor hace valida la tabla.");
        Assert.AreEqual(0, controller.RowErrors.Count, "Corregir y revalidar retira el fallo de la exposicion.");
        Assert.AreEqual(2, errorsChangedCount, "Cada revalidacion con cambio notifica a los observadores.");
    }

    [TestMethod]
    public async Task HasErrorColumn_FedByController_DrivesViewFilter()
    {
        var table = CreateSampleTable();
        table.Rows[0]["Edad"] = "abc";
        using var controller = new ImportWizardController(CreateDisplayDefinitions());
        await controller.LoadAsync(new TestDoubles.FakeDataSourceReader(table));

        Assert.IsFalse(controller.Revalidate(), "Estado previo: hay una fila con error.");
        Assert.IsTrue(table.Columns.Contains(ImportWizardController.HasErrorColumnName), "El orquestador crea la columna _HasError.");
        Assert.AreEqual(true, (bool)table.Rows[0][ImportWizardController.HasErrorColumnName]);
        Assert.AreEqual(false, (bool)table.Rows[1][ImportWizardController.HasErrorColumnName]);

        var errorView = new DataView(
            table,
            $"[{ImportWizardController.HasErrorColumnName}] = true",
            string.Empty,
            DataViewRowState.CurrentRows);

        Assert.AreEqual(1, errorView.Count, "El filtro devuelve solo filas con error.");
        Assert.AreEqual("Ana", (string)errorView[0]["Nombre"]);

        table.Rows[0]["Edad"] = "31";
        Assert.IsTrue(controller.Revalidate(), "Corregida la fila, la tabla es valida.");

        Assert.AreEqual(0, errorView.Count, "Corregida la fila, el filtro _HasError ya no la devuelve.");
        Assert.AreEqual(false, (bool)table.Rows[0][ImportWizardController.HasErrorColumnName]);
    }

    private static FieldDefinitionCollection CreateDisplayDefinitions()
    {
        var definitions = new FieldDefinitionCollection();
        definitions.Add("Nombre", displayName: "Nombre completo", sourceColumnName: "Nombre");
        definitions.Add("Edad", displayName: "Edad de la persona", sourceColumnName: "Edad", fieldType: typeof(int));
        return definitions;
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
        table.Columns.Add("Nombre");
        table.Columns.Add("Edad");
        table.Rows.Add("Ana", "30");
        table.Rows.Add("Luis", "25");
        return table;
    }
}
