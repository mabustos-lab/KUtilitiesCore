# Design — Transacciones 100% explícitas en KUtilitiesCore.Dal

## Context

Estado actual verificado del código (la motivación está en `proposal.md` — *Why*):

- `DaoUowContext.Transaction` (ll.54-65) contiene la init perezosa invertida; el flag `isTransactionCreated` (l.24) jamás se voltea y `DisposeTransaction` (ll.97-105) lo pone a `true` — segunda inversión — en una ruta inalcanzable (guardada por `Transaction != null`). Efecto observable actual: getter siempre null, autocommit total.
- Las primitivas **ya implementan el patrón objetivo**: `TransactionBase.Dispose(bool)` (ll.107-120) revierte automáticamente sin Commit y es idempotente; `ITransaction` lo documenta (l.15); todos los métodos execute de `DaoContext` aceptan `ITransaction? transaction = null` (ExecuteNonQuery l.272, ExecuteReader l.336, Scalar l.588, FillDataSet l.524, UpdateDataSet l.650 y sus variantes async); `CreateCommand` (l.169+) valida que la transacción pertenezca a la misma conexión y la enlista.
- Los atajos `protected Transaction` de `DaoRepositoryReadOnly<T>` (l.86) y `RawRepositorybase` (l.32) exponen el getter a todos los repositorios.
- `BulkOperationsService` (l.213) depende del default `Snapshot` de `IDaoContext.BeginTransaction` (ll.131-145) como fallback.
- Cobertura actual: solo `LoadRawRepoTest` en `DaoUOWTests.cs`; cero tests transaccionales. Patrones de test establecidos: `SecureConnectionBuilder` localhost/master/IntegratedSecurity, registro de `DbProviderFactories`, skips por variable de entorno de GitHub Actions y no-Windows.
- `DaoUowContext.DisposeTransaction` es internal; sin `InternalsVisibleTo`, los tests deben ejercerla vía superficie pública (`SaveChanges`, `Rollback`, `Dispose`, y la nueva `BeginTransaction`).
- Restricciones: TDD obligatorio; Dal compila con `Nullable enable`; documentación en español; Moq solo disponible en `KUtilitiesCore.DataAccessTests`.

## Goals / Non-Goals

**Goals:**

- Getter `DaoUowContext.Transaction` pasivo y estado interno coherente: sin lazy-init, sin flag muerto, sin dobles inversiones, sin fugas.
- API aditiva `DaoUnitOfWork.BeginTransaction(IsolationLevel)` con default `ReadCommitted` que registra la transacción en el UoW.
- Contrato `SaveChanges`/`Rollback` verificado por tests: 0 / no-op sin transacción viva registrada; confirmación (1) y reversión con transacción viva.
- Suite MSTest que congela todos los requisitos del spec, incluida la pasividad accediendo vía repositorios.
- XML docs en español con `<example>` del patrón `using` en los tipos tocados.

**Non-Goals:**

- `IUnitOfWork` (netstandard2.1), `EfUnitOfWork` y `DataAccess.Http`: intactos.
- Cambiar el default `Snapshot` de `IDaoContext.BeginTransaction` (del que depende el fallback de `BulkOperationsService`).
- Eliminar `IDaoUowContext.Transaction` (permanece como vía de inspección del estado transaccional).
- Transacciones distribuidas, `TransactionScope`, o una variante asíncrona de `BeginTransaction` (aditivas en el futuro si se piden).

## Decisions

### D1 — Getter pasivo en lugar de lazy-open corregido

**Elección**: `Transaction` devuelve `_transaction` tal cual (con verificación de vitalidad, ver D4); nunca invoca `BeginTransaction`.

**Alternativas descartadas**: (a) voltear la condición del flag para abrir la transacción al primer acceso — enlistaría implícitamente todos los comandos de todos los repositorios en una transacción `Snapshot`, rompiendo los SP Gateways (SPs con `BEGIN TRAN` interno) y los comandos no enlistados (SqlClient exige enlistamiento cuando la conexión tiene una transacción local pendiente); (b) lazy-open con default distinto — sigue siendo una transacción implícita, el PO la descartó de raíz.

### D2 — Eliminar el flag `isTransactionCreated`, no corregirlo

**Elección**: eliminar el campo y toda su maquinaria. El único estado del UoW pasa a ser `_transaction` (referencia viva o null).

**Razón**: el flag solo tenía sentido como bookkeeping de la lazy-init; sin lazy-init es estado muerto y una tercera oportunidad de inversión. Corregirlo (ponerlo a `false` en `DisposeTransaction`) dejaría código que aparenta tener propósito y no lo tiene.

### D3 — La conveniencia registra la transacción en el UoW

