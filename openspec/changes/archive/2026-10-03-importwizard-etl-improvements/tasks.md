# Tasks

> Verificación por tarea: cada tarea TDD sigue el ciclo test-fallido → implementación → test-verde, con `dotnet build KUtilitiesCore.sln` (las warnings son el gate) y `dotnet test --solution KUtilitiesCore.sln` (MTP, SDK 10). Tests dirigidos con `dotnet test <Proyecto> --filter "FullyQualifiedName~<Test>"`.
>
> Nota de aplicabilidad (docs\openspec-tasks-mandatory-steps.md): proyecto tipo librería sin base de datos, sin API HTTP y sin frontend web → NO se incluyen verificación de BD, pruebas con curl (N+2) ni Playwright E2E (N+3).

## 1. Paso 0: Crear rama de la característica (OBLIGATORIO - PRIMER PASO)

- [x] 1.1 Crear la rama `feature/importwizard-etl-improvements` a partir de la rama base y cambiarse a ella; verificar con `git status` y `git branch --show-current`
- [x] 1.2 Revisar `git log --oneline -10` para imitar el estilo de mensajes de commit del repositorio antes del primer commit

## 2. ImportManager: ReadData sin clon (D1)

- [x] 2.1 Escribir tests fallidos en `KUtilitiesCore.DataTests\DataImporter\ImportManagerTests.cs`: `ReadData_PaintsColumnErrors_OnProvidedTable` (tabla provista con valor no convertible "25x" en columna int → tras `ValidateDataTypes`, `raw.Rows[1].HasErrors` es true y `raw.Rows[0].HasErrors` es false) y `ValidateDataTypes_AppliesDefaultValue_OnProvidedTable` (def int `AllowNull=false` con `DefaultValue=0` y celda vacía → `raw.Rows[0]["Edad"]` queda "0" tras validar); verificar que fallan con `dotnet test KUtilitiesCore.DataTests --filter "FullyQualifiedName~ReadData_PaintsColumnErrors_OnProvidedTable"` (y el segundo)
- [x] 2.2 Implementar en `KUtilitiesCore.Data\DataImporter\ImportManager.cs`: `ReadData` usa la tabla provista directamente (`ArgumentNullException.ThrowIfNull(rawData); _rawDataSource = rawData;`), sin clonar; quitar `_rawDataSource.Dispose()` de `Dispose(bool)` (propiedad del llamador); doc comment XML en español explicando por qué se descartó el clon; verificar ambos tests en verde
- [x] 2.3 Ejecutar TODA la suite de DataTests (`dotnet test KUtilitiesCore.DataTests`) y confirmar que `MultiSourceImportManagerTests` sigue en verde; corregir solo si el contrato cambiado lo exige

## 3. ImportManager: semántica coherente con datos vacíos

- [x] 3.1 Capturar baseline: ejecutar `dotnet test KUtilitiesCore.DataTests --filter "FullyQualifiedName~ImportManager_EmptyLoad_IsValid_Test"` y registrar su resultado actual antes de tocar código
- [x] 3.2 Escribir test fallido `ValidateDataTypes_EmptyTable_ReturnsFalse` (esquema mapeado con cero filas → retorno `false` y `ValidationErrors.IsValid` false); verificar que falla
- [x] 3.3 Cambiar en `ImportManager.ValidateDataTypes` la rama de cero filas: `return true` → `return false`; verificar el test nuevo en verde y ajustar el baseline del 3.1 solo si su contrato cambia de forma intencional

## 4. ImportManager: CreateResultTable tipado (D3)

- [x] 4.1 Escribir test fallido `CreateResultTable_ReturnsTypedTable_WithoutControlColumns` (tabla resultado sin `_RowIndex`/`_IsValid`, columna Edad `typeof(int)`, valores convertidos, vacío con `DefaultValue` → valor por defecto, vacío sin `DefaultValue` → `DBNull`); verificar que falla
- [x] 4.2 Implementar `public DataTable CreateResultTable()` en `ImportManager`: columnas `def.FieldName` tipadas `def.TargetType ?? typeof(string)`; por fila: vacío → `DefaultValue` si `!def.AllowNull && def.DefaultValue != null`, si no `DBNull`; valor → `def.TypeConverter?.TryConvert(value) ?? DBNull.Value`; doc comment con `<example>`; verificar el test en verde

