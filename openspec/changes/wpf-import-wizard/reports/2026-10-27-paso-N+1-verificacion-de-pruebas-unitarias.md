# Paso N+1 — Verificación de pruebas unitarias

**Cambio:** `wpf-import-wizard`
**Rama:** `feature/wpf-import-wizard` (desde `Dev-NET10`)
**Fecha:** 2026-10-27
**Nota:** El cambio es una biblioteca de clases (orquestación compartida + front-ends WinForms/WPF) sin base de datos, sin API HTTP propia y sin frontend web: los pasos de verificación de BD (N+2) y E2E con Playwright (N+3) no aplican y se omiten según `docs/openspec-tasks-mandatory-steps.md`.

## Comandos ejecutados

### 1. Suites de los módulos modificados

```
dotnet test KUtilitiesCore.DataTests
dotnet test KUtilitiesCore.Data.WinTests
dotnet test KUtilitiesCore.Data.WPFTests
```

**Resultados:**

| Proyecto | Total | Correctas | Fallidas | Omitidas | Resultado |
|---|---|---|---|---|---|
| KUtilitiesCore.DataTests | 42 | 42 | 0 | 0 | Correcta (EXIT=0, 4s 052ms) |
| KUtilitiesCore.Data.WinTests | 26 | 26 | 0 | 0 | Correcta (EXIT=0, 5s 062ms) |
| KUtilitiesCore.Data.WPFTests | 22 | 22 | 0 | 0 | Correcta (EXIT=0, 1s 910ms) |

### 2. Suite completa obligatoria

```
dotnet test --solution KUtilitiesCore.sln
```

**Resultado:** `total: 225 / correcto: 223 / error: 1 / omitido: 1 / duración: 18s 644ms`

| Proyecto | Resultado |
|---|---|
| KUtilitiesCoreTests | correcto |
| KUtilitiesCore.DataTests (42) | correcto |
| KUtilitiesCore.Data.WinTests (26) | correcto |
| KUtilitiesCore.Data.WPFTests (22, nuevo) | correcto |
| KUtilitiesCore.DataAccessTests | correcto |
| KUtilitiesCore.MVVMTests | correcto |
| KUtilitiesCore.MVVMTests.EventCommandBinder | correcto |
| KUtilitiesCore.LoggerTests | correcto (1 omitido: `CreateLogger_And_LogMessage_ToSql` requiere SQL Server local configurado — preexistente) |
| KUtilitiesCore.GitHubUpdaterTests | 1 error: `GitHubUpdateServiceTest` falla con 401 (Unauthorized) al consultar la API en vivo de GitHub sin `GITHUB_TOKEN` en el entorno — **falla preexistente documentada, no una regresión** (ver AGENTS.md) |

### 3. Verificación de compilación

```
dotnet build KUtilitiesCore.sln --no-incremental
```

**Resultado:** `Compilación correcta. 42 Advertencia(s) / 0 Errores. Tiempo 00:00:43.26`

Atribución de los 42 warnings: todos preexistentes y ajenos a las bibliotecas del cambio. NU1902 (moderada) + NU1903 (alta) por `System.Security.Cryptography.Pkcs 9.0.18` en `Dal`, `DataAccessTests` y `LoggerTests`; CS warnings únicamente en proyectos de prueba preexistentes (`LoggerTests` CS8618×2; `GitHubUpdaterTests` CS8618×2; `DataAccessTests` CS8618×7/CS8625×1/CS8602×2/CS8604×4; `MVVMTests.EventCommandBinder` CS8604×1; `DataTests` CS8618×4/CS8601×2/MSTEST0056×8). **Cero warnings en las 14 bibliotecas** (incluidas las nuevas `KUtilitiesCore.Data.Wizard` y `KUtilitiesCore.Data.WPF`, y la refactorizada `KUtilitiesCore.Data.Win`) y en la nueva suite `KUtilitiesCore.Data.WPFTests` — el gate de build-hygiene (0 warnings atribuibles a las 14 libs) queda satisfecho.

## Resumen de pruebas añadidas en este cambio

