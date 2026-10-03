# Design

## Context

Ver `proposal.md` (sección "Por qué") como motivación. Estado actual relevante:

- `ImportManager.ReadData(DataTable)` clona la tabla provista a `_rawDataSource` y pinta los errores en ese clon privado; `Dispose` dispone ambas tablas internas. `ImportWizardForm` enlaza su vista previa a su propia tabla (`loadedDataTable`), por lo que el pintado nunca es visible y el filtro por `HasErrors` no encuentra filas.
- `ImportWizardForm.ImportData` asigna `ResultData = _importManager.DataSource` (referencia compartida, todo string, con `_RowIndex`/`_IsValid`), y `OnDispose` la dispone junto con `Dispose` del manager → doble dispose.
- El pipeline de fuente única ya expone `ReadDataAsync` en `IDataSourceReader` y `GetValidRows()/GetInvalidRows()`, ninguno usado por el wizard.
- Los proyectos afectados: `KUtilitiesCore.Data` (`ImportManager`), `KUtilitiesCore.Data.Win` (`ImportWizardForm`, `CsvConfigControl`, `ExcelConfigControl`). Tests: `KUtilitiesCore.DataTests`, `KUtilitiesCore.Data.WinTests` (WinForms, STATestMethod).

## Goals / Non-Goals

**Goals:**

- Corregir los defectos con cambios incrementales sobre la arquitectura existente (SOLID, sin grandes refactorings).
- Hacer observables los errores de validación en la tabla que el usuario ve y edita.
- Entregar al consumidor una tabla resultado tipada, sin columnas de control y con ciclo de vida independiente del formulario/manager.
- Escalar la interacción: carga sin bloqueo, revalidación estabilizada, invalidación por cambios de configuración.
- Exponer opciones de parsing ya soportadas por `TextFileParsingOptions`/`ExcelParsingOptions`.

**Non-Goals:**

- Streaming/paginación de lectura, `IProgress<T>` ni cancelación en `ImportManager` (la carga sigue siendo completa en memoria).
- Soporte de comillas RFC-4180 en el parser CSV.
- Wizard multi-archivo (conectar `MultiSourceImportManager` a la UI).
- Corregir el bug de `FieldDefinitionItem.LoadInfo` con `[Required]` (`AllowNull = true`) ni dar consumidor a `IsUnique` (detectados, fuera de alcance).
- Separador custom y `EscapeCharacter` en la UI de CSV.

## Decisions

### D1 — `ReadData` sin clon: propiedad de la tabla en el llamador

`ImportManager.ReadData(rawData)` pasa a usar la tabla provista directamente (`_rawDataSource = rawData`), pintando `SetColumnError` sobre ella. `Dispose` deja de disponer `_rawDataSource`.

- **Por qué**: es la única tabla que el usuario ve y edita; pintar sobre ella hace visibles los errores y funcionales el filtro y las ediciones. Elimina una copia O(n) completa.
- **Alternativas descartadas**: (a) mantener el clon y notificar errores por evento/callback para que el wizard los pinte en su tabla — más complejo, duplica el estado de errores; (b) exponer `_rawDataSource` por propiedad — rompe encapsulación.
- **Riesgo aceptado**: cambio de contrato público. `MultiSourceImportManager` ya trata sus tablas crudas como desechables (las crea, las pasa a `ReadData` y las dispone él mismo), por lo que sigue siendo compatible; se verifica con su suite de tests.

### D2 — Columna de apoyo `_HasError` + `DataView` para filtrar

El wizard agrega a la tabla cargada una columna `_HasError` (bool), actualizada tras cada validación (`UpdateRowErrorFlags`), y enlaza `dgvPreview` a un `DataView` de esa tabla. El filtro usa `RowFilter = "[_HasError] = true"`; la columna se oculta en el grid.

- **Por qué**: `HasErrors` no es filtrable por `RowFilter`; un `DataView` sobre la tabla original mantiene las ediciones sincronizadas (los `DataView` comparten el almacenamiento de la tabla).
- **Alternativas descartadas**: (a) clon `Clone()+ImportRow` (actual): copia, pierde ediciones — documentado en doc comment; (b) enlazar el grid a `ImportManager.DataSource`: es otra tabla, todo string y con columnas de control.

### D3 — `CreateResultTable()`: tabla resultado tipada propia

Nuevo método público en `ImportManager` que construye una `DataTable` nueva con una columna tipada por definición (`def.TargetType ?? typeof(string)`, nombrada `FieldName`), sin columnas de control, aplicando `TypeConverter.TryConvert`, `DefaultValue` en vacíos no-nulos y `DBNull` en el resto. `ImportData`/`RevalidateAfterEdit` asignan `ResultData = _importManager.CreateResultTable()`.

- **Por qué**: resuelve a la vez el doble dispose (la tabla es nueva, el wizard es su único dueño), la tipificación ETL y las columnas de control. SRP: el manager sabe convertir, el wizard solo expone.
- **Alternativas descartadas**: exponer `GetValidRows()` filtrado — sigue siendo string y con columnas de control.