## 5. Wizard: ResultData tipado y propio (D3)

- [x] 5.1 Escribir test fallido `Import_ResultData_ShouldBeTypedAndOwn_Table` en `KUtilitiesCore.Data.WinTests\ImportWizardFormTests.cs` (tras importar, `ResultData` sin columnas de control, columna Age `typeof(int)`, `result.Rows[0]["Age"]` == 30, y sigue usable tras `form.Dispose()`); verificar que falla
- [x] 5.2 Implementar en `KUtilitiesCore.Data.Win\Importer\ImportWizardForm.cs`: en `ImportData` y `RevalidateAfterEdit` (rama éxito) asignar `ResultData = _importManager.CreateResultTable();`; en `OnDispose` disponer `loadedDataTable` y `ResultData` (ya tabla propia) además de `_importManager.Dispose()`; verificar el test en verde

## 6. Wizard: filtro DataView + columna _HasError (D2)

- [x] 6.1 Escribir test fallido `FilterHasErrors_ShouldShowOnlyErrorRows_AndEditsPropagate` (STATestMethod; tras importar con "25x": `LoadedDataTable.Rows[1].HasErrors` true; activar filtro → 1 fila visible; editar la celda en la vista filtrada y revalidar → `Rows[1].HasErrors` false; desactivar filtro → 2 filas); exponer en `TestableImportWizardForm` el helper `InvokeFilterHasErrors`; verificar que falla
- [x] 6.2 Implementar: en `OnLoadedDataSource` agregar columna `_HasError` (bool) y enlazar `dgvPreview.DataSource = new DataView(table)`; ocultar la columna `_HasError` en el grid; convertir `FilterHasErrors` a `protected virtual` usando `DataView(table, "[_HasError] = true", ...)` / `DataView(table)`; agregar `UpdateRowErrorFlags(DataTable)` y llamarlo tras `ValidateDataTypes` en `ImportData` y `RevalidateAfterEdit`; doc comment explicando por qué se descartó el clon (ImportRow copia y perdía ediciones); verificar el test en verde
- [x] 6.3 Ejecutar la suite completa `dotnet test KUtilitiesCore.Data.WinTests` y ajustar solo el wiring afectado por el binding a DataView

## 7. Wizard: CreateReader + dispose del lector (D5 parcial)

- [x] 7.1 Extraer `protected virtual IDataSourceReader? CreateReader()` (guards con `ShowMessage`: archivo vacío / configuración null; devuelve reader Excel o CSV según extensión con los mismos argumentos actuales) y `protected virtual void HandleLoadError(Exception)` (status strip "Error al leer el archivo." + mensaje de error); refactorizar `LoadData()` para usarlos y disponer el reader en `finally` (`(reader as IDisposable)?.Dispose()`); verificar con `dotnet build KUtilitiesCore.sln` sin warnings y la suite del wizard en verde (comportamiento observable existente no cambia)

## 8. Wizard: BuildActiveDefinitions no muta definiciones

- [x] 8.1 Escribir test fallido `BuildActiveDefinitions_ShouldNotMutate_OriginalDefinitions` (guardar `SourceColumnName` de las definiciones; tras cargar con mapeo a columna distinta e importar, las definiciones originales conservan su valor); verificar que falla
- [x] 8.2 Implementar en `BuildActiveDefinitions`: trabajar sobre `var def = (FieldDefinitionItem)originalDef.Clone();` (columna seleccionada si != "(Ignorar)" y no vacía; si no, `def.DisplayName` como nombre ficticio); verificar el test en verde

## 9. Wizard: guard de archivo sin filas
- [x] 9.1 Escribir test fallido `Import_EmptyFile_ShouldWarn_AndNotExposeErrorsGridNoise` con helpers `GetEmptyDataTable()` (columnas Nombre/Edad string, 0 filas) y `SimulateEmptyLoadData()` (assert: mensaje contiene "no contiene filas" y el grid de errores tiene 0 filas); verificar que falla

- [x] 9.2 Implementar: en `OnLoadedDataSource`, `btnImport.Enabled = table.Rows.Count > 0` y warning visible si 0 filas; en `ImportData`, guard `if (sourceTable.Rows.Count == 0) { ShowMessage(..., Warning); return; }`; verificar el test en verde

