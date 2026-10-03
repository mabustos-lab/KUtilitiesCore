# Paso N+1 — Verificación de pruebas unitarias

**Cambio:** `importwizard-etl-improvements`
**Rama:** `feature/importwizard-etl-improvements` (desde `Dev-NET10`)
**Fecha:** 2026-10-03
**Nota:** El proyecto es una biblioteca de clases aislada (DLL) sin base de datos, sin API HTTP propia y sin frontend web: los pasos de verificación de BD (N+2) y E2E con Playwright (N+3) no aplican y se omiten según `docs/openspec-tasks-mandatory-steps.md`.

## Comandos ejecutados

### 1. Suites de los módulos modificados

```
dotnet test KUtilitiesCore.DataTests
dotnet test KUtilitiesCore.Data.WinTests
```

**Resultados:**

| Proyecto | Total | Correctas | Fallidas | Omitidas | Resultado |
|---|---|---|---|---|---|
| KUtilitiesCore.DataTests | 38 | 38 | 0 | 0 | Correcta (EXIT=0) |
| KUtilitiesCore.Data.WinTests | 26 | 26 | 0 | 0 | Correcta (EXIT=0) |

### 2. Suite completa obligatoria

```
dotnet test --solution KUtilitiesCore.sln
```

**Resultado:** `total: 169 / correcto: 167 / error: 1 / omitido: 1 / duración: 11s 933ms`

| Proyecto | Resultado |
|---|---|
| KUtilitiesCoreTests | correcto |
| KUtilitiesCore.DataTests (38) | correcto |
| KUtilitiesCore.Data.WinTests (26) | correcto |
| KUtilitiesCore.DataAccessTests | correcto |
| KUtilitiesCore.MVVMTests | correcto |
| KUtilitiesCore.MVVMTests.EventCommandBinder | correcto |
| KUtilitiesCore.LoggerTests | correcto (1 omitido: `CreateLogger_And_LogMessage_ToSql` requiere SQL Server local configurado — preexistente) |
| KUtilitiesCore.GitHubUpdaterTests | 1 error: `GitHubUpdateServiceTest` falla con 401 (Unauthorized) al consultar la API en vivo de GitHub sin `GITHUB_TOKEN` en el entorno — **falla preexistente documentada, no una regresión** (ver AGENTS.md y tasks.md 16.2) |

### 3. Verificación de compilación

Los comandos `dotnet test` reconstruyen los proyectos con las mismas opciones de compilación. No se introdujeron warnings nuevos en los proyectos modificados (`KUtilitiesCore.Data`, `KUtilitiesCore.Data.Win`, `KUtilitiesCore.DataTests`, `KUtilitiesCore.Data.WinTests`); los warnings mostrados durante la compilación pertenecen a proyectos de prueba ajenos al cambio (`GitHubUpdaterTests`, `LoggerTests`, `DataAccessTests`, `MVVMTests.EventCommandBinder`) y son preexistentes.

## Resumen de pruebas añadidas en este cambio

- `ImportManagerTests`: `ReadData_PaintsColumnErrors_OnProvidedTable`, `ValidateDataTypes_AppliesDefaultValue_OnProvidedTable`, `ValidateDataTypes_EmptyTable_ReturnsFalse`, `CreateResultTable_ReturnsTypedTable_WithoutControlColumns`.
- `ImportWizardFormTests`: `Import_ResultData_ShouldBeTypedAndOwn_Table`, `FilterHasErrors_ShouldShowOnlyErrorRows_AndEditsPropagate`, `BuildActiveDefinitions_ShouldNotMutate_OriginalDefinitions`, `Import_EmptyFile_ShouldWarn_AndNotExposeErrorsGridNoise`, `LoadDataAsync_ShouldLoad_WithoutBlockingCaller`, `MappingChange_ShouldTrigger_SilentRevalidationPath`, `OptionsChanged_ShouldMarkDataStale_AndDisableImport`, `ErrorsGrid_ShouldShowDisplayName_ForFieldName`, `FileName_Empty_ShouldResetWizardState`.
- `ImportConfigControlTests` (nuevo): `ExcelConfig_GetParsingOptions_ShouldReturnSelectedSheet`, `CsvConfig_GetParsingOptions_ShouldExpose_TrimAndEmptyLines`, `ExcelConfig_GetParsingOptions_ShouldExpose_RowRange`.

Todas las pruebas siguen el ciclo TDD (fase roja confirmada antes de cada implementación).

## Revisión de baselines y tests de contrato (paso 15)

- `ImportManager_EmptyLoad_IsValid_Test` (baseline 3.1): el archivo `datos_vacios.csv` contiene 4 filas con **valores** vacíos (no cero filas), por lo que no ejerce la rama de cero filas del contrato T2; pasó sin ajustes.
- `MultiSourceImportManagerTests`: ninguna prueba depende del clon interno de `ReadData` (T1) ni afirma `PropertyName` de los fallos (solo `IndexRow`); pasaron sin ajustes.
- `ImportWizardFormTests`: las pruebas existentes de errores afirman conteos y mensajes, no la columna `Campo`; el contrato nuevo (DisplayName) quedó fijado por `ErrorsGrid_ShouldShowDisplayName_ForFieldName`.
- Conclusión: no hubo que ajustar ninguna prueba existente; los contratos intencionales nuevos están cubiertos por las pruebas añadidas en sus secciones correspondientes.

## Resultado del paso

**APROBADO con la excepción preexistente documentada** (`GitHubUpdateServiceTest` 401 sin `GITHUB_TOKEN`): 167/169 correctas, 1 omitida por requisito de entorno (SQL Server local), 0 regresiones atribuibles al cambio.
