# Tasks

> Norma aplicable (`docs/openspec-tasks-mandatory-steps.md`): suite de librerías .NET aislada, **sin API HTTP y sin frontend** → los Pasos N+2 (curl) y N+3 (Playwright E2E) se omiten por completo. Este cambio materializa conexiones reales contra el SQL Server local de la suite (`KUtilitiesCore.DataAccessTests`), pero todo SQL que ejecuta es de ámbito de sesión (`sp_set_session_context`, sondeo de lectura) y muere con la sesión — sin mutaciones persistentes —, por lo que la verificación de base de datos del Paso N+1 aplica en forma reducida: confirmar conectividad y ausencia de artefactos residuales. Verificación por `openspec/config.yaml`: `dotnet build KUtilitiesCore.sln` y `dotnet test --solution KUtilitiesCore.sln` (experiencia MTP, SDK 10). Gotchas de AGENTS.md obligatorios: mocks de `DbDataReader` con `CallBase = true` y `Read()`/`NextResult()` finitos, `ObjectDisposedException` → `Assert.Inconclusive`, Moq solo en `KUtilitiesCore.DataAccessTests`, skips heredados (GitHub Actions / no-Windows), fallo 401 preexistente de `GitHubUpdateServiceTest` sin `GITHUB_TOKEN`.

## 0. Configuración: Crear rama de la característica (OBLIGATORIO - PRIMER PASO)

- [x] 0.1 Crear la rama `feature/dal-onconnectionopened-reentrancy` a partir de la rama base `Dev-NET10` y cambiarse a ella; verificar con `git status` que la rama actual es la correcta
- [x] 0.2 Confirmar que el SQL Server local de la suite está disponible ejecutando `dotnet test KUtilitiesCore.DataAccessTests --filter "FullyQualifiedName~LoadRawRepoTest"` y confirmando verde (entorno listo para los tests de materialización de conexión)

## 1. TDD: tests en rojo del nuevo contrato del hook

- [x] 1.1 Crear en `KUtilitiesCore.DataAccessTests` la clase `DaoContextOnConnectionOpenedTests` (patrón `DaoUOWTests`: `SecureConnectionBuilder` localhost, skips heredados) y escribir el test de regresión de re-entrada: handler que invoca `SetSessionContextAsync` desde `OnConnectionOpened` debe completar sin excepciones, con `context.Connection` devolviendo la misma referencia que usarán las operaciones posteriores y el hook disparado exactamente una vez; confirmar el rojo — hoy el síntoma es recursión infinita (agotamiento del pool con timeout o `StackOverflowException` inatrapable que derriba el test host; ambos son evidencia válida del bug)
- [x] 1.2 Escribir en rojo el test de sonda: con el hook configurado, `DatabaseExists()` NO debe invocarlo ni materializar la conexión lógica; confirmar el rojo (hoy la sonda comparte fábrica y dispara el hook)
- [x] 1.3 Escribir en rojo los tests de la variante asíncrona: `OnConnectionOpenedAsync` configurado se invoca una vez (bloqueando la materialización hasta completar) y con ambos hooks configurados el orden es síncrono → asíncrono; confirmar el rojo (la API aún no compila) con `dotnet build KUtilitiesCore.DataAccessTests`
- [x] 1.4 Escribir en rojo los tests fail-closed: si el hook lanza, la conexión abierta queda dispuesta (sin fugas) y todo acceso posterior a `Connection` re-lanza la excepción cacheada; confirmar el rojo (hoy la conexión NO se dispone en el catch de la fábrica — fuga preexistente)

## 2. Contrato: miembro asíncrono en `ISqlExecutorContext`

- [x] 2.1 Añadir `Func<ISqlExecutorContext, Task>? OnConnectionOpenedAsync { get; set; }` a `ISqlExecutorContext` con XML docs en español (`<summary>` propósito, `<remarks>` con la advertencia de interbloqueo sync-over-async si el handler captura un `SynchronizationContext` y la semántica de envenenamiento permanente) e implementarlo en `DaoContext`; verificar con `dotnet build KUtilitiesCore.sln` que compila sin warnings de documentación (CS1591/CS1573) en los tipos tocados

## 3. Implementación de la fábrica, getter, sonda y Dispose

