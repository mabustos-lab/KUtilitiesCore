# Línea base previa al salto a .NET 10 (tarea 1.3)

Fecha: 2026-10-01 · SDK: 10.0.401 (global.json recién creado) · Rama: feature/migrate-to-dotnet10 · TFMs actuales (pre-migración).

## Build — `dotnet build KUtilitiesCore.sln --no-incremental`

- **0 errores, 712 advertencias** (recuento del resumen; ~1424 líneas raw por duplicación de TFMs).
- Desglose por código (líneas raw, duplicadas por TFM):

| Código | Líneas | Significado |
|---|---|---|
| CS1591 | 856 | Falta comentario XML en miembro público visible |
| CS8625/CS8618/CS8600/CS8604/CS8602/CS8603/CS8613/CS8766/CS8767 | ~350 | Advertencias de tipos de referencia que aceptan NULL |
| CS1570/CS1573/CS1572/CS1574/CS1711/CS1587 | 122 | Defectos de documentación XML |
| MSTEST0056 | 8 | Usar `DisplayName` en lugar de cadena en `TestMethod` |
| SYSLIB0051 | 2 | API obsoleta (tipo de excepción en tiempo de ejecución) |

- Muestra por proyecto: `KUtilitiesCore` net8.0 = 298 warnings, net48 = 296 (distribución pareja: la deuda NO es solo de net48).
- **Decisión de PO confirmada en ejecución**: el gate final es "0 errores y 0 advertencias NUEVAS respecto de esta línea base"; la deuda preexistente queda fuera de alcance (cambio aparte).

## Tests — `dotnet test KUtilitiesCore.sln --no-build`

| Proyecto | TFM | Aprobadas | Fallidas | Omitidas | Total |
|---|---|---|---|---|---|
| KUtilitiesCore.LoggerTests | net8.0 | 11 | 0 | 1 | 12 |
| KUtilitiesCoreTests | net8.0 | 38 | 0 | 0 | 38 |
| KUtilitiesCore.MVVMTests | net48 | 4 | 0 | 0 | 4 |
| KUtilitiesCore.Data.WinTests | net8.0(-windows) | 6 | 0 | 0 | 6 |
| KUtilitiesCore.DataAccessTests | net8.0 | 15 | 0 | 0 | 15 |
| KUtilitiesCore.GitHubUpdaterTests | net48 | 16 | 1 | 0 | 17 |
| KUtilitiesCore.DataTests | net8.0 | 8 | 0 | 0 | 8 |
| KUtilitiesCore.MVVMTests.EventCommandBinder | net8.0 | — | — | — | NO EJECUTABLE |

**Total ejecutable: 98 aprobadas, 1 fallida, 1 omitida.**

Excepciones conocidas de la línea base (NO causadas por la migración):

1. `GitHubUpdateServiceTest` (GitHubUpdaterTests) falla con **401 Unauthorized** contra la API real de GitHub — depende de credenciales/entorno (DotNetEnv); se trata de una prueba de integración de red.
2. `KUtilitiesCore.MVVMTests.EventCommandBinder` **no puede ejecutarse vía VSTest** bajo el SDK 10: "Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK" — incompatibilidad MTP/VSTest preexistente (ver commit 3230c99); la abordará la tarea 8.3 con la nueva experiencia `dotnet test` (MTP).

## Referencia para el cierre (9.1)

Tras el salto, la suite completa debe mantener: 0 errores de compilación, sin advertencias nuevas respecto de 712, y el perfil de pruebas equivalente (las 2 excepciones de la línea base se documentan y tratan por separado; EventCommandBinder debe quedar EJECUTABLE vía MTP tras 8.3).