### D4 — Debounce de revalidación con `System.Windows.Forms.Timer`

Timer de 400 ms reiniciado por cada edición de celda (`CellEndEdit`) y por cambios de mapeo (`dgvMapping.CellValueChanged` con guard `_populatingMapping`). Al hacer tick ejecuta `RevalidateAfterEdit()` en el hilo UI. `ImportData` detiene el timer pendiente.

- **Por qué**: reduce el pipeline O(n) por tecla a una ejecución por ráfaga de ediciones, en el hilo UI.
- **Alternativa descartada**: validación en background con lock — un `DataTable` vivo enlazado al grid no es thread-safe.

### D5 — Carga asíncrona con anti-reentrada; `LoadData()` síncrono se conserva

`public virtual async Task LoadDataAsync()` usa `reader.ReadDataAsync()` con `ConfigureAwait(true)` (el binding al grid debe ocurrir en el hilo UI). `btnLoadData_Click` pasa a `async void` con flag `_isLoading` y deshabilitación del botón (anti doble clic). `LoadData()` síncrono se mantiene como método virtual para no romper tests que lo sobrescriben. Se extrae `CreateReader()` (virtual) y `HandleLoadError(Exception)` (virtual) para compartir ambos caminos, y el reader se dispone en `finally` (cast a `IDisposable`).

- **Alternativa descartada**: eliminar `LoadData()` síncrono — rompe el harness de tests existente.

### D6 — `OptionsChanged` marca obsoleto, no recarga

El wizard se suscribe a `OptionsChanged` del control de configuración al seleccionar archivo. El handler `OnConfigOptionsChanged` invalida `ResultData`, deshabilita `btnImport` y muestra advertencia "recargue los datos"; con `loadedDataTable == null` no hace nada (neutraliza el `SelectedIndexChanged` inicial de `ExcelConfigControl.Initialize`).

- **Alternativa descartada**: recarga automática al cambiar opciones — los combos disparan eventos intermedios (incluida la inicialización de hojas Excel) y provocarían recargas costosas e inesperadas.

### D7 — Hooks de prueba `EditorBrowsableState.Never` en controles

`ExcelConfigControl` y `CsvConfigControl` exponen métodos de configuración para tests (`AddSheetForTesting`, `SelectSheetForTesting`, `SetTrimValuesForTesting`, `SetIgnoreEmptyLinesForTesting`, `SetRowRangeForTesting`) decorados con `[EditorBrowsable(EditorBrowsableState.Never)]`, siguiendo el patrón existente de `TestableImportWizardForm` (subclase en el proyecto de tests) sin usar `InternalsVisibleTo`.

### Modelo de errores/excepciones

- `ReadData(null)` → `ArgumentNullException` (nuevo guard con `ArgumentNullException.ThrowIfNull`).
- Tabla sin filas: `ValidateDataTypes` retorna `false`; el wizard muestra aviso (`Warning`) y no lanza.
- Errores de E/S en carga: `HandleLoadError` centraliza feedback (status strip + `ShowMessage` error), sin excepciones hacia el UI thread.

## Risks / Trade-offs

- [Riesgo] Contrato público de `ReadData` cambia; consumidores externos que asumieran la copia interna ven rotura. → Mitigación: doc comment XML explica la semántica de propiedad y por qué se descartó el clon; se anota en el delta `multi-source-import` y en el proposal (Impact).
- [Riesgo] Baseline de `ImportManager_EmptyLoad_IsValid_Test` puede cambiar con la semántica de vacío. → Mitigación: ejecutar el test antes del cambio para capturar el baseline y ajustarlo solo si el contrato vacío cambia intencionalmente.
- [Riesgo] Enlazar `dgvPreview` a `DataView` puede afectar tests existentes del wizard. → Mitigación: suite completa `KUtilitiesCore.Data.WinTests` tras T5; ajustes solo en wiring, no en asserts de datos.
- [Riesgo] Columna `_HasError` visible en el preview. → Mitigación: ocultación explícita en el wiring del grid (DataPropertyName `_HasError`).
- [Trade-off] El debounce introduce una ventana de 400 ms donde el grid de errores puede estar desactualizado; aceptable: `ImportData` siempre revalida de forma explícita y completa antes de entregar el resultado.

## Migration Plan

- Cambio en una sola rama `feature/importwizard-etl-improvements`, 14 tareas TDD secuenciales, cada una con build + tests verdes antes de la siguiente.
- Sin datos persistentes ni consumidores externos conocidos; la verificación de compatibilidad se limita a las suites del repo (incluida `MultiSourceImportManagerTests`).
- Rollback: revert de la rama; no hay artefactos de estado que migrar.

## Open Questions

Ninguna: los aspectos que podrían cambiar el alcance (streaming, cancelación, multi-archivo) están explícitamente fuera de los Non-Goals y documentados como trabajo futuro.