- `ImportWizardControllerTests` (nuevo, `KUtilitiesCore.Data.WPFTests\Wizard\`, 13 pruebas): estado/carga/reset del orquestador (`Controller_AfterLoad_ExposesLoadedDataView_AndStopsLoading`, `Controller_SecondLoadWhileLoading_IsRejected`, `Controller_EmptyFileName_ResetsState`, `Controller_ZeroRowTable_KeepsImportDisabled`), propiedad de la tabla del llamador (`Controller_Load_UsesCallerOwnedTable_WithoutCloning`), debounce (`Debounce_ScheduleThenElapsed_RevalidatesOnceSilently`, `Debounce_ExplicitValidation_CancelsPending`, `Debounce_SuccessiveSchedules_Coalesce`), invalidación (`OptionsChanged_MarksDataStale_AndDisablesImport`) y exposición neutral de errores (`Errors_AfterValidation_ExposeRowFieldMessage`, `Errors_AfterCorrection_DisappearFromExposure`, `HasErrorColumn_FedByController_DrivesViewFilter`).
- `ImportWizardViewModelTests` (nuevo, `KUtilitiesCore.Data.WPFTests\WPF\`, 5 pruebas): `ViewModel_Commands_ExecuteAgainstController`, `ViewModel_RaisesPropertyChanged_OnControllerEvents`, `ViewModel_CanImport_MirrorsStaleAndValidState`, `CsvOptions_ExposeTrimAndIgnoreEmptyLines`, `ExcelOptions_ExposeSheetAndRowRange`.
- `RowErrorDescriptorTests` (nuevo, 1 prueba): `RowErrorDescriptor_MapsColumnErrors_FromDataRow`.
- `MessageDialogModelTests` (nuevo, 3 pruebas): `MessageDialogModel_FromArgs_MapsHeadingFootnoteExpander`, `MessageDialogModel_EmptyOptionals_AreCollapsed`, `ViewModel_LoadFailed_RoutesToMessageService`.
- `ExportDialogPreparationTests` (nuevo, `KUtilitiesCore.DataTests\DataExporter\`, 4 pruebas): `ValidateSource_NullOrEmptyTable_ReturnsFalse`, `GetDefaultFileName_FollowsTimestampPattern`, `ResolveFormat_CsvTsvPsvXlsxXls_MapsToExporter`, `ResolveFormat_UnknownExtension_ReturnsNone`.

Todas las pruebas siguen el ciclo TDD (fase roja confirmada antes de cada implementación).

## Revisión de baselines y tests de contrato (paso 10.1)

- `ImportWizardFormTests` (26 pruebas, WinTests): la suite usa exclusivamente la superficie pública conservada del formulario (hooks de `TestableImportWizardForm`: `ShowOpenDialogFile`, `LoadData`, `CreateReader`, `OnProcessImportFinished`, `ShowMessage`, `ImportData`, `RevalidateAfterEdit`, `FilterHasErrors`, `OnConfigOptionsChanged`, `FileName`, `AutoCloseOnSuccess`, `LoadedDataTable`, `ResultData` y controles del Designer). Ninguna prueba ejercita los contratos intencionalmente cambiados: `LoadedDataTable = null` ahora se ignora (ninguna prueba asigna null), `UpdateRowErrorFlags`/`EnsureHasErrorColumn` migraron al orquestador (nadie los invoca en el form) y `OnProcessImportFinished` ya no se dispara en la revalidación por debounce (la prueba de debounce no depende de él). `Import_ResultData_ShouldBeTypedAndOwn_Table` lee `ResultData` tras `Dispose()` — contrato preservado (el orquestador dispone la tabla del resultado sin anular la referencia). Pasó sin ajustes.
- `ImportConfigControlTests` (3 pruebas, WinTests): solo ejercen `GetParsingOptions()` vía la interfaz (la herencia del `IImportConfigControl` compartido es invisible — mismos miembros) y los seams `[EditorBrowsable(Never)]`. Pasó sin ajustes.
- Tests de `Exporter`: no existen pruebas preexistentes del exportador WinForms (verificado por grep); las nuevas `ExportDialogPreparationTests` cubren la preparación compartida extraída.
- Conclusión: no hubo que ajustar ninguna prueba existente; los contratos intencionales nuevos están documentados en los remarks XML del form y del orquestador.

## Resultado del paso

**APROBADO con la excepción preexistente documentada** (`GitHubUpdateServiceTest` 401 sin `GITHUB_TOKEN`): 223/225 correctas, 1 omitida por requisito de entorno (SQL Server local), 0 regresiones atribuibles al cambio. Compilación de la solución con 0 errores y 0 warnings atribuibles a las 14 bibliotecas del repositorio.