## 10. Controles de configuración: hoja Excel y opciones de parsing

- [x] 10.1 Crear `KUtilitiesCore.Data.WinTests\ImportConfigControlTests.cs` ([TestClass] sealed, STATestMethod) y escribir tests fallidos: `ExcelConfig_GetParsingOptions_ShouldReturnSelectedSheet` (con hooks `AddSheetForTesting`/`SelectSheetForTesting`: Hoja1/Hoja2, seleccionar la segunda → `SheetName == "Hoja2"`), `CsvConfig_GetParsingOptions_ShouldExpose_TrimAndEmptyLines`, `ExcelConfig_GetParsingOptions_ShouldExpose_RowRange` (StartRow 3, EndRow 10; EndRow 0 → null); verificar que fallan
- [x] 10.2 Corregir `ExcelConfigControl.GetParsingOptions`: `cboSheet.SelectedItem?.ToString()` en lugar de `SelectedText`; agregar hooks `[EditorBrowsable(EditorBrowsableState.Never)]`; verificar el test de hoja en verde
- [x] 10.3 Agregar en `CsvConfigControl` (Designer + código): `chkTrimValues` ("Quitar espacios en los valores", default true) y `chkIgnoreEmptyLines` ("Ignorar líneas vacías", default true), mapear en `GetParsingOptions` y elevar `CheckedChanged` → `OnOptionsChanged`; seguir el patrón Designer existente (fields abajo, SuspendLayout); verificar el test CSV en verde
- [x] 10.4 Agregar en `ExcelConfigControl` (Designer + código): `numStartRow` (Min=1, Value=1) y `numEndRow` (Min=0, Value=0; 0 = todas) con labels "Fila inicial:" / "Fila final (0 = todas):", mapear en `GetParsingOptions` (`EndRow = numEndRow.Value > 0 ? (int)numEndRow.Value : null`), elevar `ValueChanged` → `OnOptionsChanged` y agregar hook `SetRowRangeForTesting`; verificar el test de rango en verde

## 11. Wizard: carga asíncrona no reentrante (D5)

- [x] 11.1 Escribir test fallido `[TestMethod] async Task LoadDataAsync_ShouldLoad_WithoutBlockingCaller` en `ImportWizardFormTests` usando `TestableImportWizardForm` con `UseStubReader = true` (`StubDataSourceReader` privado que implementa `IDataSourceReader` devolviendo la tabla de muestra); verificar que falla (no existe `LoadDataAsync`)
- [x] 11.2 Implementar `public virtual async Task LoadDataAsync()` (igual que `LoadData` pero `await reader.ReadDataAsync().ConfigureAwait(true)`, reutilizando `CreateReader`/`HandleLoadError` y dispose); convertir `btnLoadData_Click` a `async void` con guard `_isLoading` y deshabilitación del botón; mantener `LoadData()` síncrono virtual; verificar el test en verde y la suite del wizard

## 12. Wizard: revalidación silenciosa con debounce (D4)

- [x] 12.1 Escribir test fallido `MappingChange_ShouldTrigger_SilentRevalidationPath` (tras cargar datos, `SimulateMappingChange()` programa revalidación sin mostrar diálogo: `LastMessageShown` sigue null); verificar que falla
- [x] 12.2 Implementar: `System.Windows.Forms.Timer _revalidateDebounce` (Interval = 400) cuyo Tick ejecuta `RevalidateAfterEdit()`; `protected void ScheduleSilentRevalidation()` (Stop+Start) llamado desde `dgvPreview_CellEndEdit` y el nuevo `dgvMapping_CellValueChanged` (guard `_populatingMapping` + `loadedDataTable is null`); wire del evento en Designer; `_revalidateDebounce.Stop()` al inicio de `ImportData`; flag `_populatingMapping` alrededor del cuerpo de `PopulateMappingGrid`; doc comment de la alternativa descartada (validación background: DataTable enlazado no es thread-safe); verificar el test nuevo en verde y el test existente de revalidación por celda

## 13. Wizard: invalidación por OptionsChanged (D6)

