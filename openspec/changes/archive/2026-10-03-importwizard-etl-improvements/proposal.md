# Propuesta: Mejoras del Asistente de Importación (ETL)

## Por qué

`ImportWizardForm` es el punto de entrada de importación de datos para funcionalidades ETL y similares, pero el análisis del código actual (`KUtilitiesCore.Data.Win\Importer\` y `KUtilitiesCore.Data\DataImporter\`) revela defectos que rompen funciones ya expuestas y limitaciones que impiden su uso en cargas de datos reales:

- **Filtro "Mostrar solo filas con errores" roto**: el checkbox nunca muestra filas porque `ImportManager` pinta los errores en una copia interna (`_rawDataSource`) que la vista previa no usa; además el filtro actual clona la tabla (`Clone()` + `ImportRow`), por lo que las ediciones hechas en la vista filtrada se pierden.
- **Celdas con error invisibles**: el pintado de errores cae en la tabla interna, nunca en la vista previa enlazada al grid.
- **`ResultData` inutilizable tras cerrar el formulario**: expone el mismo `DataTable` interno de `ImportManager`, que se dispone dos veces al cerrar (el consumidor con `using(form)` recibe una tabla disposed), incluye columnas de control (`_RowIndex`, `_IsValid`) y no está tipado según las definiciones.
- **Selección de hoja Excel ignorada**: `ExcelConfigControl.GetParsingOptions` usa `cboSheet.SelectedText` (texto resaltado, casi siempre vacío), por lo que siempre se lee la primera hoja.
- **Contradicción con datos vacíos**: `ValidateDataTypes` con cero filas retorna `true` pero marca `IsValid = false`.
- **Fugas y mutaciones**: el reader (`IExcelSourceReader` es `IDisposable`, ClosedXML retiene handle) nunca se dispone; `BuildActiveDefinitions` muta las `FieldDefinitionItem` compartidas del llamador.
- **Escalabilidad ETL**: la carga de archivos bloquea el hilo UI (existe `ReadDataAsync` sin usar); cada edición de celda re-ejecuta el pipeline completo O(n); el evento `OptionsChanged` no tiene subscriptores (cambiar separador/hoja no invalida los datos cargados); opciones de parsing soportadas por `TextFileParsingOptions`/`ExcelParsingOptions` (TrimValues, IgnoreEmptyLines, StartRow, EndRow) no tienen UI.

## Qué cambia

### Corrección de defectos (bloque A)

- `ImportManager.ReadData(DataTable)` deja de clonar la tabla provista y pinta los errores de validación directamente sobre ella; la propiedad de la tabla pasa a ser del llamador (ya no la dispone `ImportManager`).
- `ValidateDataTypes` retorna y marca `IsValid` coherentemente (`false`) cuando no hay filas.
- El wizard filtra y pinta errores mediante `DataView` sobre la tabla cargada (con una columna de apoyo `_HasError`), de modo que el filtro funciona y las ediciones propagan.
- `ResultData` pasa a ser una tabla nueva, tipada según `TargetType`, propiedad del consumidor, sin columnas de control, obtenida vía un nuevo `ImportManager.CreateResultTable()`.
- `ExcelConfigControl` usa la hoja realmente seleccionada (`SelectedItem`).
- El reader se dispone tras la carga; `BuildActiveDefinitions` trabaja sobre clones de las definiciones; guard explícito de archivo sin filas.

### Escalabilidad ETL/UX (bloque B)

- Carga asíncrona (`LoadDataAsync` + `ReadDataAsync`) con anti-reentrada.
- Revalidación silenciosa con debounce para ediciones de celda y cambios de mapeo.
- Suscripción a `OptionsChanged` para marcar datos obsoletos y deshabilitar la importación.
- Exposición en UI de opciones de parsing: TrimValues e IgnoreEmptyLines (CSV); rango de filas StartRow/EndRow (Excel).

### Higiene (bloque C)

- Unificación del `PropertyName` de los fallos (`FieldName`), `DisplayName` en el grid de errores, uso del retorno bool de `ValidateDataTypes`, eliminación de `Application.DoEvents()`, using muerto, comentario muerto, null-check redundante, y reset de estado cuando `FileName` se asigna vacío.

## Capacidades

### Nueva: `specs/single-source-import/spec.md`

Pipeline de importación de fuente única (`ImportManager`): mapeo sobre la tabla provista sin clonar, pintado de errores en la tabla del llamador, semántica coherente con datos vacíos y generación de una tabla resultado tipada sin columnas de control.

### Nueva: `specs/import-wizard/spec.md`

Comportamiento observable del asistente: filtro de filas con errores con propagación de ediciones, `ResultData` tipado y propio, liberación del reader, no mutación de las definiciones del llamador, guard de archivo vacío, carga asíncrona no reentrante, revalidación silenciosa con debounce (celdas y mapeo), invalidación por cambio de opciones de análisis, y exposición de opciones de parsing en los controles de configuración (incluida la hoja Excel seleccionada).

### Modificada: `specs/multi-source-import/spec.md`

El requirement existente "Compatibilidad con la importación de fuente única" exige que los tests existentes pasen "sin modificación de su código ni de su comportamiento". El cambio intencional de contrato de `ReadData` (propiedad de la tabla del llamador) hace necesario ajustar ese requirement: `MultiSourceImportManager` sigue siendo compatible (ya trata sus tablas como desechables), pero los tests de contrato de fuente única se adaptan al nuevo comportamiento.

## Impacto

- **Proyectos afectados**: `KUtilitiesCore.Data` (ImportManager), `KUtilitiesCore.Data.Win` (ImportWizardForm, CsvConfigControl, ExcelConfigControl), tests `KUtilitiesCore.DataTests` y `KUtilitiesCore.Data.WinTests`.
- **Cambios de contrato público**: `ImportManager.ReadData` (semántica de propiedad, breaking para quien confiara en la copia interna), `ImportManager.Dispose` (ya no dispone la tabla cruda), retorno de `ValidateDataTypes` con datos vacíos, `ResultData` del wizard (nueva tabla tipada propia). `MultiSourceImportManager` verificado como compatible.
- **Compatibilidad**: el comportamiento del wizard para el consumidor mejora sin cambiar su superficie API (salvo `ResultData` ahora seguro de usar tras dispose del formulario).
- **Riesgos**: adaptación de baselines de tests existentes (`ImportManager_EmptyLoad_IsValid_Test`); verificación de `MultiSourceImportManagerTests` tras el cambio de `ReadData`.
