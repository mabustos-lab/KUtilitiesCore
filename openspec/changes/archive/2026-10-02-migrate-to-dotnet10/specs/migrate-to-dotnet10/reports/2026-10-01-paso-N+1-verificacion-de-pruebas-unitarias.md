# Reporte — Paso N+1: Verificación de pruebas unitarias (migrate-to-dotnet10)

**Fecha**: 2026-10-01
**Rama**: `feature/migrate-to-dotnet10`
**SDK**: 10.0.401 (fijado por `global.json`, `rollForward: latestFeature`, runner `Microsoft.Testing.Platform`)
**Ejecutado por**: el agente, conforme a `docs/openspec-tasks-mandatory-steps.md` (sin BD propia: los pasos N+1-BD, N+2 curl y N+3 Playwright no aplican).

## Comandos ejecutados

```text
dotnet build KUtilitiesCore.sln -c Debug
dotnet test --solution KUtilitiesCore.sln --no-build
dotnet build KUtilitiesCore.sln -c Release
```

## Resultados de compilación

| Config | Errores | Warnings (total líneas) | Sitios únicos `.cs` vs línea base | Nuevos | Desaparecidos |
|---|---|---|---|---|---|
| Debug   | 0 | 482 | 183 → 181 | **0** | 2 |
| Release | 0 | 482 | 183 → 181 | **0** | 2 |

- Gate del PO: «0 errores + 0 advertencias nuevas respecto de la línea base». Cumplido en ambas configuraciones.
- Los 2 sitios desaparecidos son mejoras del salto, no regresiones: `GitHubUpdateManager.cs::CS8604` (sitio de la era net48) y `IEnumerableExtensions.cs::CS1574` (cref resuelto en net10).
- 0 advertencias NU*/NETSDK*/MSB* (NU1510 remediado en el grupo 8 al eliminar 4 PackageReference redundantes).
- La deuda de warnings preexistente (CS1591, CS86xx, CS157x, MSTEST0056, SYSLIB0051/0052 documentada en la línea base de 712 sitios sin TFM-duplicar) queda fuera de alcance por decisión de PO.

## Resultados de la suite completa (MTP)

**Total: 113 pruebas — 111 correctas, 1 omitida, 1 con error. Duración: ~4 s.**

| Suite | Resultado |
|---|---|
| KUtilitiesCoreTests | 38/38 correctas |
| KUtilitiesCore.DataAccessTests | 15/15 correctas |
| KUtilitiesCore.DataTests | 8/8 correctas |
| KUtilitiesCore.MVVMTests | 4/4 correctas |
| KUtilitiesCore.MVVMTests.EventCommandBinder | 13/13 correctas |
| KUtilitiesCore.GitHubUpdaterTests | 16 correctas, **1 con error** (`GitHubUpdateServiceTest`) |
| KUtilitiesCore.Data.WinTests | 6/6 correctas |
| KUtilitiesCore.LoggerTests | 11 correctas, 1 omitida |

## Comparación con la línea base (1.3) y resolución de divergencias

| Métrica | Línea base | Tras la migración | Divergencia |
|---|---|---|---|
| Pruebas correctas | 98 | 111 | +13 |
| Pruebas fallidas | 1 | 1 | 0 |
| Pruebas omitidas | 1 | 1 | 0 |
| No ejecutables | 13 (EventCommandBinder) | 0 | −13 |

- **+13 correctas**: en la línea base `KUtilitiesCore.MVVMTests.EventCommandBinder` no era ejecutable bajo el SDK 10 (incompatibilidad VSTest/MTP preexistente, ver commit `3230c99`). Tras el ajuste a la nueva experiencia `dotnet test` MTP (`"test": { "runner": "Microsoft.Testing.Platform" }` en `global.json`, `<EnableMSTestRunner>true</EnableMSTestRunner>` en los 7 proyectos MSTest, `UseMicrosoftTestingPlatform` conservado en EventCommandBinder), las 13 pruebas ejecutan y pasan. Divergencia resuelta a favor de la migración.
- **1 error, sin cambio**: `GitHubUpdateServiceTest` falla en ambos estados por la misma causa: la prueba invoca la API en vivo de GitHub y recibe **401 Unauthorized**; es dependiente de `GITHUB_TOKEN`/entorno (cargado vía DotNetEnv) y NO es una regresión de la migración. Ver reporte de línea base `2026-10-01-linea-base-pre-migracion.md`.
- **1 omitida, sin cambio**: prueba omitida de LoggerTests, idéntica a la línea base.

## Notas y excepciones

- Sintaxis real de ejecución: bajo el SDK 10 la nueva experiencia MTP requiere `dotnet test --solution KUtilitiesCore.sln` (equivalente funcional del `dotnet test KUtilitiesCore.sln` citado en la spec; documentado en la evidencia de la tarea 8.3).
- Salida de consola de MSTest desplaza la etiqueta del ensamblado una línea arriba; los conteos por suite se mapean por orden, no por etiqueta.
- Salida del código 2 de MTP corresponde exclusivamente al fallo de `GitHubUpdateServiceTest`.

## Veredicto

Suite en verde en los targets `net10.0`/`net10.0-windows` (más los netstandard conservados), con el único fallo preexistente dependiente del entorno. Gate de compilación cumplido en Debug y Release. **Paso N+1 APROBADO.**
