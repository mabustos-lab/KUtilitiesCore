# Tasks

> Verificación por tarea: cada tarea TDD sigue el ciclo test-fallido → implementación → test-verde, con `dotnet build KUtilitiesCore.sln` (0 warnings nuevos es el gate) y `dotnet test --solution KUtilitiesCore.sln` (MTP, SDK 10). Tests dirigidos con `dotnet test <Proyecto> --filter "FullyQualifiedName~<Test>"`.
>
> Nota de aplicabilidad (docs\openspec-tasks-mandatory-steps.md): proyecto tipo librería sin base de datos, sin API HTTP y sin frontend web → NO se incluyen verificación de BD, pruebas con curl (N+2) ni Playwright E2E (N+3). El front-end WPF es escritorio, no navegador.
>
> Referencias (D1–D9): ver `design.md`. Nombre del cambio: `wpf-import-wizard`.

## 1. Paso 0: Crear rama de la característica (OBLIGATORIO - PRIMER PASO)

- [x] 1.1 Crear la rama `feature/wpf-import-wizard` a partir de la rama de integración (`Dev-NET10`) y cambiarse a ella; verificar con `git status` y `git branch --show-current`
- [x] 1.2 Revisar `git log --oneline -10` para imitar el estilo de mensajes de commit del repositorio antes del primer commit

## 2. Scaffolding: proyectos `KUtilitiesCore.Data.Wizard` y `KUtilitiesCore.Data.WPFTests` (D1, D8)

- [x] 2.1 Crear `KUtilitiesCore.Data.Wizard` (`net10.0`, `Nullable enable`, `ImplicitUsings enable`, `GenerateDocumentationFile True`, sin `UseWindowsForms`/`UseWPF`), referencia a `KUtilitiesCore.Data`, agregar al `.sln`; verificar con `dotnet build KUtilitiesCore.sln` (0 errores, 0 warnings nuevos)
- [x] 2.2 Crear `KUtilitiesCore.Data.WPFTests` (`net10.0-windows`, MSTest 4.4.1 + Microsoft.NET.Test.Sdk 18.10.1 + `EnableMSTestRunner` true, como `Data.WinTests`), referencias a `KUtilitiesCore.Data.Wizard` y `KUtilitiesCore.Data`, agregar al `.sln`; verificar con `dotnet build KUtilitiesCore.sln` y un `[TestClass]` vacío en verde con `dotnet test KUtilitiesCore.Data.WPFTests`

## 3. Orquestador: estado, carga y reset (D2, D3) — TDD

- [x] 3.1 Escribir tests fallidos en `KUtilitiesCore.Data.WPFTests\Wizard\ImportWizardControllerTests.cs`: `Controller_AfterLoad_ExposesLoadedDataView_AndStopsLoading` (inyectado un lector fake que devuelve una tabla muestra, `LoadAsync` completa → `LoadedDataView` observable, `IsLoading` vuelve a false), `Controller_SecondLoadWhileLoading_IsRejected` (segunda `LoadAsync` durante la primera no inicia nueva lectura; la primera completa), `Controller_EmptyFileName_ResetsState` (asignar ruta vacía → preview/resultado/botones en estado inicial); verificar que fallan con `dotnet test KUtilitiesCore.Data.WPFTests --filter "FullyQualifiedName~ImportWizardControllerTests"` (los tipos no existen)
- [x] 3.2 Implementar en `Data.Wizard`: contratos `IWizardDebounceTimer` (Start/Stop + evento Elapsed) e `IImportConfigControl` compartido (`Initialize`/`GetParsingOptions`/`OptionsChanged`, namespace propio), y `ImportWizardController` con estado (`FileName`, `LoadedDataView`, `IsLoading`, `ResultData`, `CanImport`), `Load`/`LoadAsync` (espera `reader.ReadDataAsync()` sin `ConfigureAwait(false`), guard cero filas y reset por ruta vacía; XML docs en español con el contrato de afinidad de hilo; verificar los tests en verde
- [x] 3.3 Escribir test fallido `Controller_Load_UsesCallerOwnedTable_WithoutCloning` (tabla provista adoptada: los errores quedan pintados sobre ella y `Dispose` del orquestador no la dispone); verificar que falla y luego en verde tras reutilizar `ImportManager.ReadData` sin clonar en el orquestador

## 4. Orquestador: debounce inyectable e invalidación (D3, D6) — TDD

