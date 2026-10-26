# Design

## Context

`DaoContext` materializa su conexión con `Lazy<DbConnection>` (modo `ExecutionAndPublication`, DaoContext.cs:86). La fábrica `CreateConnection()` (DaoContext.cs:823-844) crea y abre la conexión, invoca `OnConnectionOpened?.Invoke(this)` DENTRO de la fábrica (línea 833) — antes de que el Lazy publique el valor — y luego habilita el logging SQL. El getter `Connection` (líneas 101-104) resuelve `_connection?.Value`. El sondeo de `DatabaseExists()` (líneas 247-262) llama hoy a la misma fábrica, por lo que el hook también dispara para conexiones de sonda. La documentación XML de `ISqlExecutorContext.OnConnectionOpened` declara `sp_set_session_context` como caso de uso, y `DaoContextExt.SetSessionContextAsync` opera sobre `context.Connection` — la combinación produce recursión infinita (ver proposal.md - Why).

## Goals / Non-Goals

**Goals:**

- Hacer el hook utilizable para su propósito documentado (SESSION_CONTEXT/RLS/auditoría) sin recursión ni fugas.
- Garantizar disparo único por conexión lógica, con exclusión de sondas y semántica fail-closed (decisiones 1-4 del consumidor, ya acordadas).
- Preservar el contrato transaccional vigente (`dal-transactions`): este cambio no toca la creación/enlistamiento de transacciones.

**Non-Goals:**

- No se añade el parámetro opcional `readOnly` (`@read_only = 1`) a `SetSessionContextAsync` — seguimiento opcional fuera de alcance salvo petición expresa.
- No se cambia la firma del hook síncrono `Action<ISqlExecutorContext>` (decisión 1).
- No se rediseña `DaoContextExt` más allá de documentación del patrón de uso.

## Decisions

### D1 — Publicación anticipada de la conexión en un campo dedicado

Se añade un campo privado `private volatile DbConnection? _openedConnection` en `DaoContext`. La fábrica lo asigna inmediatamente después de abrir la conexión y ANTES de invocar cualquier hook. El getter `Connection` pasa a resolver en orden: `_openedConnection ?? _connection?.Value ?? throw ObjectDisposedException`.

- **Por qué**: elimina la re-entrada en el Lazy por la misma hebra (el handler ya ve la conexión publicada) y por hebras cruzadas (otra hebra que toque `.Value` durante la fábrica queda bloqueada por el Monitor del Lazy hasta que la fábrica —incluidos los hooks— complete, cumpliendo el escenario de concurrencia del spec).
- **Alternativas descartadas**: (a) pasar la conexión como argumento del delegado (`Action<ISqlExecutorContext, DbConnection>`) — rompe la firma acordada en la decisión 1; (b) `LazyThreadSafetyMode.PublicationOnly` — no cachea excepciones, rompe la semántica fail-closed de la decisión 4; (c) reemplazar el Lazy por construcción ansiosa — cambia el contrato de conexión diferida existente.

### D2 — Extracción de `CreateConnectionCore()` para sondas

Se extrae un método privado `CreateConnectionCore()` que crea y abre la conexión (sin publicar el campo ni invocar hooks). La fábrica `CreateConnection()` lo reutiliza: core → publicar `_openedConnection` → `EnableSqlLogging` → hook síncrono → hook asíncrono → devolver. `DatabaseExists()` cambia su sonda a `CreateConnectionCore()` dentro del `using` existente.

- **Por qué**: la decisión 2 exige que las sondas jamás disparen el hook; hoy lo disparan porque comparten la fábrica.
- **Alternativa descartada**: distinguir en la fábrica mediante un parámetro booleano "es sonda" — contamina la fábrica y es propenso a errores de mantenimiento.

### D3 — Orden interno de la fábrica: logging antes del hook

`EnableSqlLogging(connection)` pasa a ejecutarse antes de los hooks (hoy es después). Así, el `sp_set_session_context` que emita el handler queda registrado en el log SQL, lo que es valioso para auditoría.

- **Alternativa descartada**: mantener el orden actual — los comandos del handler no quedarían logueados; el cambio es propuesto como default vetoable y ya fue aceptado en la conversación de diseño.

