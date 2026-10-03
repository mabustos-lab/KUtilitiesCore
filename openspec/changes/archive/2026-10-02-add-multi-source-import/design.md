# Design

## Context

`ImportManager` (KUtilitiesCore.Data/DataImporter/ImportManager.cs) procesa una única fuente por operación: `LoadData(IDataSourceReader)` lee hacia su `DataTable` interno `DataSource` y valida con un `FieldDefinitionCollection` y un `ValidationResult` únicos. La infraestructura ya soporta enumeración multi-hoja (`IExcelSourceReader.GetSheets()/GetSheetInfo()`, `IExcelWorkbookReader.GetWorksheet()`). Ver proposal.md (sección Why) para la motivación.

Restricciones relevantes verificadas en código:

- `ValidateRow` usa `_RowIndex` como índice directo en `_rawDataSource.Rows[rowIndex]` (ImportManager.cs:341): los índices de fila son inherentes al ámbito de cada fuente.
- `ValidationResult`/`ValidationFailure` no distinguen la fuente de origen de cada error.
- Todo el pipeline es en memoria (`DataTable` con columnas `string`), duplicando crudo + mapeado. Escala objetivo: decenas de fuentes/filas.
- `KUtilitiesCore.Data` es net10.0 con `Nullable enable`; XML docs en español; sin `InternalsVisibleTo`.
- Consumidor de UI: `ImportWizardForm` (KUtilitiesCore.Data.Win) instancia un único `ImportManager`.

## Goals / Non-Goals

**Goals:**

- Orquestar N fuentes de mismo esquema en una operación, componiendo (no modificando) el pipeline existente.
- Garantizar semántica todo-o-nada pura (sin persistencia externa, la "transacción" es sobre el estado en memoria expuesto).
- Entregar un `DataSet` trazable por origen.
- Mantener `ImportManager` y `ImportWizardForm` intactos (cero regresiones).

**Non-Goals:**

- Esquemas heterogéneos en una misma operación (historia futura).
- UI en `ImportWizardForm` (historia de seguimiento).
- Streaming/paralelismo masivo para miles+ de fuentes/filas.
- Cambios en contratos existentes (`IDataSourceReader`, `FieldDefinitionCollection`, `ValidationResult`).

## Decisions

### D1: Composición de `ImportManager` por fuente (no herencia ni modificación)

La nueva clase (p. ej. `MultiSourceImportManager`) crea internamente **un `ImportManager` por fuente**, configurado con el mismo `FieldDefinitionCollection`, y delega en él la lectura, el mapeo y la validación.

- **Por qué**: cumple OCP sin riesgo de regresión; reutiliza toda la validación (tipos, reglas de negocio, defaults); y resuelve automáticamente el riesgo de `_RowIndex` — cada instancia tiene su propio `_rawDataSource`, de modo que los índices y errores son naturalmente relativos a su fuente.
- **Alternativas descartadas**:
  - *Modificar `ImportManager` para aceptar N fuentes*: rompería el contrato del que depende `ImportWizardForm` y acoplaría dos responsabilidades.
  - *Replicar el pipeline en paralelo*: duplicaría la lógica de validación (violation DRY) y divergiría con el tiempo.

### D2: Identidad de fuente — par lector + nombre de origen

Cada fuente se registra como un par `(IDataSourceReader, sourceName)`. Para Excel, `sourceName` se toma de la hoja seleccionada (`SheetName` / `GetSheets()`); para CSV/disco, del archivo. `sourceName` es la clave de trazabilidad: `TableName` del `DataTable` resultante, identificación en excepciones y en errores de validación.

- **Alternativa descartada**: inferir el nombre del lector — `IDataSourceReader` no expone nombre y las implementaciones difieren (ruta vs. hoja).

### D3: Resultado — `DataSet` con copias de los `DataTable`

Al completar todas las fuentes con éxito, se ensambla un `DataSet` con una **copia** de cada `ImportManager.DataSource` (`Copy()`), asignando `TableName = sourceName`.