**Elección**: `DaoUnitOfWork.BeginTransaction(isolationLevel = IsolationLevel.ReadCommitted)` crea la transacción vía `IDaoContext.BeginTransaction` y la asigna al estado del UoW (`_transaction`).

**Razón**: da propósito coherente a la maquinaria existente (`SaveChanges`, `Rollback`, `DisposeTransaction`) en lugar de dejarla muerta, y mantiene el opt-in puro: quien no llama `BeginTransaction` conserva el autocommit total actual (decisión 3 y 4 del PO).

**Alternativa descartada**: azúcar pura sin registro — `SaveChanges` devolvería 0 por siempre y la promesa de atomicidad de los XML docs del UoW seguiría siendo falsa para todos.

### D4 — Verificación de vitalidad en el getter (referencia obsoleta)

**Elección**: si el consumidor finaliza la transacción registrada fuera del UoW (típico: fin del bloque `using` con Commit o Dispose), el getter devolverá null en lugar de la referencia muerta, detectado internamente vía `((TransactionBase)_transaction).GetTransactionObject() == null` (cast contenido en el ensamblado Dal, sin superficie nueva).

**Razón**: evita que `SaveChanges` devuelva un engañoso 1 llamando `Commit` sobre una transacción ya finalizada; el contrato "sin transacción viva → 0" (spec) queda veraz.

**Alternativa descartada**: eventos de finalización desde `TransactionBase` — sobreingeniería para el caso.

### D5 — Default `ReadCommitted` en la conveniencia; `Snapshot` intacto en el contexto

**Razón**: `Snapshot` lanza el error 3960 en conflictos de actualización, infla el version store con SPs largos y no es el nivel contra el que los SP Gateways están escritos. `IDaoContext.BeginTransaction` conserva `Snapshot` porque `BulkOperationsService` (l.213) usa su fallback por defecto — cambiarlo alteraría silenciosamente la semántica de las bulk operations.

### D6 — Verificación con seams públicos (sin `InternalsVisibleTo`)

- **Pasividad del getter** (incluida vía repositorios): mock de Moq sobre `IDaoContext`; verify de cero llamadas a `BeginTransaction` tras accesos repetidos directos y vía `Repository<T>()` / `RawRepository<TRepo>()`.
- **Semántica transaccional real** (dispose→rollback, commit, `SaveChanges` = 1, enlistamiento): contra SQL Server local (patrón `DaoUOWTests`), porque `TransactionBase` envuelve un `DbTransaction` real.
- **Mocks de `DbDataReader`**: `CallBase = true`, `Read()`/`NextResult()` con fin finito, `ObjectDisposedException` → `Assert.Inconclusive` (per AGENTS.md).
- **Skips heredados**: variable de entorno de GitHub Actions y no-Windows.

### D7 — Documentación XML en español con `<example>` canónico

En `IDaoUowContext.Transaction` (pasividad), `DaoUnitOfWork.BeginTransaction` (patrón), `ITransaction`/`TransactionBase` (refuerzo de dispose→rollback), `SaveChanges`/`Rollback` (contrato 0/no-op). Ejemplo canónico anclado a operaciones que ya aceptan el parámetro:

```csharp
using (var tx = uow.BeginTransaction())
{
    uow.Context.ExecuteNonQuery(sql, parameters, transaction: tx);
    tx.Commit(); // sin esto, el Dispose del using revierte automáticamente
}
```

## Risks / Trade-offs

- [Los atajos `protected` exponen el getter a cada repositorio] → el test de pasividad accede también vía `Repository<T>()` y `RawRepository<TRepo>()`, congelando el contrato.
- [Consumidor finaliza la tx registrada fuera del UoW] → D4: el getter reporta null tras la finalización y `DisposeTransaction` anula la referencia; sin stale-commit.
- [Tests de integración requieren SQL Server local] → mismo patrón de skip heredado de `DaoUOWTests` (GitHub Actions / no-Windows los omiten).
- [`SaveChanges` pasa de "siempre 0" a "1 con tx registrada"] → aditivo: el nuevo camino solo existe para quien llame `BeginTransaction` explícitamente; el código existente es inalcanzable para el cambio.
- [Cast interno a `TransactionBase` en el getter] → contenido en Dal; toda transacción registrada proviene de `IDaoContext.BeginTransaction`, que siempre devuelve `TransactionBase`; cubierto por test de contrato.

## Migration Plan

- **Despliegue**: cambio único y aditivo (neutralización del getter + API nueva + suite de tests + docs). Sin pasos de migración para consumidores: nadie que hoy confía en el autocommit requiere modificación.
- **Rollback**: revertir el commit restaura el estado anterior; como el getter ya era de facto pasivo (autocommit total), el rollback no altera comportamiento observable alguno para no-adoptantes.
