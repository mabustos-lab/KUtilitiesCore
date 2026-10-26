# Proposal — Transacciones 100% explícitas en KUtilitiesCore.Dal

## Why

El getter `DaoUowContext.Transaction` contiene una init perezosa con la condición invertida: el flag `isTransactionCreated` arranca en `false` y solo puede voltearse dentro del propio getter o en `DisposeTransaction` (que además lo pone a `true` — segunda inversión), rutas que hoy son inalcanzables. Resultado neto: el UoW **nunca** abre transacciones (autocommit total), `SaveChanges()` siempre devuelve 0 y `Rollback()` es un no-op, mientras la documentación XML del UoW promete una atomicidad que la implementación no entrega.

El código inerte no es inocuo: es una trampa. Un "arreglo" ingenuo (voltear la condición para abrir la transacción al primer uso) enlistaría de golpe todos los comandos de todos los repositorios en una transacción `Snapshot` implícita, rompiendo los SP Gateways de las aplicaciones consumidoras (SPs con `BEGIN TRAN` interno, comandos no enlistados que SqlClient rechaza cuando la conexión tiene una transacción local pendiente) y el fallback de `BulkOperationsService`.

El patrón correcto — transacción explícita en un bloque `using`, con `Commit` explícito y Dispose sin Commit → Rollback automático, pasada por parámetro **opcional** a cada operación — ya existe completo en las primitivas del Dal (`ITransaction`/`TransactionBase` implementan y documentan la semántica de dispose; todos los métodos execute de `IDaoContext` aceptan `ITransaction?` opcional), pero no tiene especificación, no tiene tests, y tiene una trampa en el medio. Este cambio cementa el contrato explícito y neutraliza la trampa.

## What Changes

- **Neutralización de la trampa**: el getter `DaoUowContext.Transaction` se vuelve pasivo — devuelve exclusivamente la transacción registrada explícitamente y jamás invoca `BeginTransaction`. Se elimina (o se corrige a estado inerte) el flag `isTransactionCreated` y se corrige la doble inversión de `DisposeTransaction` (debe dejar el contexto "sin transacción", idempotente, sin fugas).
- **API aditiva de conveniencia**: `DaoUnitOfWork.BeginTransaction(IsolationLevel = IsolationLevel.ReadCommitted)` que crea la transacción vía `IDaoContext.BeginTransaction` y la **registra** en el UoW, haciendo coherentes `SaveChanges()` (confirma y devuelve 1) y `Rollback()` (revierte) solo para quien la adopte. El default es `ReadCommitted` — apropiado para flujos con SP Gateways — y **no** `Snapshot`.
- **Contrato de no-op preservado**: sin transacción registrada viva, `SaveChanges()` devuelve 0 y `Rollback()` es no-op silencioso (comportamiento actual). Los consumidores que hoy confían en el autocommit total no notan ningún cambio (opt-in puro).
- **Intangibles por decisión explícita**: `IUnitOfWork` (netstandard2.1), `EfUnitOfWork`, implementación Http y el default `Snapshot` de `IDaoContext.BeginTransaction` (del que depende el fallback de `BulkOperationsService`) quedan fuera del alcance.
- **Cobertura TDD**: suite MSTest en `KUtilitiesCore.DataAccessTests` que congela la pasividad del getter, el patrón `using` (commit explícito / dispose→rollback), el no-op de `SaveChanges` sin transacción y la regresión de comandos sin enlistar (SP Gateways intactos).
- **Documentación**: XML docs en español en los tipos tocados, con `<example>` del patrón `using`.

Sin cambios **BREAKING**: toda la superficie nueva es aditiva y el único cambio de comportamiento (la pasividad del getter) coincide con el comportamiento observable actual.

## Capabilities

### New Capabilities

- `dal-transactions`: Contrato transaccional de `KUtilitiesCore.Dal` — creación explícita de transacciones (vía `IDaoContext.BeginTransaction` y la conveniencia `DaoUnitOfWork.BeginTransaction`), enlistamiento opcional por parámetro en las operaciones, semántica de finalización (`Commit` explícito; Dispose/Rollback sin Commit → reversión automática), pasividad del UoW (jamás abre transacciones implícitas) y comportamiento de `SaveChanges`/`Rollback` según exista una transacción registrada viva.

### Modified Capabilities

(ninguno — no existe especificación previa de transacciones en `openspec/specs/`)

## Impact

- **Código**: `KUtilitiesCore.Dal` — `UOW/DaoUowContext.cs` (getter, flag, `DisposeTransaction`), `UOW/DaoUnitOfWork.cs` (conveniencia aditiva), docs XML de `UOW/IDaoUowContext.cs`, `ITransaction.cs` y `TransactionBase.cs` (solo documentación, la semántica de dispose ya está implementada). Tests en `KUtilitiesCore.DataAccessTests`.
- **APIs públicas**: un método nuevo en `DaoUnitOfWork` (`BeginTransaction`); documentación formal de la pasividad de `IDaoUowContext.Transaction`. `IUnitOfWork`, `EfUnitOfWork`, `DataAccess.Http` intactos.
- **Dependencias**: ninguna nueva; `Microsoft.Data.SqlClient` se conserva.
- **Sistemas**: SQL Server local para los tests de integración (`SecureConnectionBuilder` localhost, mismo patrón que `DaoUOWTests.cs`); verificación local con `dotnet build KUtilitiesCore.sln` y `dotnet test --solution KUtilitiesCore.sln`.
- **Consumidores**: los flujos con SP Gateways y todo consumidor del autocommit actual no requieren modificación alguna; la adopción de transacciones es explícita y opt-in.