### D4 — Hook asíncrono bloqueante con `.GetAwaiter().GetResult()`

Nuevo miembro en `ISqlExecutorContext`: `Func<ISqlExecutorContext, Task>? OnConnectionOpenedAsync { get; set; }`. La fábrica, tras el hook síncrono, lo invoca y bloquea la hebra materializadora con `.GetAwaiter().GetResult()`.

- **Por qué**: el contrato de `Connection` es síncrono; la materialización no puede completarse hasta que el handler async termine (fail-closed). `.GetAwaiter().GetResult()` se prefiere sobre `.Result`/`.Wait()` para no envolver la excepción original en `AggregateException` y preservar la excepción cacheada del Lazy tal cual.
- **Alternativas descartadas**: (a) API síncrona completa (`GetConnectionAsync`) — rompería todo el surface existente de `IDaoContext`; (b) disparar el hook async sin bloquear (fire-and-forget) — viola fail-closed: operaciones de datos podrían ejecutarse antes de que el contexto de sesión quede establecido.

### D5 — Fail-closed vía cacheo natural del Lazy

En el `catch` de la fábrica: disponer la conexión abierta, asignar `null` a `_openedConnection`, y envolver en `DataAccessException` (se corrige además la fuga preexistente — hoy la conexión no se dispone en el catch). El Lazy `ExecutionAndPublication` cachea la excepción: todo acceso posterior a `Connection` la re-lanza. `Dispose()` también debe poner `_openedConnection = null`.

- **Por qué**: decisión 4 del consumidor — el handler de auditoría es fail-closed; el contexto no debe ejecutar operaciones sin el contexto de sesión establecido. El envenenamiento permanente es deliberado: el consumidor crea un contexto nuevo.
- **Alternativa descartada**: reintentar el handler en accesos posteriores (p. ej. flag de fallo que re-ejecuta la fábrica) — permitiría operaciones de datos sin auditoría durante la ventana de reintento; contradice fail-closed.

### D6 — Compatibilidad del interface

`ISqlExecutorContext` (netstandard2.1, `Nullable disable`) gana el miembro `OnConnectionOpenedAsync`. En el repo solo `DaoContext` lo implementa (vía `IDaoContext`); el resto de usos son constraints genéricos (`where TDAO : ISqlExecutorContext`) y no se ven afectados. XML docs en español: `<remarks>` del hook asíncrono advertirá del riesgo de interbloqueo si el handler captura un `SynchronizationContext` (p. ej. hilo de UI) y de la semántica de envenenamiento permanente.

## Risks / Trade-offs

- [Riesgo] Interbloqueo sync-over-async si el handler async captura un `SynchronizationContext` (hilo UI) y espera sobre él. → Mitigación: advertencia explícita en `<remarks>`; tarea de auditoría de `ConfigureAwait(false)` en las rutas asíncronas de Dal dentro del plan de implementación.
- [Riesgo] Envenenamiento permanente convierte una falla transitoria del handler (p. ej. timeout de red) en indisponibilidad del contexto. → Mitigación: comportamiento elegido (fail-closed para auditoría); documentado en XML docs; el costo es un contexto nuevo, operación barata.
- [Riesgo] Binary-breaking para implementadores externos de `ISqlExecutorContext`. → Mitigación: marcado como **BREAKING** en la proposal; en el repo no hay implementadores externos.
- [Riesgo] Hebra cruzada que accede `Connection` durante la fábrica queda bloqueada hasta el final de los hooks. → Aceptado: es exactamente el escenario de concurrencia requerido (los accesos esperan a que el hook complete).

## Migration Plan

1. Implementar en rama de feature con base `Dev-NET10`; TDD: primero los tests de regresión (recursión, envenenamiento, sonda, disparo único, orden de hooks) que fallan hoy.
2. Los consumidores internos no existen (ningún uso actual del hook en el repo) — sin migración interna.
3. Consumidores externos que implementen `ISqlExecutorContext`: recompilación requerida (miembro nuevo).
4. Reversión: revert del commit `feat` del cambio; sin persistencia ni esquema implicados.

## Open Questions

Ninguna — las cuatro decisiones de diseño relevantes (firma, exclusión de sondas, variante async, fail-closed) fueron resueltas con el consumidor en la fase de refinamiento.