- [x] 3.1 Extraer `CreateConnectionCore()` (crear + abrir, sin publicar campo ni invocar hooks) y cambiar el sondeo de `DatabaseExists()` para usarla dentro del `using` existente; verificar que el test 1.2 pasa en verde
- [x] 3.2 Implementar la publicación anticipada: campo `private volatile DbConnection? _openedConnection`, getter `Connection` = `_openedConnection ?? _connection?.Value ?? throw ObjectDisposedException`; fábrica `CreateConnection()`: core → publicar `_openedConnection` → `EnableSqlLogging` → hook síncrono → hook asíncrono con `.GetAwaiter().GetResult()`; en el catch: disponer la conexión, anular `_openedConnection` y envolver en `DataAccessException`; verificar que el test 1.1 pasa en verde
- [x] 3.3 Anular `_openedConnection` en `Dispose()` junto a `_connection = null`; verificar que el dispose del contexto tras materializar conexión no lanza y los tests del grupo 1 siguen en verde
- [x] 3.4 Verificar el grupo 1 completo en verde con `dotnet test KUtilitiesCore.DataAccessTests --filter "FullyQualifiedName~DaoContextOnConnectionOpenedTests"`

## 4. Tests de comportamiento completo del contrato

- [x] 4.1 Test de disparo único bajo concurrencia: varios hilos acceden a `Connection` simultáneamente durante la materialización; todos reciben la misma conexión y el hook se invoca exactamente una vez (los accesos concurrentes esperan a que el hook complete); verificar en verde
- [x] 4.2 Test de orden determinista con ambos hooks configurados: síncrono primero, asíncrono después (registrar el orden observado y afirmarlo); verificar en verde — cubierto por el test del grupo 1 `BothHooks_SyncBeforeAsync_DeterministicOrder`, que registra el orden observado en una lista y lo afirma con `CollectionAssert`
- [x] 4.3 Test de extremo a extremo con SQL real de sesión: `OnConnectionOpened` invoca `SetSessionContextAsync` y una operación posterior sobre el mismo contexto lee `SESSION_CONTEXT(N'...')` devolviendo el valor establecido (misma conexión física lógica, sin pérdida del contexto de sesión); verificar en verde
- [x] 4.4 Test fail-closed de extremo a extremo: handler que lanza → conexión dispuesta (estado `Closed` y dispuesta) + acceso posterior re-lanza la `DataAccessException` cacheada sin ejecutar operaciones de base de datos (el consumidor debe crear un contexto nuevo); verificar en verde

## 5. Revisar y actualizar las pruebas unitarias existentes (OBLIGATORIO)

- [x] 5.1 Revisar los proyectos de test en busca de usos afectados: tests existentes de `DaoContext` que materializan conexión (`DaoUOWTests`, SP Gateway), mocks de `DbDataReader` conforme a los gotchas de AGENTS.md (`CallBase = true`, `Read()`/`NextResult()` finitos, `ObjectDisposedException` → `Assert.Inconclusive`); actualizar solo lo que el cambio exige, sin drive-by refactors; verificar que la suite compila sin errores — revisado con búsqueda global de `OnConnectionOpened`/`DatabaseExists`/`CreateConnection`: ningún test existente configura hooks ni depende del comportamiento antiguo de sonda/fábrica (solo la clase nueva `DaoContextOnConnectionOpenedTests`); `ExecuteReaderCore` y los mocks de `DbDataReader` no fueron tocados; la compilación de la suite se confirma en 6.2

## 6. Ejecutar pruebas unitarias y verificar el estado de la base de datos (OBLIGATORIO - EL AGENTE DEBE EJECUTARLAS)

- [x] 6.1 Capturar la línea base de la base de datos: conectividad al SQL Server local y ausencia de artefactos residuales persistentes (todo el SQL del cambio es de ámbito de sesión — `sp_set_session_context` y sondeos de lectura mueren con la sesión; solo persistiría algo si un test fuera defectuoso) — conectividad OK (servidor `ZERO`, SQL Server 15.0.2200.2 Developer); inventario `sys.databases` y sesiones usuario registrados en el reporte
- [x] 6.2 Ejecutar la suite específica `dotnet test KUtilitiesCore.DataAccessTests` y registrar aprobadas/falladas/omitidas — total: 45, correcto: 45, error: 0, omitido: 0 (3s 044ms)
- [x] 6.3 Ejecutar la suite completa `dotnet test --solution KUtilitiesCore.sln` y registrar conteo total, fallos, tiempo de ejecución y tests inestables (el fallo 401 de `GitHubUpdateServiceTest` sin `GITHUB_TOKEN` es preexistente y se documenta como tal) — total: 196, correcto: 194, error: 1 (GitHubUpdateServiceTest 401, preexistente), omitido: 1 (Logger SQL, por diseño), duración 21s; sin tests inestables
- [x] 6.4 Verificar el estado de la base de datos posterior a las pruebas: repetir los indicadores de la línea base, confirmar que no quedan mutaciones persistentes y restaurar/documentar si se encontrara alguna — inventario y sesiones idénticos a la línea base; 0 mutaciones persistentes; 0 acciones de restauración
- [x] 6.5 Crear el reporte `specs/dal-onconnectionopened-reentrancy/reports/AAAA-MM-DD-paso-N+1-verificacion-de-pruebas-unitarias-y-bd.md` (bajo la carpeta del cambio) con comandos ejecutados, resultados resumidos, comparación antes/después de la BD y acciones de limpieza — creado `reports/2026-10-25-paso-N+1-verificacion-de-pruebas-unitarias-y-bd.md`
- [x] 6.6 Marcar este paso como completado solo después de que la suite pase y el reporte exista