- [x] 4.1 Implementar `FakeDebounceTimer` en `Data.WPFTests` (disparo manual del `Elapsed`); escribir tests fallidos: `Debounce_ScheduleThenElapsed_RevalidatesOnceSilently` (programar revalidación + disparar el fake → exactamente una revalidación, sin mensajes), `Debounce_ExplicitValidation_CancelsPending` (validación explícita → el fake disparado después ya no revalida), `Debounce_SuccessiveSchedules_Coalesce` (varias programaciones reinician la ventana → un solo disparo efectivo), `OptionsChanged_MarksDataStale_AndDisablesImport` (cambio de opciones con datos cargados → `CanImport` false, `ResultData` null, `IsDataStale` true hasta recarga); verificar que fallan
- [x] 4.2 Implementar en el orquestador: `ScheduleSilentRevalidation()` sobre `IWizardDebounceTimer` (Stop+Start, intervalo por defecto 400 ms configurable), revalidación silenciosa, invalidación por `OptionsChanged` y eventos neutrales (`LoadCompleted`, `LoadFailed`, `ImportCompleted`); verificar los tests en verde

## 5. Orquestador: exposición neutral de errores y helpers `_HasError` (D5) — TDD

- [x] 5.1 Escribir tests fallidos: `Errors_AfterValidation_ExposeRowFieldMessage` (tabla con valor no convertible → la exposición neutral contiene (fila, DisplayName, mensaje)), `Errors_AfterCorrection_DisappearFromExposure` (corregir + revalidar → el fallo desaparece), `HasErrorColumn_FedByController_DrivesViewFilter` (el orquestador crea/actualiza `_HasError` y el filtro `[_HasError] = true` devuelve solo filas con error); verificar que fallan
- [x] 5.2 Implementar: migrar al orquestador `EnsureHasErrorColumn`/`UpdateRowErrorFlags` (hoy en `ImportWizardForm`) y la proyección de errores (derivada de `ValidationErrors`, con `FieldName`→`DisplayName` resuelto sin mutar definiciones); mantener `BuildActiveDefinitions` sobre clones; verificar los tests en verde y `dotnet build KUtilitiesCore.sln` sin warnings

## 6. Refactor WinForms sobre el orquestador (D9)

- [x] 6.1 En `Data.Win`: `WinFormsDebounceTimer` (envuelve `System.Windows.Forms.Timer`) y hacer que `IImportConfigControl` de Data.Win herede del contrato compartido de `Data.Wizard`; verificar con `dotnet build KUtilitiesCore.sln` (las implementaciones `CsvConfigControl`/`ExcelConfigControl` compilan sin cambios de firma)
- [x] 6.2 Refactorizar `ImportWizardForm` para delegar en `ImportWizardController` (estado, `LoadDataAsync`, debounce, invalidación, reset, guard cero filas, helpers `_HasError`, clones de definiciones), conservando la superficie pública (`ctor`, `FileName`, `ResultData`, `AutoCloseOnSuccess`, `LoadedDataTable`, `LoadDataAsync`) y solo la interacción de vista (pintado, diálogos, `MessageBox`, `CommitPendingGridEdit`); los hooks de `TestableImportWizardForm` deben seguir compilando; verificar con `dotnet test KUtilitiesCore.Data.WinTests` en verde SIN cambios de comportamiento observable
- [x] 6.3 Ejecutar la suite WinForms completa tras cada extracción parcial (estado → carga → debounce → invalidación → errores) y ajustar solo el wiring afectado; dejar la suite en verde antes de pasar al grupo siguiente

## 7. Exportación DRY (D7) — TDD

- [x] 7.1 Escribir tests fallidos en `KUtilitiesCore.DataTests\DataExporter\ExportDialogPreparationTests.cs`: `ValidateSource_NullOrEmptyTable_ReturnsFalse`, `GetDefaultFileName_FollowsTimestampPattern` (prefijo `Exportacion_` + `yyyyMMdd_HHmmss`), `ResolveFormat_CsvTsvPsvXlsxXls_MapsToExporter` y `ResolveFormat_UnknownExtension_ReturnsNone`; verificar que fallan con `dotnet test KUtilitiesCore.DataTests --filter "FullyQualifiedName~ExportDialogPreparationTests"`
- [x] 7.2 Implementar `ExportDialogPreparation` público en `KUtilitiesCore.Data.DataExporter` (validación, nombre por defecto, `FileDialogFilter` constante, resolución extensión→formato) extrayendo la lógica de `Exporter.cs`; reducir `Exporter` de Data.Win a fachada (`SaveFileDialog`) delegando en la preparación, conservando su firma pública; verificar `dotnet test KUtilitiesCore.DataTests` y `dotnet test KUtilitiesCore.Data.WinTests` en verde