- [x] 13.1 Escribir test fallido `OptionsChanged_ShouldMarkDataStale_AndDisableImport` (tras `LoadData`: botón importar habilitado; `SimulateOptionsChanged()` → botón deshabilitado, warning visible, `ResultData` null); verificar que falla
- [x] 13.2 Implementar: suscripción `configControl.OptionsChanged += OnConfigOptionsChanged;` al final de `OnSelectedFile`; `protected virtual void OnConfigOptionsChanged` (guard `loadedDataTable is null` return; invalida `ResultData`, deshabilita `btnImport`, warning "La configuración de análisis cambió: recargue los datos."); exponer `SimulateOptionsChanged` en `TestableImportWizardForm`; doc comment explicando por qué se descartó la recarga automática; verificar el test en verde

## 14. Higiene y presentación coherente

- [x] 14.1 Escribir test fallido `ErrorsGrid_ShouldShowDisplayName_ForFieldName` (definición con regla y `DisplayName` "Edad"; tras importar con error, la columna Campo del grid de errores muestra "Edad"); escribir test fallido `FileName_Empty_ShouldResetWizardState` (asignar `FileName = string.Empty` → preview/resultado limpios y botones deshabilitados); verificar que fallan
- [x] 14.2 Unificar `PropertyName` en `ImportManager.ValidateRow`: `ValidationFailure(def.FieldName, ...)` (líneas ~364/385) y en `RefreshErrorsGrid` resolver `DisplayName` (`_fieldDefinitions.FirstOrDefault(f => f.FieldName == failure.PropertyName)?.DisplayName ?? failure.PropertyName`, "General" para fallos sin propiedad); verificar el test de DisplayName en verde
- [x] 14.3 Usar el retorno bool de `ValidateDataTypes()` en `ImportData`/`RevalidateAfterEdit`; eliminar `Application.DoEvents()` de `OnSelectedFile`, el using `System.Security.Cryptography` (línea 9), el comentario muerto (línea 54) y el null-check redundante en `OnProcessImportFinished`; implementar `ResetWizardState()` invocado cuando `FileName` se asigna vacío (limpia config, preview, resultado, mapeo, botones y status strip); verificar el test de reset en verde

## 15. Paso N: Revisar y actualizar pruebas unitarias existentes (OBLIGATORIO)

- [x] 15.1 Revisar los baselines capturados (3.1) y los tests de contrato afectados por T1/T2 (`ImportManagerTests`, `MultiSourceImportManagerTests`, `ImportWizardFormTests`); ajustar ÚNICAMENTE los que cambian por contrato intencional y documentar en el cuerpo del test el porqué; verificar con `dotnet test --solution KUtilitiesCore.sln` en verde

## 16. Paso N+1: Ejecutar pruebas unitarias y generar reporte (OBLIGATORIO)

- [x] 16.1 Ejecutar las pruebas específicas de los módulos modificados: `dotnet test KUtilitiesCore.DataTests` y `dotnet test KUtilitiesCore.Data.WinTests`; registrar conteos de aprobadas/fallidas/omitidas
- [x] 16.2 Ejecutar la suite completa obligatoria: `dotnet test --solution KUtilitiesCore.sln`; registrar conteo total, fallos, tiempo y comportamientos inestables (nota: `GitHubUpdateServiceTest` falla 401 sin `GITHUB_TOKEN` — preexistente, no regresión)
- [x] 16.3 Crear el reporte `openspec\changes\importwizard-etl-improvements\reports\AAAA-MM-DD-paso-N+1-verificacion-de-pruebas-unitarias.md` con los comandos ejecutados, resultados resumidos y resultado del paso (sin sección de base de datos: el proyecto no usa BD)
- [x] 16.4 Marcar este paso como completado solo después de que las pruebas pasen y el reporte exista

## 17. Paso N+4: Actualizar documentación técnica (OBLIGATORIO)

- [x] 17.1 Revisar y completar los XML doc comments en español de todas las APIs públicas cambiadas (`ReadData`, `CreateResultTable`, `LoadDataAsync`, `FilterHasErrors`, `CreateReader`, `HandleLoadError`, `OnConfigOptionsChanged`, hooks), explicando propósito y porqué (semántica de propiedad, alternativas descartadas); verificar con `dotnet build KUtilitiesCore.sln` sin warnings (el build es el gate de docs)
- [x] 17.2 Confirmar que la documentación de arquitectura relevante (AGENTS.md, sección de notas si aplica) refleja el nuevo contrato de `ReadData` y el comportamiento de `ResultData`; verificar que el build y la suite completa siguen en verde
