# Reporte de verificación — Paso N+1: Pruebas unitarias y base de datos

## Datos generales

- **Fecha**: 2026-10-25
- **Cambio**: `dal-onconnectionopened-reentrancy` (re-entrada del hook `OnConnectionOpened` en `KUtilitiesCore.Dal`)
- **Agente**: opencode (openspec-apply-change), rama `feature/dal-onconnectionopened-reentrancy`
- **Norma aplicada**: `docs/openspec-tasks-mandatory-steps.md` — verificación reducida de BD (suite de librerías .NET, sin API HTTP ni frontend; Pasos N+2 y N+3 omitidos). Todo el SQL del cambio es de ámbito de sesión (`sp_set_session_context`, sondeos de lectura `SELECT 1` / `SESSION_CONTEXT(N'...')`) y muere con la conexión al disponer los contextos, por lo que no muta estado persistente.

## Comandos ejecutados

```bash
# Suites específicas (durante la implementación, 2026-10-25)
dotnet test KUtilitiesCore.DataAccessTests --filter "FullyQualifiedName~DaoContextOnConnectionOpenedTests"   # 8/8
dotnet test KUtilitiesCore.DataAccessTests --filter "FullyQualifiedName~LoadRawRepoTest"                      # 1/1
dotnet build KUtilitiesCore.Dal                                                                               # solo warnings preexistentes

# Verificación del Paso N+1 (2026-10-25)
dotnet test KUtilitiesCore.DataAccessTests
dotnet test --solution KUtilitiesCore.sln
```

Consultas SQL de línea base y verificación posterior (vía `sqlcmd -S localhost -E`, conexión de solo lectura):

```sql
SELECT @@SERVERNAME, @@VERSION;                                   -- conectividad
SELECT name FROM sys.databases ORDER BY name;                      -- inventario de BDs
SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE is_user_process = 1; -- sesiones usuario activas
```

## Resultados de pruebas

### Suite específica del cambio (`DaoContextOnConnectionOpenedTests`)

- **Resultado**: 8/8 correctas, 0 errores, 0 omitidas.
- Cubre: regresión de re-entrada (`OnConnectionOpened` → `SetSessionContextAsync` completa sin recursión, 1.1), exclusión de la sonda `DatabaseExists()` (hook no invocado, conexión lógica no materializada, 1.2), variante asíncrona `OnConnectionOpenedAsync` con orden determinista síncrono → asíncrono (1.3), fail-closed con disposición de conexión y envenenamiento con excepción cacheada (1.4), disparo único bajo concurrencia con 4 hilos compartiendo la misma conexión (4.1), E2E de sesión con `SESSION_CONTEXT(N'Usuario')` leído en una operación posterior sobre el mismo contexto (4.3) y E2E fail-closed que confirma que el contexto envenenado falla sin operaciones de BD (4.4).

### Regresión

- `DaoUOWTests.LoadRawRepoTest` (comandos sin enlistar / SP Gateway): **1/1 correcta** — `DaoUOWTests.cs` sin modificaciones (sin cambios impulsados por fuera del alcance; verificado además en 5.1 con búsqueda global de `OnConnectionOpened`/`DatabaseExists`/`CreateConnection`: ningún test preexistente configura hooks ni depende del comportamiento antiguo de sonda/fábrica).

### Proyecto completo (`dotnet test KUtilitiesCore.DataAccessTests`, 2026-10-25)

- **Resultado**: **Correcta — total: 45, error: 0, correcto: 45, omitido: 0, duración: 3s 044ms** (37 previas + 8 nuevas del cambio).

### Suite completa (`dotnet test --solution KUtilitiesCore.sln`, 2026-10-25)

- **Resultado**: total: **196**, error: **1**, correcto: **194**, omitido: **1**, duración: **21s 021ms** (MTP, SDK 10).
- 8 contenedores de prueba ejecutados; 7 en verde (`DataAccessTests` 45, `MVVMTests`, `KUtilitiesCoreTests`, `LoggerTests`, `MVVMTests.EventCommandBinder`, `DataTests`, `Data.WinTests`).
- **Único error — excepción preexistente documentada**: `GitHubUpdateServiceTest` (`KUtilitiesCore.GitHubUpdaterTests`) falla con `Assert.IsNotNull(release)` tras error de red `401 (Unauthorized)` contra la API en vivo de GitHub. Causa: falta de `GITHUB_TOKEN` válido en el entorno. Documentada en `AGENTS.md` como fallo preexistente, **no es una regresión** de este cambio.
- **Única omisión — por diseño**: `CreateLogger_And_LogMessage_ToSql` (`LoggerTests`, "Requiere SQL Server local configurado").

### Notas de inestabilidad (flaky)

- Ningún fallo transitorio observado en las ejecuciones del Paso N+1. Los tests de concurrencia (4.1) usan `ManualResetEventSlim`/`Barrier`/`SpinWait` con tiempos límite de 10s y resultaron estables.
- Warnings de compilación observados: únicamente los preexistentes documentados (NU1902/NU1903 por `System.Security.Cryptography.Pkcs` 9.0.18 en `Dal`/proyectos de test, y CS86xx dispersos en proyectos de test). Sin CS1591/CS1573 en los tipos tocados.

## Verificación del estado de la base de datos

- **Línea base previa** (2026-10-25, antes de las suites): conectividad OK al servidor local (`localhost`, SQL Server 15.0.2200.2 Developer Edition, servidor `ZERO`). `sys.databases`: `KUtilitiesLogTest`, `master`, `model`, `msdb`, `SiomaxDB`, `tempdb` (las dos primeras de ejecuciones preexistentes ajenas al cambio). Sesiones usuario activas: **2**. Sin artefactos residuales de ejecuciones previas atribuibles al cambio.
- **Validación posterior** (2026-10-25, tras las suites de proyecto y de solución): mismas consultas → inventario de BDs idéntico, sesiones usuario activas: **2**. Estado idéntico a la línea base.
- **Estado restablecido**: sí, automáticamente. Todos los tests del cambio materializan conexiones contra `master`/`localhost` con SQL de ámbito de sesión (`sp_set_session_context` + sondeos de lectura); el contexto de sesión muere con la conexión al disponer los contextos en `TestCleanup`. Ninguna escritura tocó estado persistente (`master`/`tempdb`/cualquier BD de usuario).
- **Acciones de restauración**: ninguna necesaria (0 acciones). Verificado por comparación de consultas antes/después.

## Resultado

**APROBADO** — Suite específica 8/8 y proyecto `DataAccessTests` 45/45 en verde; suite completa 194/196 correctas con 1 error preexistente documentado (`GitHubUpdateServiceTest`, requiere `GITHUB_TOKEN`) y 1 omisión por diseño (`CreateLogger_And_LogMessage_ToSql`). Base de datos sin residuos (antes/después idénticos). Sin bloqueos.