## 8. Front-end WPF: proyecto y ViewModels (D2, D8) — TDD

- [x] 8.1 Crear `KUtilitiesCore.Data.WPF` (`net10.0-windows`, `UseWPF`, `Nullable enable`, `GenerateDocumentationFile True`, SIN `UseWindowsForms` ni referencias WinForms), referencias a `Data.Wizard` + `Data` + `KUtilitiesCore.MVVM`, agregar al `.sln`; verificar con `dotnet build KUtilitiesCore.sln` y con `openspec show "import-wizard"`/inspección del csproj el escenario "Inspección del proyecto WPF" (declara `UseWPF`, no `UseWindowsForms`)
- [x] 8.2 Escribir tests fallidos en `KUtilitiesCore.Data.WPFTests\WPF\ImportWizardViewModelTests.cs`: `ViewModel_Commands_ExecuteAgainstController` (Browse/Load/Import delegan en el orquestador con `FakeDebounceTimer`), `ViewModel_RaisesPropertyChanged_OnControllerEvents` (`LoadCompleted` → `PropertyChanged` de `LoadedDataView`/`IsLoading`), `ViewModel_CanImport_MirrorsStaleAndValidState` (datos obsoletos o inválidos → comando Import deshabilitado), `CsvOptions_ExposeTrimAndIgnoreEmptyLines`, `ExcelOptions_ExposeSheetAndRowRange` (EndRow 0 → "hasta el final"/null); verificar que fallan
- [x] 8.3 Implementar ViewModels con `KUtilitiesCore.MVVM` (`ImportWizardViewModel` sobre `ImportWizardController` con `ViewModelHelperBase`/`RelayCommand`; `CsvConfigViewModel` con TrimValues/IgnoreEmptyLines; `ExcelConfigViewModel` con hoja seleccionada + StartRow/EndRow opcional 0, evento `OptionsChanged` hacia el orquestador); XML docs en español; verificar los tests en verde

## 9. Front-end WPF: ventana, controles, errores por celda y mensajería (D5, D6)

- [x] 9.1 XAML: `ImportWizardWindow` (modal, estilo por defecto de WPF), `CsvConfigControl`/`ExcelConfigControl` (UserControls WPF con las opciones de los VM), grid de mapeo y grid de errores con `DisplayName` resuelto (paridad con WinForms); verificar con `dotnet build KUtilitiesCore.sln`
- [x] 9.2 Extraer la lógica pura de señalización a un método testeable (DataRow → descriptores de (columna, mensaje) leyendo `RowError`/`GetColumnError`) y escribir el test `RowErrorDescriptor_MapsColumnErrors_FromDataRow` en verde; implementar el attached behavior de WPF que aplica estilos de error (borde + tooltip) sobre el `DataGrid` enlazado al `DataView` usándola
- [x] 9.3 Implementar `DispatcherDebounceTimer` (envuelve `DispatcherTimer`) y el commit de edición: forzar `CommitEdit`/`EndCurrentEdit` equivalente antes de cada revalidación (paridad con `CommitPendingGridEdit`); vista previa con `DataView` y filtro `[_HasError] = true` con checkbox "Mostrar solo filas con errores"; verificar con `dotnet test KUtilitiesCore.Data.WPFTests` en verde (tests de VM/capa) y build sin warnings
- [x] 9.4 Implementar `Exporter` WPF (`Microsoft.Win32.SaveFileDialog`) delegando en `ExportDialogPreparation` + `DataExporter` existentes (fachada delgada, sin lógica propia); verificar con `dotnet build KUtilitiesCore.sln` y que `Data.WinTests`/`DataTests` siguen en verde
- [x] 9.5 Escribir tests fallidos en `KUtilitiesCore.Data.WPFTests\WPF\MessageDialogModelTests.cs`: `MessageDialogModel_FromArgs_MapsHeadingFootnoteExpander` (mapeo puro `MessageArgs` → modelo de diálogo: `Caption`/`Heading`/`Message`/`Footnote`/`Expander`/`MessageIcon`) y `MessageDialogModel_EmptyOptionals_AreCollapsed` (sin footnote/expander → el modelo indica que las secciones se omiten); añadir también `ViewModel_LoadFailed_RoutesToMessageService` con un fake de `ISupportMessageService` (el VM notifica el fallo con los detalles técnicos en el expander); verificar que fallan
- [x] 9.6 Implementar `WpfMessageService` (`ISupportMessageService` de `KUtilitiesCore.MVVM`; `ValueTask` completado de forma síncrona tras `ShowDialog` modal — contrato documentado en XML docs) con `MessageDialogWindow`, diálogo XAML propio de anatomía TaskDialog (encabezado de fuente grande, texto, pie de página, sección desplegable de detalles técnicos, icono por `MessageIcon`, botones según `MessageType`/`ConfirmAsync`); integrarlo en `ImportWizardViewModel` para fallos de carga y resultado de importación; XML docs en español; verificar `dotnet test KUtilitiesCore.Data.WPFTests` en verde y build sin warnings