- **Por qué**: desacopla el resultado del estado interno de los `ImportManager` (que exponen columnas de control `_RowIndex`/`_IsValid` útiles al consumidor, igual que en fuente única); con escala de decenas, el coste de la copia es irrelevante y evita aliasing accidental.
- **Alternativa descartada**: transferir la propiedad de los `DataTable` internos — acoplaría el ciclo de vida del resultado a los gestores internos.

### D4: Semántica todo-o-nada en dos fases

La operación ejecuta en dos fases: (1) **procesar y validar todas** las fuentes (composición D1), coleccionando fallos por fuente; (2) solo si **todas** pasan, ensamblar el `DataSet` (D3). Si alguna falla, se lanza la excepción agregada (D5) y se descarta el estado intermedio — como el resultado solo se expone en la fase 2, no existe estado parcial observable.

- **Alternativa descartada**: *fail-fast al primer error* — la decisión del Product Owner es todo-o-nada, pero el procesamiento completo permite reportar **todas** las fuentes fallidas en una sola excepción en vez de solo la primera.

### D5: Excepción estructurada `MultiSourceImportException` (deriva de `DataLoadException`)

Nueva excepción con una colección de fallos por fuente: cada fallo lleva `SourceName`, la causa (excepción de lectura o resumen de errores de validación con su `ValidationFailure` e índice relativo). Deriva de `DataLoadException` para ser capturable por los consumidores existentes.

- **Alternativa descartada**: `DataLoadException` con mensaje plano — pierde la estructura por fuente que la UI futura necesitará para reportar.

### D6: Asíncrono secuencial con `CancellationToken`

`ImportAsync(cancellationToken)` procesa las fuentes en secuencia vía `ReadDataAsync()`, comprobando el token entre fuentes (y delegándolo a los lectores que lo soporten).

- **Por qué**: a escala de decenas la concurrencia no aporta; ClosedXML no es thread-safe por libro; secuencial = comportamiento determinista y simple para la semántica transaccional.
- **Alternativa descartada**: `Task.WhenAll` paralelo — riesgo de thread-safety con el mismo libro y complicaría la cancelación ordenada.

### D7: Sin `IProgress<T>` en esta iteración

Se excluye el reporte de progreso de la API de esta versión para mantener la superficie mínima (la historia refinada no lo exige). Se registra en Open Questions.

## Risks / Trade-offs

- [Memoria: copia de N `DataTable` en la fase 2] → Aceptado por escala objetivo (decenas); el streaming queda explícitamente fuera de alcance. Si una historia futura eleva la escala, revisar D3.
- [Alias de `FieldDefinitionCollection` compartido entre N `ImportManager` (mutable, `ICloneable`)] → La clase orquestadora clonará la colección por fuente para evitar interferencia si alguna instancia muta su copia local.
- [Regresión en `ImportManager`/`ImportWizardForm`] → Adición pura (solo archivos nuevos); verificación con `ImportManagerTests` + `ImportWizardFormTests` + build de la solución como puerta.
- [`TableName` duplicado entre fuentes (p. ej. dos hojas con el mismo nombre en libros distintos, mezcla CSV+Excel)] → Validar unicidad de `sourceName` al registrar fuentes; rechazo con error de precondición.

## Migration Plan

Adición pura: nuevos archivos en `KUtilitiesCore.Data/DataImporter/` y tests en `KUtilitiesCore.DataTests/DataImporter/`. Sin cambios en contratos ni consumidores existentes. Rollback = eliminar los archivos nuevos.

## Open Questions

- ¿Añadir `IProgress<T>` (progreso por fuente completada) en una iteración posterior? No cambia el enfoque; es un aditivo a la API. Pendiente de la historia de UI.
- ¿Exponer además un acceso por fuente individual al resultado (indexer `this[string sourceName]`)? Decisión de conveniencia de API, aplazable al refactor de uso real.
