# Design

## Context

Ver histórico en `proposal.md` (sección Why). Estado actual medido con `dotnet build KUtilitiesCore.sln --no-incremental`: 0 errores, 482 warnings; 454 en 12 librerías, 28 en 6 proyectos de test (fuera de alcance). Concentración: `Dal` ~175 y `Core` ~142. Las librerías ya migradas a net10.0; `DataAccess` es netstandard2.1 y `Logger` netstandard2.0. `Dal` y `Data` compilan hoy con `Nullable disable`, el resto de librerías con `Nullable enable`. El inventario completo por proyecto/código se capturó del build y sirve como línea base para medir progreso.

Restricciones del entorno que condicionan el diseño:

- El build es la puerta de verificación del repo (no existe lint/analyzer adicional).
- Tests de `Dal` usan mocks de `DbDataReader` (`CallBase = true`, `Read()`/`NextResult()` finitos) sin SQL Server real; la suite es la única red de seguridad contra regresiones de nulabilidad.
- Convención de docs XML en español, orientadas a propósito/"por qué", con `<example>` cuando el uso no es obvio.

## Goals / Non-Goals

**Goals:**

- Alcanzar 0 warnings en los 12 proyectos de librería, de forma incremental y verificable por proyecto.
- Corregir la nulabilidad de verdad (flujo de NULL), no maquillarla.
- Dejar la documentación XML pública completa, válida y en español.

**Non-Goals:**

- Refactors de diseño, renombrados o cambios de arquitectura ("touch only what the task needs").
- Proyectos de test, activación de `TreatWarningsAsErrors`, ni cambios de CI.
- Cambios de comportamiento en runtime; las firmas solo varían en anotaciones de nulabilidad.
- Habilitar Nullable en `DataAccess` y `DataAccess.Http` (mantienen `disable`; sus warnings son solo de docs o inexistentes).

## Decisions

### D1: Orden de ejecución — "primero cuantificar, luego de menor a mayor riesgo"

**Decisión**: (1) habilitar `Nullable enable` en `Dal` y `Data` y capturar el inventario emergente; (2) fixes puntuales globales (SYSLIB0051, docs malformadas CS157x/CS1711/CS1587); (3) librerías pequeñas en orden ascendente de warnings (`Encryption` 7, `MVVM.Messaging` 7, `GitHubUpdater` 10, `DataAccess` 16, `Logger` 18, `MVVM` 13, `EfCore` 13, `Data` 27, `Data.Win` 26); (4) `Dal` (~175 + emergentes) y `Core` (~142) al final.
**Racional**: el número real de CS86xx tras habilitar Nullable en `Dal` es desconocido hasta hacerlo; hacerlo primero fija la cifra y evita estimar sobre arena. Los proyectos pequeños generan momentum y patrones reutilizables para los grandes.
**Alternativas descartadas**: empezar por `Core` (mayor volumen de CS1591 puro) arriesga bloquearse en documentación masiva antes de validar los patrones de fix de nulabilidad.

### D2: Fix de nulabilidad por flujo, no por supresión

**Decisión**: para cada CS86xx, corregir según la naturaleza del warning: CS8618 (miembro no-null no inicializado) → inicializador perezoso, constructor, `required` o volver el tipo nullable si el dominio lo permite; CS8600/02/03/04 (conversión/desreferencia) → `ArgumentNullException.ThrowIfNull`, `is not null`, guardas o anotaciones; CS8766/CS8767 (mismatch de contrato) → alinear la anotación de la firma con la implementación, no al revés. `!` y `= null!` solo con comentario que justifique la invariante.
**Racional**: el objetivo es prevenir `NullReferenceException`, no silenciar el compilador. `Dal` es la capa crítica de acceso a datos.
**Alternativas descartadas**: `#pragma`/`NoWarn` — prohibido por la spec `build-hygiene`; reescritura completa de flujos — fuera del principio de cambio gradual.

### D3: CS1591 — documentar en español con criterio de "propósito y por qué"

**Decisión**: redactar `<summary>` orientado a propósito, `<param>`/`<returns>`/`<exception>` cuando apliquen, y `<example>` en APIs no obvias (p. ej. el fluent builder `DataReaderConverter.Create().WithResult<T>()`, el patrón strategy de `IMappingStrategy`). No generar texto de relleno tipo "Obtiene o establece X": mejor un summary corto y honesto que uno largo y vacío.
**Racional**: la convención del repo exige explicar el "por qué"; docs de relleno violan la convención aunque silencien el warning.
**Alternativas descartadas**: excluir CS1591 con `NoWarn` — rechazado por el PO (CS1591 entra en alcance).

### D4: Commits incrementales por proyecto, build limpio como gate de cada uno

**Decisión**: un commit por proyecto (o por familia de fix en los globales); cada commit debe dejar el build sin warnings nuevos ni regresiones y con la suite de tests en verde. El progreso se mide re-ejecutando el build completo y contando warnings por proyecto contra la línea base.
**Racional**: 454+ warnings no es gobernable en un solo cambio; permite revisar por avance y revertir puntualmente.
**Alternativas descartadas**: un mega-commit — ingobernable para review; un commit por warning — ruido histérico.

### D5: SYSLIB0051 — eliminar el ctor de serialización obsoleto

**Decisión**: quitar el constructor `Exception(SerializationInfo, StreamingContext)` de `EmptyDataSourceException` (y cualquier otro resto de serialización binaria en librerías) sin reemplazo; la serialización de excepciones por `BinaryFormatter` está obsoleta por diseño de .NET.
**Racional**: la API está marcada obsoleta con intención de desaparición; mantenerla solo pospone el warning.
**Alternativas descartadas**: supresión con `#pragma`/`NoWarn SYSLIB0051` — prohibido por la spec.

## Risks / Trade-offs

- [Al habilitar Nullable en `Dal`/`Data` puede emerger un volumen alto de CS86xx no presupuestado] → se ejecuta como primer paso (D1) para fijar la cifra real; si el volumen crece desproporcionadamente, se re-plantea el alcance con el PO antes de continuar.
- [Fixes de nulabilidad pueden introducir `NullReferenceException` nuevos en paths no cubiertos por tests] → priorizar guardas (`ThrowIfNull`) sobre supresiones; suite completa de tests como gate por commit (D4); en `Dal`, cubrir los gotchas documentados de mocking en AGENTS.md.
- [Volumen de CS1591 induce docs de baja calidad] → criterio D3 + code review por proyecto; el "gate" del commit es la ausencia del warning, no la longitud del texto.
- [CS8766/CS8767 en interfaces de `DataAccess`/`Logger` pueden propagar cambios de firma a las implementaciones (`Dal`, `EfCore`, `Http`)] → alinear anotaciones primero en la abstracción, luego propagar a los consumidores dentro del mismo commit para no dejar warnings cruzados entre proyectos.
- [Rama larga de vida con 12 proyectos tocados] → commits incrementales (D4) permiten aplicar progreso parcial incluso si el cambio se interrumpe.

## Migration Plan

No hay despliegue ni migración de datos: es una suite de librerías. Estrategia de rollback: `git revert` por commit (D4 garantiza granularidad). Los consumidores externos solo recompilan; las anotaciones de nulabilidad nuevas pueden generar warnings nuevos en consumidores que ya compilaban contra las firmas sin anotación — aceptable y deseable (revela problemas latentes), sin breaking change binario.

## Open Questions

- Ninguna que bloquee el diseño. El conteo definitivo de CS86xx emergentes en `Dal`/`Data` se resuelve ejecutando el Paso 1 de tasks.md y no altera ni el enfoque ni el desglose de tareas (solo la duración).
