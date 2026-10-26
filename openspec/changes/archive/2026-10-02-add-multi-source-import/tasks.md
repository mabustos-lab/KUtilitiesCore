# Tasks

> Tipo de proyecto: **librería de clases .NET aislada** (`KUtilitiesCore.Data`, net10.0). Sin base de datos, sin API HTTP, sin interfaz web. Según la Nota de Validación de `docs/openspec-tasks-mandatory-steps.md`, los Pasos N+1 (verificación de BD), N+2 (curl) y N+3 (Playwright E2E) **se omiten por completo**. Rama: `feature/add-multi-source-import`.

## 0. Configuración: Crear rama de la característica (OBLIGATORIO - PRIMER PASO)

- [x] 0.1 Crear la rama `feature/add-multi-source-import` a partir de main/master y cambiarse a ella; verificar con `git branch --show-current` que la rama activa es la correcta

## 1. Contratos: excepción multi-fuente (TDD)

- [x] 1.1 Escribir tests en rojo de `MultiSourceImportException` en `KUtilitiesCore.DataTests/DataImporter/`: deriva de `DataLoadException`, expone la colección de fallos por fuente (cada fallo con `SourceName`, causa de lectura o errores de validación con índice de fila relativo a su fuente); verificar con `dotnet test KUtilitiesCore.DataTests` que fallan antes de implementar
- [x] 1.2 Implementar `MultiSourceImportException` en `KUtilitiesCore.Data/DataImporter/` con XML docs en español (propósito y `<example>`); verificar que los tests de 1.1 pasan

## 2. Orquestador multi-fuente: registro de fuentes y operación síncrona transaccional (TDD)

- [x] 2.1 Escribir tests en rojo del registro de fuentes del orquestador (`MultiSourceImportManager`): rechaza invocar la operación sin fuentes (error de precondición), rechaza lector nulo o nombre de fuente vacío/nulo, rechaza nombres de fuente duplicados (unicidad de `TableName`); verificar RED
- [x] 2.2 Implementar el registro de fuentes (par lector + `sourceName`, `FieldDefinitionCollection` clonada por fuente según diseño D1/D2) y sus precondiciones; verificar que los tests de 2.1 pasan
- [x] 2.3 Escribir tests en rojo de la operación síncrona: N fuentes del mismo esquema procesan correctamente (un `ImportManager` interno por fuente); una fuente con fallo de lectura (`FileNotFoundException`/`IOException`/`UnauthorizedAccessException`) produce `MultiSourceImportException` identificando la fuente y la causa, sin exponer resultado parcial; una fuente con errores de validación produce fallo integral con fallos reportados por fuente; verificar RED
- [x] 2.4 Implementar la operación síncrona en dos fases (procesar y validar todas las fuentes; solo si todas pasan, habilitar el resultado; ante fallo, lanzar la excepción agregada y descartar el estado intermedio); verificar que los tests de 2.3 pasan

## 3. Resultado trazable: `DataSet` con un `DataTable` por fuente (TDD)

- [x] 3.1 Escribir tests en rojo del resultado: la operación exitosa expone un `DataSet` con un `DataTable` por fuente, `TableName` = nombre del origen, fuentes con columnas en distinto orden mapean igual (emparejamiento case-insensitive), y las filas conservan `_RowIndex` relativo a su fuente; verificar RED
- [x] 3.2 Implementar el ensamblado del `DataSet` (copia de cada `DataSource` interno con `Copy()`, asignando `TableName`); verificar que los tests de 3.1 pasan

## 4. Variante asíncrona cancelable (TDD)

- [x] 4.1 Escribir tests en rojo de `ImportAsync`: procesa N fuentes vía `ReadDataAsync()` de forma secuencial; la cancelación del token aborta la operación con `OperationCanceledException` sin exponer resultados parciales y dejando el estado sin cambios; verificar RED
- [x] 4.2 Implementar `ImportAsync` secuencial comprobando el `CancellationToken` entre fuentes; verificar que los tests de 4.1 pasan

## 5. Liberación de recursos (TDD)

- [x] 5.1 Escribir tests en rojo de la disposición: tras disponer el gestor, los `ImportManager` internos y los `DataTable`/`DataSet` de resultado quedan liberados/desechados; verificar RED
- [x] 5.2 Implementar `IDisposable` en el orquestador liberando todos los recursos de las fuentes procesadas; verificar que los tests de 5.1 pasan

## 6. Revisar y actualizar pruebas unitarias existentes (OBLIGATORIO - Paso N)

- [x] 6.1 Revisar `KUtilitiesCore.DataTests/DataImporter/ImportManagerTests.cs` y `KUtilitiesCore.Data.WinTests/ImportWizardFormTests.cs`: confirmar que el cambio es una adición pura (sin modificación de contratos existentes), que ninguna prueba existente requiere cambios y que la cobertura del spec delta queda completa con los tests nuevos de los grupos 1-5

## 7. Ejecutar pruebas unitarias y generar reporte (OBLIGATORIO - Paso N+1; sin BD — librería aislada)

- [x] 7.1 Ejecutar las pruebas específicas de los módulos modificados: `dotnet test KUtilitiesCore.DataTests` — confirmar cero fallos
- [x] 7.2 Ejecutar la suite completa: `dotnet build KUtilitiesCore.sln` (puerta de verificación: sin warnings) y `dotnet test --solution KUtilitiesCore.sln` (experiencia MTP del SDK 10); registrar conteo total, fallos y tiempo de ejecución. Nota conocida: `GitHubUpdaterTests` puede fallar con 401 sin `GITHUB_TOKEN` — preexistente, documentar como excepción si ocurre
- [x] 7.3 Crear el reporte `openspec/changes/add-multi-source-import/reports/AAAA-MM-DD-paso-N+1-verificacion-de-pruebas-unitarias.md` con los comandos ejecutados y los resultados resumidos (sin sección de base de datos: no aplica)
- [x] 7.4 Marcar este paso como completado solo después de que las pruebas pasen (o se documenten las excepciones aprobadas) y exista el archivo del reporte

## 8. Actualizar documentación técnica (OBLIGATORIO - Paso N+4)

- [x] 8.1 Completar los XML doc comments en español de todos los tipos públicos nuevos (propósito y "por qué", `<example>` donde el patrón de uso no sea obvio) y verificar que `dotnet build KUtilitiesCore.sln` no genera warnings de documentación
- [x] 8.2 Actualizar la documentación técnica del módulo afectado (p. ej. `generated-docs/USAGE.md` u otra doc existente de `KUtilitiesCore.Data`) con el patrón de uso del orquestador multi-fuente; verificar que el ejemplo documentado compila tal como está escrito y que es coherente con `specs/multi-source-import/spec.md`