## 10. Paso N: Revisar y actualizar pruebas unitarias existentes (OBLIGATORIO)

- [x] 10.1 Revisar los baselines afectados por la extracción (`ImportWizardFormTests`, tests de `Exporter` en `DataTests` si existen, `ImportConfigControlTests`); ajustar ÚNICAMENTE los que cambian por contrato intencional (herencia del `IImportConfigControl` compartido, fachada de exportación) documentando el porqué en el cuerpo del test; verificar con `dotnet test --solution KUtilitiesCore.sln` en verde

## 11. Paso N+1: Ejecutar pruebas unitarias y generar reporte (OBLIGATORIO)

- [x] 11.1 Ejecutar las pruebas específicas de los módulos modificados: `dotnet test KUtilitiesCore.DataTests`, `dotnet test KUtilitiesCore.Data.WinTests` y `dotnet test KUtilitiesCore.Data.WPFTests`; registrar conteos de aprobadas/fallidas/omitidas
- [x] 11.2 Ejecutar la suite completa obligatoria: `dotnet build KUtilitiesCore.sln --no-incremental` (0 errores, 0 warnings atribuibles a las 14 librerías) y `dotnet test --solution KUtilitiesCore.sln`; registrar conteo total, fallos, tiempo y comportamientos inestables (nota: `GitHubUpdateServiceTest` falla 401 sin `GITHUB_TOKEN` — preexistente, no regresión)
- [x] 11.3 Crear el reporte `openspec\changes\wpf-import-wizard\reports\AAAA-MM-DD-paso-N+1-verificacion-de-pruebas-unitarias.md` con los comandos ejecutados, resultados resumidos y resultado del paso (sin sección de base de datos: el proyecto no usa BD)
- [x] 11.4 Marcar este paso como completado solo después de que las pruebas pasen y el reporte exista

## 12. Paso N+4: Actualizar documentación técnica (OBLIGATORIO)

- [x] 12.1 Revisar y completar los XML doc comments en español de todas las APIs públicas nuevas y cambiadas (`ImportWizardController`, `IWizardDebounceTimer`, `IImportConfigControl` compartido, `ExportDialogPreparation`, VMs, fachadas WPF, attached behavior, `WpfMessageService` y `MessageDialogWindow` — incluir el contrato de `ValueTask` síncrono y el porqué del diálogo propio frente a TaskDialog de WinForms): propósito y porqué (semántica de propiedad, alternativas descartadas, contrato de afinidad de hilo) y `<example>` donde el uso no sea obvio; verificar con `dotnet build KUtilitiesCore.sln` sin warnings CS1591/CS1570 (el build es el gate de docs)
- [ ] 12.2 Actualizar `AGENTS.md`: tabla de proyectos (12 → 14 librerías; 19 → 22 `.csproj`; `Data.Wizard` net10.0, `Data.WPF` y `Data.WPFTests` net10.0-windows), sección Testing (nueva suite `KUtilitiesCore.Data.WPFTests`), y notas de arquitectura (capa compartida del asistente, contrato de orquestación, gotchas: `FakeDebounceTimer`, STA solo si se testa la ventana); verificar que el build y la suite completa siguen en verde

## Workflow follow-up

- Al archivar (`/opsx-archive`): sincronizar los deltas a los specs principales y editar directamente el `Purpose` de `build-hygiene` ("las 12 librerías" → 14) en `openspec\specs\build-hygiene\spec.md` — los deltas no pueden modificar `Purpose`.
- Patrón de commits del repo: primero `feat(<scope>)` con implementación + tests + `AGENTS.md`; luego `docs(openspec): archivar cambio wpf-import-wizard y sincronizar specs` (no mezclar código y bookkeeping).
- PR con `gh` hacia la base `Dev-NET10`, título `feat: asistente de importacion ETL para WPF con capa compartida (wpf-import-wizard)`, cuerpo en español con secciones Resumen / detalle de contrato / Pruebas / Documentacion.
