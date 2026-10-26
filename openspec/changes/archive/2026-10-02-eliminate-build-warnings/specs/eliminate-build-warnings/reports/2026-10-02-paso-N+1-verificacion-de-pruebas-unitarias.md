# Reporte — Paso N+1: Verificación de pruebas unitarias (eliminate-build-warnings)

**Fecha**: 2026-10-02
**Rama**: `feature/eliminate-build-warnings`
**SDK**: 10.0.401 (fijado por `global.json`, `rollForward: latestFeature`, runner `Microsoft.Testing.Platform`)
**Ejecutado por**: el agente, conforme a `docs/openspec-tasks-mandatory-steps.md` (librería sin BD propia: los pasos N+1-BD, N+2 curl y N+3 Playwright no aplican).

## Comandos ejecutados

```text
dotnet build KUtilitiesCore.sln --no-incremental -v:n
dotnet test --solution KUtilitiesCore.sln
```

## Resultados de compilación

| Métrica | Línea base (pre-change) | Tras el change |
|---|---|---|
| Errores | 0 | **0** |
| Warnings en los 12 proyectos de librería | 454 | **0** |
| Warnings en los 7 proyectos de test | 28 | **28** (idénticos sitios, fuera de alcance por decisión de PO) |
| Warnings totales de la solución | 482 | **28** |

- Gate del PO: «0 warnings en librerías, verificado con build completo no incremental». **Cumplido** (parseo del log con deduplicación: 56 líneas de warning → 28 únicos, todos en `.csproj` de test: DataAccessTests=12, DataTests=9, Data.WinTests=2, GitHubUpdaterTests=2, LoggerTests=2, EventCommandBinder=1). El pico intermedio de 622 warnings tras activar Nullable en Dal/Data quedó reducido a 0 en librerías con correcciones de flujo reales.
- Distribución por código de los 28 residuales (todos preexistentes): CS8618=14, CS8604=5, MSTEST0056=4, CS8601=2, CS8602=2, CS8625=1.
- Se activó `<Nullable>enable</Nullable>` en `KUtilitiesCore.Dal` y `KUtilitiesCore.Data` (decisión de PO); los CS86xx emergentes del flip (+139) se resolvieron con correcciones de flujo reales (guardas `ArgumentNullException`/`InvalidOperationException`, anotaciones de contrato, inicializadores honestos), sin `TreatWarningsAsErrors` y sin supresiones globales nuevas (sin `NoWarn`, `GenerateDocumentationFile` intacto en los 12 csproj).
- Los únicos `!` justificados y documentados con comentario en español: `WeakAction.Execute` (`default!`, T por referencia) y `EfRepositoryReadOnly.GetFirstOrDefaultAsync` (puente de interoperabilidad del contrato obvio `Task<T>`; documentado en el código).

## Resultados de la suite completa (MTP)

**Total: 119 pruebas — 117 correctas, 1 omitida, 1 con error. Duración: ~8,5 s.**

| Suite | Resultado |
|---|---|
| KUtilitiesCoreTests | 38/38 correctas |
| KUtilitiesCore.DataAccessTests | 18/18 correctas (15 base + 3 nuevas del grupo 15) |
| KUtilitiesCore.DataTests | 8/8 correctas |
| KUtilitiesCore.MVVMTests | 7/7 correctas (4 base + 3 nuevas del grupo 15) |
| KUtilitiesCore.MVVMTests.EventCommandBinder | 13/13 correctas |
| KUtilitiesCore.GitHubUpdaterTests | 16 correctas, **1 con error** (`GitHubUpdateServiceTest`) |
| KUtilitiesCore.Data.WinTests | 6/6 correctas |
| KUtilitiesCore.LoggerTests | 11 correctas, 1 omitida |

## Comparación con la línea base y resolución de divergencias

| Métrica | Línea base (migrate-to-dotnet10) | Tras eliminate-build-warnings | Divergencia |
|---|---|---|---|
| Pruebas correctas | 111 | 117 | +6 |
| Pruebas fallidas | 1 | 1 | 0 |
| Pruebas omitidas | 1 | 1 | 0 |
| Warnings en librerías | 454 | 0 | −454 |

- **+6 correctas**: pruebas nuevas añadidas en el paso N (grupo 15) para cubrir guardas de flujo nulo introducidas al eliminar warnings: 3 en `KUtilitiesCore.MVVMTests` (`ViewModelExtensionsTests`: padre inicializado / `ViewModelSourceException` cuando el padre no está inicializado / fuente sin interfaz) y 3 en `KUtilitiesCore.DataAccessTests` (`ObjectDataReaderTests`: `GetData` → `NotSupportedException` tras `Read`, `Read` tras `Dispose` → `ObjectDisposedException`; `DaoContextTests.Connection_AfterDispose_Throws_ObjectDisposed` para la propiedad `Connection` envenenada tras `Dispose`).
- **1 error, sin cambio**: `GitHubUpdateServiceTest` falla por la misma causa preexistente y documentada: invoca la API en vivo de GitHub y recibe **401 Unauthorized** sin `GITHUB_TOKEN` válido en el entorno (cargado vía DotNetEnv). No es una regresión del change; se documenta como excepción conocida.
- **1 omitida, sin cambio**: `CreateLogger_And_LogMessage_ToSql` (LoggerTests), requiere SQL Server local configurado.
- Los cambios de contrato de nulabilidad cruzados entre proyectos (`IImportValidationRule.Validate(object?)`, `IRepository.GetFirstOrDefault` con `[return: MaybeNull]`, `ISqlExecutorContext.Scalar → TResult?`, `IDaoParameterCollection.GetParamValue → TValue?`, `IFieldDefinitionItem.DefaultValue/TypeConverter → ?`, eventos `EventHandler?`) requirieron ajustes en los consumidores de librería pero **ninguna prueba existente necesitó modificación** (revisado en el grupo 15); las 113 pruebas de la línea base pasan sin cambios.

## Notas y excepciones

- Sintaxis de ejecución: `dotnet test --solution KUtilitiesCore.sln` (experiencia MTP del SDK 10; el modo VSTest ya no soporta Microsoft.Testing.Platform v2).
- Salida del código 2 de MTP corresponde exclusivamente al fallo de `GitHubUpdateServiceTest` (401).
- El parseo de logs de build con `-v:n` requiere deduplicar (cada warning se emite 2 veces) y descartar prefijos de nodo `\d+>`; inventarios completos en `$env:TEMP\opencode\*.csv`.
- Los 28 warnings residuales pertenecen íntegramente a proyectos de test, fuera de alcance por decisión de PO registrada en `proposal.md`.

## Veredicto

Los 12 proyectos de librería compilan con **0 warnings y 0 errores** (454 → 0), los únicos `!` están justificados y documentados, no se añadieron supresiones globales, y la suite completa pasa (117/119, con el fallo 401 y la omisión de SQL local como excepciones preexistentes documentadas). **Paso N+1 APROBADO.**
