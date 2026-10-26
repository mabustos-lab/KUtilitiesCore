# Proposal

## Why

El módulo de importación de `KUtilitiesCore.Data.DataImporter` solo soporta una fuente por operación: `ImportManager.LoadData(IDataSourceReader)` procesa un único lector hacia un único `DataTable`. Para archivos con múltiples orígenes del mismo esquema (p. ej. un libro Excel con una pestaña por mes), el consumidor debe invocar la importación fuente por fuente, sin trazabilidad de origen ni garantías transaccionales. Se necesita una orquestación multi-fuente para automatizar estos escenarios a escala de decenas de fuentes.

## What Changes

- **Nueva clase orquestadora** en `KUtilitiesCore.Data.DataImporter` (p. ej. `MultiSourceImportManager`) que procesa N fuentes que comparten un mismo esquema (`FieldDefinitionCollection` único) en una sola operación.
- **Resultado agregado**: expone un `DataSet` con un `DataTable` por fuente, donde `TableName` identifica el origen (nombre de hoja/archivo).
- **Semántica transaccional (todo-o-nada)**: si cualquier fuente falla al leerse o valida con errores, la operación falla integralmente con una excepción que identifica la(s) fuente(s) fallida(s) y la causa; no se expone ningún resultado parcial y el estado del gestor queda como antes de la operación.
- **Variante asíncrona** con `CancellationToken`, alineada con `IDataSourceReader.ReadDataAsync()`.
- **Sin cambios en `ImportManager`**: la nueva clase compone el pipeline existente (principio abierto/cerrado); `ImportManager` y `ImportWizardForm` mantienen su comportamiento actual.
- **Fuera de alcance** (historias futuras): fuentes con esquemas heterogéneos en una misma operación, integración de UI en `ImportWizardForm` (`KUtilitiesCore.Data.Win`), y estrategia de streaming para miles+ de fuentes/filas.

## Capabilities

### New Capabilities

- `multi-source-import`: Importación transaccional de datos desde múltiples fuentes con esquema común, con resultado trazable por origen (un `DataSet` con un `DataTable` por fuente).

### Modified Capabilities

<!-- No existen capabilities previos (openspec/specs vacío); no se modifican requisitos existentes. -->

## Impact

- **Código**: solo se agregan tipos nuevos en `KUtilitiesCore.Data/DataImporter/` (orquestador, resultado, excepción de fallo multi-fuente). Sin cambios en `ImportManager`, `ImportWizardForm` ni interfaces existentes.
- **APIs**: nueva superficie pública en `KUtilitiesCore.Data` (net10.0, `Nullable enable`, XML docs en español con `<example>`).
- **Dependencias**: ninguna nueva; reutiliza `IDataSourceReader`, `IExcelSourceReader.GetSheets()/GetSheetInfo()`, `FieldDefinitionCollection` y `DataLoadException`.
- **Tests**: nuevo/extendido conjunto de tests MSTest en `KUtilitiesCore.DataTests/DataImporter/` (TDD); las suites existentes (`ImportManagerTests`, `ImportWizardFormTests`) deben permanecer en verde.
- **Consumidores**: ninguno afectado — adición pura, sin breaking changes.
