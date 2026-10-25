# Tasks

> Norma aplicable (`docs/openspec-tasks-mandatory-steps.md`): suite de librerías .NET aislada, **sin API HTTP y sin frontend** → los Pasos N+2 (curl) y N+3 (Playwright E2E) se omiten por completo. Este cambio sí ejecuta SQL transaccional real contra el SQL Server local de la suite (`KUtilitiesCore.DataAccessTests`), por lo que la verificación de base de datos del Paso N+1 aplica en forma reducida: los tests de escritura deben usar objetos de ámbito de sesión (tablas `#temp`) para no mutar estado persistente, y el reporte documenta la comparación antes/después. Verificación por `openspec/config.yaml`: `dotnet build KUtilitiesCore.sln` y `dotnet test --solution KUtilitiesCore.sln` (experiencia MTP, SDK 10). Gotchas de AGENTS.md obligatorios: mocks de `DbDataReader` con `CallBase = true` y `Read()`/`NextResult()` finitos, `ObjectDisposedException` → `Assert.Inconclusive`, Moq solo en `KUtilitiesCore.DataAccessTests`, skips heredados (GitHub Actions / no-Windows).

## 0. Configuración: Crear rama de la característica (OBLIGATORIO - PRIMER PASO)

- [x] 0.1 Crear la rama `feature/dal-explicit-transactions` a partir de la rama base y cambiarse a ella; verificar con `git status` que la rama actual es la correcta
- [x] 0.2 Confirmar que el SQL Server local de la suite está disponible ejecutando `dotnet test KUtilitiesCore.DataAccessTests --filter "FullyQualifiedName~LoadRawRepoTest"` y confirmando verde (entorno listo para los tests transaccionales)

## 1. Congelar la pasividad actual (tests de caracterización, verde desde el inicio)

- [x] 1.1 Crear en `KUtilitiesCore.DataAccessTests` la clase `DaoUowTransactionTests` con tests de pasividad usando mock Moq de `IDaoContext`: N accesos reiterados a `DaoUowContext.Transaction` devuelven null y `Verify` confirma cero llamadas a `BeginTransaction`; verificar en verde (es el comportamiento observable actual) con `dotnet test KUtilitiesCore.DataAccessTests --filter "FullyQualifiedName~DaoUowTransactionTests"`
- [x] 1.2 Extender la pasividad vía repositorios: subclases de prueba de `DaoRepositoryReadOnly<T>` y `RawRepositorybase` que expongan el acceso `protected Transaction`, resueltas vía `Repository<T>()`/`RawRepository<TRepo>()` del UoW, devuelven null sin aperturas implícitas; verificar en verde junto al 1.1
- [x] 1.3 Congelar el no-op actual: `SaveChanges()` sin transacción devuelve 0 y `Rollback()` sin transacción es no-op silencioso (cero llamadas a `BeginTransaction` en el mock); verificar en verde
- [x] 1.4 Verificar la regresión de comandos sin enlistar ejecutando `LoadRawRepoTest` (compatibilidad SP Gateway / autocommit intacta)

## 2. TDD: getter pasivo y API `DaoUnitOfWork.BeginTransaction`

- [x] 2.1 Escribir en rojo los tests de la nueva API: `uow.BeginTransaction()` crea una transacción `ReadCommitted` y queda registrada (`UowContext.Transaction` la devuelve); `uow.BeginTransaction(isolationLevel)` respeta el nivel explícito; `uow.Context.BeginTransaction()` no registra nada en el UoW; confirmar el rojo (la API no compila) con `dotnet build KUtilitiesCore.DataAccessTests`
- [x] 2.2 Implementar `DaoUnitOfWork.BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)` delegando en `IDaoContext.BeginTransaction` y registrando la transacción en el UoW; neutralizar el getter de `DaoUowContext` (devolver `_transaction` tal cual, jamás invocar `BeginTransaction`); eliminar el flag `isTransactionCreated` y corregir `DisposeTransaction` (anular la referencia, idempotente, sin fugas); verificar el grupo 2.1 en verde y el grupo 1 sin regresiones
- [x] 2.3 Escribir en rojo el test de vitalidad: tras finalizar la transacción registrada fuera del UoW (`tx.Commit()` o Dispose del `using`), `UowContext.Transaction` devuelve null y `SaveChanges()` devuelve 0 (referencia obsoleta, design.md D4); implementar la verificación de vitalidad (`GetTransactionObject() == null`); verificar en verde
- [x] 2.4 Documentar en español con `<example>`: `DaoUnitOfWork.BeginTransaction` (patrón `using` con `Commit` explícito), `IDaoUowContext.Transaction`/`DaoUowContext` (pasividad: devuelve solo la transacción registrada) y el contrato 0/no-op en `SaveChanges`/`Rollback` (design.md D7); verificar con `dotnet build KUtilitiesCore.sln` que no hay CS1591/CS1573 en los tipos tocados

## 3. Semántica transaccional real contra SQL Server local (tests de integración)

