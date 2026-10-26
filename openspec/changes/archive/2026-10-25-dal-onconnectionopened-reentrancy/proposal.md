# Proposal

## Why

El hook `OnConnectionOpened` de `ISqlExecutorContext` se invoca hoy DENTRO de la fábrica del `Lazy<DbConnection>` de `DaoContext` (DaoContext.cs:833), antes de que el Lazy publique la conexión. Su propia documentación XML lo destina a `sp_set_session_context` (ISqlExecutorContext.cs:26-29), pero ese uso provoca recursión infinita: el handler → `SetSessionContextAsync` → `context.Connection` → `_connection.Value` → re-entra en la fábrica → nuevo `Open()` → hook de nuevo → `StackOverflowException` (inatrapable, mata el proceso), con una `SqlConnection` real filtrada por cada nivel (puede agotar el pool y colgar la aplicación antes del desbordamiento). El hook es inutilizable para su propósito documentado; cualquier consumidor de auditoría/RLS que lo use según su documentación destruye el proceso.

## What Changes

- **Sin re-entrada**: `context.Connection` resolverá la conexión viva desde dentro del propio handler. La conexión se publicará en un campo (`_openedConnection`) ANTES de invocar el hook, de modo que el getter nunca re-ejecute la fábrica del Lazy.
- **Disparo único por conexión lógica**: el hook se invoca exactamente UNA vez por conexión lógica del `DaoContext`, en su materialización (primer uso). Los reciclajes del pool de ADO.NET no re-disparan el hook.
- **Exclusión de conexiones de sonda**: se extraerá `CreateConnectionCore()` (crear + abrir, sin hook) para el sondeo de `DatabaseExists()`; las conexiones de sonda/temporales JAMÁS disparan el hook.
- **Variante asíncrona**: nuevo miembro `OnConnectionOpenedAsync` de tipo `Func<ISqlExecutorContext, Task>` en `ISqlExecutorContext`. Ambos hooks (síncrono y asíncrono) son independientes y configurables; con ambos configurados, el orden documentado es determinista: primero síncrono, luego asíncrono (bloqueante sobre el hilo materializador).
- **Fail-closed con envenenamiento**: si el hook falla (síncrono o asíncrono), la conexión abierta se dispone (sin fugas), el campo publicado se limpia y la excepción se cachea por el Lazy (`ExecutionAndPublication`): todo acceso posterior la re-lanza. El contexto NUNCA ejecuta operaciones de base de datos sin que el handler haya completado; el consumidor debe crear un contexto nuevo. **BREAKING** para implementadores externos de `ISqlExecutorContext` (el interface gana el miembro `OnConnectionOpenedAsync`); en el repo solo lo implementa `DaoContext`, el resto son constraints genéricos.
- **Orden de logging**: `EnableSqlLogging` pasa a ejecutarse ANTES del hook para que el `sp_set_session_context` emitido por el handler quede registrado en el log SQL.

## Capabilities

### New Capabilities

- `dal-connection-lifecycle`: Contrato de materialización de conexiones de `KUtilitiesCore.Dal` — disparo único, libre de re-entrada y fail-closed del hook `OnConnectionOpened` (y su variante asíncrona) sobre la conexión lógica del contexto, con exclusión de conexiones de sonda.

### Modified Capabilities

(ninguna — el cambio no altera los requisitos de `dal-transactions`; el hook es ortogonal al contrato transaccional)

## Impact

- **Código afectado**: `KUtilitiesCore.Dal/DaoContext.cs` (fábrica `CreateConnection`, getter `Connection`, sondeo `DatabaseExists`, `Dispose`); `KUtilitiesCore.Dal/ISqlExecutorContext.cs` (nuevo miembro `OnConnectionOpenedAsync` + XML docs); `KUtilitiesCore.Dal/DaoContextExt.cs` (solo documentación del patrón de uso con SESSION_CONTEXT).
- **Compatibilidad**: adición de miembro a `ISqlExecutorContext` — binary-breaking para implementadores externos; dentro del repo solo `DaoContext` lo implementa (el resto usa constraints `where TDAO : ISqlExecutorContext`).
- **Consumidores del hook**: hoy ningún consumidor interno usa `OnConnectionOpened`; el comportamiento existente (disparo sobre conexiones de sonda, orden hook→logging) cambia, pero el hook documentado era inutilizable para su propósito (RLS/auditoría vía `sp_set_session_context`).
- **Riesgos residuales**: el bloqueo `.GetAwaiter().GetResult()` del hook asíncrono puede interbloquear si un handler captura un `SynchronizationContext` de UI (se documentará en `<remarks>`; se verificará `ConfigureAwait(false)` en las rutas asíncronas de Dal); el envenenamiento permanente convierte una falla transitoria del handler en indisponibilidad del contexto (comportamiento elegido: fail-closed para auditoría).
- **Pruebas**: `KUtilitiesCore.DalTests` — regresión de recursión (falla hoy, pasa tras el cambio), envenenamiento, sonda, disparo único bajo concurrencia, orden determinista de hooks.
