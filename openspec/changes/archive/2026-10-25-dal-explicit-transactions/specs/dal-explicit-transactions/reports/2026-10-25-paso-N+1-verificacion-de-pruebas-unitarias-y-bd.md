# Reporte de verificación — Paso N+1: Pruebas unitarias y base de datos

## Datos generales

- **Fecha**: 2026-10-25
- **Cambio**: `dal-explicit-transactions` (transacciones explícitas en el UoW de `KUtilitiesCore.Dal`)
- **Agente**: opencode (openspec-apply-change), rama `feature/dal-explicit-transactions`
- **Norma aplicada**: `docs/openspec-tasks-mandatory-steps.md` — verificación reducida de BD (suite de librerías .NET, sin API HTTP ni frontend; Pasos N+2 y N+3 omitidos). Los tests de escritura usan exclusivamente objetos de ámbito de sesión (tablas `#temp`), por lo que no mutan estado persistente.

## Comandos ejecutados

```bash
# Suites específicas (durante la implementación, 2026-10-24)
dotnet test KUtilitiesCore.DataAccessTests --filter "FullyQualifiedName~DaoUowTransactionTests"   # 19/19
dotnet test KUtilitiesCore.DataAccessTests --filter "FullyQualifiedName~LoadRawRepoTest"          # 1/1
dotnet build KUtilitiesCore.sln                                                                   # solo warnings preexistentes

# Verificación del Paso N+1 (2026-10-25)
dotnet test KUtilitiesCore.DataAccessTests
dotnet test --solution KUtilitiesCore.sln
```

Consultas SQL de línea base y verificación posterior (vía conexión de solo lectura al servidor local):

```sql
SELECT name FROM tempdb.sys.objects WHERE name LIKE '%TempTx%';
SELECT name, create_date FROM master.sys.tables
WHERE name LIKE '%Temp%' OR name LIKE '%Test%' ORDER BY create_date DESC;
```

## Resultados de pruebas

### Suite específica del cambio (`DaoUowTransactionTests`)

- **Resultado**: 19/19 correctas, 0 errores, 0 omitidas — **1s 526ms**.
- Cubre: congelación de la pasividad del getter (4), API explícita `uow.BeginTransaction` (5), verificación de vitalidad D4 (3), integración contra SQL Server local con `#TempTx` (7: commit/rollback, `using` sin Commit → reversión, commit explícito, idempotencia de Commit/Rollback/Dispose, Dispose del UoW con transacción registrada, transacción de conexión ajena → `DataAccessException`).

### Regresión

- `DaoUOWTests.LoadRawRepoTest` (comandos sin enlistar / SP Gateway): **1/1 correcta, 997ms** — `DaoUOWTests.cs` sin modificaciones (sin cambios impulsados por fuera del alcance).

### Proyecto completo (`dotnet test KUtilitiesCore.DataAccessTests`, 2026-10-25)

- **Resultado**: **Correcta — total: 37, error: 0, correcto: 37, omitido: 0, duración: 1s 808ms**.

### Suite completa (`dotnet test --solution KUtilitiesCore.sln`, 2026-10-25)

- **Resultado**: total: **188**, error: **1**, correcto: **186**, omitido: **1**, duración: **8s 967ms** (MTP, SDK 10).
- 8 contenedores de prueba ejecutados; 7 en verde (`DataAccessTests` 37, `MVVMTests`, `KUtilitiesCoreTests`, `LoggerTests`, `MVVMTests.EventCommandBinder`, `DataTests`, `Data.WinTests`).
- **Único error — excepción preexistente documentada**: `GitHubUpdateServiceTest` (`KUtilitiesCore.GitHubUpdaterTests`) falla con `Assert.IsNotNull(release)` tras error de red `401 (Unauthorized)` contra la API en vivo de GitHub. Causa: falta de `GITHUB_TOKEN` válido en el entorno. Documentada en `AGENTS.md` como fallo preexistente, **no es una regresión** de este cambio.
- **Única omisión — por diseño**: `CreateLogger_And_LogMessage_ToSql` (`LoggerTests`, "Requiere SQL Server local configurado").

### Notas de inestabilidad (flaky)

- Durante la fase 3.x (2026-10-24) se observó **un** fallo transitorio de infraestructura MTP (no de pruebas): ejecución con `--logger "console;verbosity=normal"` terminó con código de salida 5, "No se ejecutaron pruebas", "Handshake failures", total: 0 — glitch de comunicación con el test host. El reintento inmediato con el comando estándar resultó en 19/19 correctas. No hubo fallos de prueba reales atribuibles al cambio en ninguna ejecución.
- Warnings de compilación observados: únicamente los preexistentes documentados (NU1902/NU1903 por `System.Security.Cryptography.Pkcs` 9.0.18 en `Dal`/proyectos de test, y CS86xx/MSTEST0056 dispersos en proyectos de test). Sin CS1591/CS1573 en los tipos tocados.

## Verificación del estado de la base de datos

- **Línea base previa** (2026-10-25, antes de las suites): conectividad OK al servidor local (`127.0.0.1`, SQL Server 15.0.2200.2 Developer Edition, servidor `ZERO`). `tempdb.sys.objects` con nombre `%TempTx%`: **0 filas**. `master.sys.tables` con nombre `%Temp%`/`%Test%`: **0 filas**. Sin artefactos residuales de ejecuciones previas.
- **Validación posterior** (2026-10-25, tras las suites completa y de solución): mismas consultas → `tempdb`: **0 filas**; `master`: **0 filas**. Estado idéntico a la línea base.
- **Estado restablecido**: sí, automáticamente. Todos los tests de escritura usan la tabla temporal de sesión `#TempTx`, que muere con la conexión al disponer los contextos (`using`/`Dispose`). Ninguna escritura toco estado persistente (`master`/`tempdb`/cualquier BD de usuario).
- **Acciones de restauración**: ninguna necesaria (0 acciones). Verificado por comparación de consultas antes/después.

## Resultado

**APROBADO** — Suite específica 19/19 y proyecto `DataAccessTests` 37/37 en verde; suite completa 186/188 correctas con 1 error preexistente documentado (`GitHubUpdateServiceTest`, requiere `GITHUB_TOKEN`) y 1 omisión por diseño (`CreateLogger_And_LogMessage_ToSql`). Base de datos sin residuos (antes/después idénticos). Sin bloqueos.