- [x] 3.1 Añadir tests de integración con tabla `#temp` de sesión (patrón `DaoUOWTests`: `SecureConnectionBuilder` localhost, skips heredados): `BeginTransaction` → INSERT → `SaveChanges()` devuelve 1 y la fila persiste; `BeginTransaction` → INSERT → `Rollback()` → la fila no existe y el UoW queda sin transacción; verificar en verde con `dotnet test KUtilitiesCore.DataAccessTests --filter "FullyQualifiedName~DaoUowTransactionTests"`
- [x] 3.2 Cubrir la finalización automática: `using var tx = uow.BeginTransaction()` → INSERT → salir del `using` sin `Commit` → la fila se revierte automáticamente (Dispose→Rollback de `TransactionBase`); `tx.Commit()` explícito → la fila persiste; verificar en verde
- [x] 3.3 Cubrir idempotencia y estado finalizado: `Commit`/`Rollback`/`Dispose` repetidos no lanzan; `SaveChanges()` tras transacción confirmada o revertida devuelve 0 (como sin transacción); verificar en verde
- [x] 3.4 Congelar el error de transacción ajena: pasar a una operación una transacción de otra conexión lanza `InvalidOperationException` ("La transacción pertenece a una conexión diferente") sin ejecutar el comando; verificar en verde

## 4. Revisar y actualizar las pruebas unitarias existentes (OBLIGATORIO)

- [x] 4.1 Revisar los proyectos de test en busca de usos afectados por el cambio: `DaoUOWTests.LoadRawRepoTest` (debe seguir verde sin cambios), mocks de `DbDataReader` conforme a los gotchas de AGENTS.md (`CallBase = true`, `Read()`/`NextResult()` finitos, `ObjectDisposedException` → `Assert.Inconclusive`); actualizar solo lo que el cambio exige, sin drive-by refactors; verificar que la suite compila sin errores

## 5. Ejecutar pruebas unitarias y verificar el estado de la base de datos (OBLIGATORIO - EL AGENTE DEBE EJECUTARLAS)

- [x] 5.1 Capturar la línea base de la base de datos: conectividad al SQL Server local y ausencia de artefactos residuales persistentes de pruebas anteriores del cambio en `master`/`tempdb` (las tablas `#temp` mueren con la sesión; solo persisten artefactos si un test fue defectuoso)
- [x] 5.2 Ejecutar la suite específica `dotnet test KUtilitiesCore.DataAccessTests` y registrar aprobadas/falladas/omitidas
- [x] 5.3 Ejecutar la suite completa `dotnet test --solution KUtilitiesCore.sln` y registrar conteo total, fallos, tiempo de ejecución y tests inestables (el fallo 401 de `GitHubUpdateServiceTest` sin `GITHUB_TOKEN` es excepción preexistente y se documenta como tal)
- [x] 5.4 Verificar el estado de la base de datos posterior a las pruebas: repetir los indicadores de la línea base, confirmar que no quedan mutaciones persistentes y restaurar/documentar si se encontrara alguna
- [x] 5.5 Crear el reporte `specs/dal-explicit-transactions/reports/AAAA-MM-DD-paso-N+1-verificacion-de-pruebas-unitarias-y-bd.md` (bajo la carpeta del cambio) con comandos ejecutados, resultados resumidos, comparación antes/después de la BD y acciones de limpieza
- [x] 5.6 Marcar este paso como completado solo después de que la suite pase y el reporte exista

## 6. Actualizar la documentación técnica (OBLIGATORIO)

- [x] 6.1 Actualizar la sección *Architecture notes* de `AGENTS.md`: el UoW de Dal nunca abre transacciones implícitas (getter pasivo), existe `DaoUnitOfWork.BeginTransaction` con default `ReadCommitted` y registro en el UoW, y `SaveChanges`/`Rollback` son 0/no-op sin transacción registrada viva; verificar leyendo la sección actualizada
- [x] 6.2 Buscar documentación con promesas de transacciones implícitas (p. ej. wording en XML docs o guías) y corregirlas al contrato explícito; verificar con una búsqueda en `KUtilitiesCore.Dal` y confirmar que ningún doc promete atomicidad automática del UoW

## 7. Verificación final de integración

- [x] 7.1 Ejecutar `dotnet build KUtilitiesCore.sln --no-incremental` y confirmar 0 errores y 0 warnings en los 12 proyectos de librería (los 28 preexistentes de tests están fuera de alcance)
- [x] 7.2 Confirmar que ningún `.csproj` incorporó `NoWarn` nuevo ni se alteró `GenerateDocumentationFile`; verificar con una búsqueda en los csproj
- [x] 7.3 Ejecutar `openspec validate --change dal-explicit-transactions` y confirmar que el cambio valida correctamente

## Workflow follow-up

- Archivar el cambio (`openspec archive dal-explicit-transactions`) cuando los requisitos de revisión del proyecto estén satisfechos; el archivado genera el spec principal `dal-transactions` con su Purpose completo.
- Verificar el resultado archivado (spec `openspec/specs/dal-transactions/spec.md` con todos los requisitos y escenarios).