## 7. Actualizar la documentación técnica (OBLIGATORIO)

- [x] 7.1 Actualizar la sección *Architecture notes* de `AGENTS.md` con el contrato del hook: publicación anticipada sin re-entrada, disparo único por conexión lógica, exclusión de sondas, orden síncrono → asíncrono y semántica fail-closed con envenenamiento; verificar leyendo la sección actualizada — añadida subsección "Dal OnConnectionOpened hook contracts (dal-onconnectionopened-reentrancy)" antes de la subsección de UoW
- [x] 7.2 Documentar en español con `<example>` el patrón de uso completo en `ISqlExecutorContext.OnConnectionOpened`/`OnConnectionOpenedAsync` (RLS/auditoría: `OnConnectionOpened += ctx => ctx.SetSessionContextAsync(...)` señalando que `Connection` es seguro desde el handler) y en `DaoContextExt.SetSessionContextAsync`; corregir cualquier doc que prometa el comportamiento antiguo (disparo sobre sondas, orden hook→logging); verificar con una búsqueda en `KUtilitiesCore.Dal` de promesas obsoletas — `<example>` añadido a los tres miembros; búsqueda global sin docs obsoletas (la doc de `IDalContext.DatabaseExists` es genérica y correcta); corregido además el `<param>` equivocado de `SetSessionContextAsync` ("Conexión ADO.NET" → "Contexto de ejecución SQL"); los genéricos de los ejemplos van escapados (`<`/`>`) — build sin CS1570/CS1591/CS1573
- [x] 7.3 Auditar `ConfigureAwait(false)` en las rutas asíncronas de `KUtilitiesCore.Dal` (mitigación del riesgo de interbloqueo del design.md) y corregir donde falte en los métodos afectados por el cambio; verificar con búsqueda y build limpio — `DaoContextExt` (la ruta del handler documentada) no tenía ninguno: añadido a los 6 `await` de `SetSessionContextAsync`/`GetPublishedServerNameAsync`/`GetServerDateTimeAsync`; los `await` internos de `DaoContext` (ExecuteReaderAsyncCore, BulkInsert) y `DaoUnitOfWork` no están tocados por el cambio y quedan fuera de alcance (cambios graduales); `SPHelper` está excluido de compilación

## 8. Verificación final de integración

- [x] 8.1 Ejecutar `dotnet build KUtilitiesCore.sln --no-incremental` y confirmar 0 errores y 0 warnings en los proyectos de librería (los preexistentes de tests están fuera de alcance) — 0 errores; librerías limpias salvo NU1902/NU1903 preexistentes en `Dal` (vulnerabilidad del paquete transitorio System.Security.Cryptography.Pkcs 9.0.18, presentes desde la línea base del cambio 0.2); CS86xx/MSTEST0056 solo en proyectos de tests (fuera de alcance, preexistentes)
- [x] 8.2 Confirmar que ningún `.csproj` incorporó `NoWarn` nuevo ni se alteró `GenerateDocumentationFile`; verificar con una búsqueda en los csproj — búsqueda global: 0 `NoWarn` en todos los csproj; `GenerateDocumentationFile` presente e intacto en los 11 proyectos de librería; ningún csproj modificado por el cambio
- [x] 8.3 Ejecutar `openspec validate --change dal-onconnectionopened-reentrancy` y confirmar que el cambio valida correctamente — ejecutado como `openspec validate dal-onconnectionopened-reentrancy` (la opción `--change` no existe en esta versión de la CLI): "Change 'dal-onconnectionopened-reentrancy' is valid"

## Workflow follow-up

- Archivar el cambio (`openspec archive dal-onconnectionopened-reentrancy`) cuando los requisitos de revisión del proyecto estén satisfechos; el archivado genera el spec principal `dal-connection-lifecycle` con su Purpose completo.
- Verificar el resultado archivado (spec `openspec/specs/dal-connection-lifecycle/spec.md` con todos los requisitos y escenarios).
- Commits por patrón per-change: `feat(KUtilitiesCore.Dal)` con implementación + tests + `AGENTS.md`, luego `docs(openspec)` con el archivado; PR con `gh` base `Dev-NET10`.
