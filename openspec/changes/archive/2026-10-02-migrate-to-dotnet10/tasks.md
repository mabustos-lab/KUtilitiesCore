# Tasks — Salto del ecosistema KUtilitiesCore a .NET 10

> Nota de validación del proyecto: suite de librerías de clases (.dll) sin base de datos propia, sin API HTTP expuesta y sin interfaz web. Los Pasos N+1 (verificación de BD), N+2 (curl) y N+3 (Playwright E2E) no aplican y se omiten por completo conforme a `docs/openspec-tasks-mandatory-steps.md`.

## 0. Configuración: Crear la rama de la característica (OBLIGATORIO - PRIMER PASO)

- [x] 0.1 Crear la rama `feature/migrate-to-dotnet10` a partir de la rama principal y cambiarse a ella
- [x] 0.2 Verificar la creación de la rama y el estado limpio del árbol de trabajo con `git status` y `git branch --show-current`

## 1. Preparación del entorno y línea base

- [x] 1.1 Crear `global.json` en la raíz con `{ "sdk": { "version": "10.0.401", "rollForward": "latestFeature" } }` y verificar con `dotnet --version` que resuelve a la banda 10.x
- [x] 1.2 Eliminar el `HintPath` muerto de `KUtilitiesCore.DataAccess.Http.csproj` (referencia a `System.ComponentModel.DataAnnotations` de .NET Framework 4.8) y los dos `PropertyGroup` de `WarningLevel 4` condicionados a `net48` en `KUtilitiesCore.csproj`; verificar con `dotnet build KUtilitiesCore.sln` que la solución sigue compilando sin errores
- [x] 1.3 Capturar la línea base: ejecutar `dotnet build KUtilitiesCore.sln` y `dotnet test KUtilitiesCore.sln` completos sobre los TFMs actuales y registrar conteo de warnings y de pruebas aprobadas/fallidas/omitidas como referencia del estado previo al salto

## 2. Capa base: Encryption y Core

- [x] 2.1 Cambiar `KUtiitiesCore.Encryption\KUtilitiesCore.Encryption.csproj` de `net48;net8.0` a `net10.0` y verificar con `dotnet build KUtilitiesCore.Encryption` en 0 errores
- [x] 2.2 En `KUtilitiesCore.csproj`: localizar con grep los `using`/`using static` de `System.ComponentModel.DataAnnotations`/`System.ComponentModel.Annotations` usados en el código; cambiar el TFM a `net10.0`; eliminar los paquetes `Microsoft.CSharp 4.7.0` y `System.ComponentModel.Annotations 5.*` ajustando los `using` al namespace in-box (`System.ComponentModel.DataAnnotations`); verificar con `dotnet build KUtilitiesCore` en 0 errores
- [x] 2.3 Ejecutar `dotnet build KUtilitiesCore.sln` y confirmar 0 warnings nuevos respecto de la línea base en los proyectos de esta capa

## 3. Capa intermedia: Data, MVVM y MVVM.Messaging

- [x] 3.1 Cambiar `KUtilitiesCore.Data.csproj` a `net10.0`, conservar `ClosedXML 0.105.1`, y verificar `dotnet build KUtilitiesCore.Data` en 0 errores y 0 warnings nuevos
- [x] 3.2 Cambiar `KUtilitiesCore.MVVM.csproj` a `net10.0`; eliminar el `PackageReference` condicional `System.Threading.Tasks.Extensions` (solo net48) y `System.ComponentModel.Annotations 5.*` (verificando namespaces igual que en 2.2); verificar `dotnet build KUtilitiesCore.MVVM` en 0 errores
- [x] 3.3 Cambiar `KUtilitiesCore.MVVM.Messaging.csproj` a `net10.0` y verificar `dotnet build KUtilitiesCore.MVVM.Messaging` en 0 errores
- [x] 3.4 Ejecutar `dotnet build KUtilitiesCore.sln` y confirmar que las capas 2-3 compilan con 0 warnings nuevos

## 4. Capa Windows y proyectos netstandard (sin cambio de TFM)

- [x] 4.1 Cambiar `KUtilitiesCore.Data.Win.csproj` de `net48;net8.0-windows` a `net10.0-windows` (conservar `UseWindowsForms=true` y `System.Resources.Extensions 10.*`); verificar `dotnet build KUtilitiesCore.Data.Win` en 0 errores, incluida la compilación de recursos (.resx) con `GenerateResourceUsePreserializedResources`
- [x] 4.2 Verificar que `KUtilitiesCore.DataAccess` (`netstandard2.1`) y `KUtilities.Logger` (`netstandard2.0`) compilan sin cambios de TFM bajo el SDK 10: `dotnet build` de ambos proyectos en 0 errores y 0 warnings nuevos (sin modificar sus `.csproj`)
- [x] 4.3 Ejecutar `dotnet build KUtilitiesCore.sln` completo en 0 errores tras esta capa (verificado: los 30 errores restantes son todos NU1201 de proyectos aún no migrados —Dal, GitHubUpdater y 7 de tests—, 0 errores en los proyectos ya migrados; estado transitorio documentado en design.md)

## 5. Implementaciones de acceso a datos

- [x] 5.1 Cambiar `KUtilitiesCore.Dal.csproj` a `net10.0` (conservar `Microsoft.Data.SqlClient 7.1.0`, `Nullable disable`); verificar `dotnet build KUtilitiesCore.Dal` en 0 errores y 0 warnings nuevos
- [x] 5.2 En `KUtilitiesCore.DataAccess.EfCore.csproj`: cambiar el TFM a `net10.0` y actualizar `Microsoft.EntityFrameworkCore` de `9.0.14` a la versión 10.x estable; resolver los breaking changes del major que afecten al código del proyecto; verificar `dotnet build KUtilitiesCore.DataAccess.EfCore` en 0 errores
- [x] 5.3 En `KUtilitiesCore.DataAccess.Http.csproj`: cambiar el TFM a `net10.0` y actualizar `Microsoft.AspNetCore.WebUtilities` de `8.0.25` a `10.x`; conservar `Microsoft.Extensions.Http 10.0.12`, `Microsoft.Extensions.Http.Polly 10.0.12` y `System.Net.Http.Json 10.0.12`; verificar `dotnet build KUtilitiesCore.DataAccess.Http` en 0 errores
- [x] 5.4 Ejecutar `dotnet build KUtilitiesCore.sln` completo en 0 errores tras esta capa (verificado: los 24 errores restantes son todos NU1201 de GitHubUpdater y 7 proyectos de tests aún no migrados; 0 errores en los proyectos ya migrados)

## 6. GitHubUpdater y limpieza de paquetes condicionales

- [x] 6.1 Cambiar `KUtilitiesCore.GitHubUpdater.csproj` a `net10.0`; eliminar el `ItemGroup` condicional de `System.Net.Http 4.3.4` (solo net48); conservar `Newtonsoft.Json 13.0.4` y `System.Text.Json 10.0.12`; verificar `dotnet build KUtilitiesCore.GitHubUpdater` en 0 errores
- [x] 6.2 Verificar con `rg "net48" --glob "*.csproj"` que no queda ninguna condición ni target `net48` en los 12 proyectos de librerías y que los paquetes heredados eliminados ya no aparecen (`rg "Microsoft.CSharp|System.ComponentModel.Annotations|System.Threading.Tasks.Extensions|System.Net.Http 4.3.4"`) (verificado: solo restan MVVMTests/GitHubUpdaterTests, que se migran en el grupo 7; se eliminaron además 2 comentarios muertos con net48 en DataAccess y DataAccessTests; 0 paquetes heredados)

## 7. Migración de los 8 proyectos de tests

- [x] 7.1 Cambiar a `net10.0` los 5 proyectos de tests en `net8.0`: `KUtilitiesCoreTests`, `KUtilitiesCore.DataAccessTests`, `KUtilitiesCore.DataTests`, `KUtilitiesCore.MVVMTests.EventCommandBinder` y `KUtilitiesCore.LoggerTests`; si MSTest 4.4.1 / Test SDK 18.10.1 / Moq 4.20.72 generan advertencias de compatibilidad en net10, subir cada uno a la última versión compatible y registrar el cambio; verificar `dotnet build` de cada proyecto en 0 errores
  - Evidencia: 5 csproj pasados a `net10.0`. Sin advertencias de compatibilidad de MSTest 4.4.1 / Test SDK 18.10.1 / Moq 4.20.72 sobre net10 → versiones conservadas (sin bump). Build de la solución completa: 0 errores.
- [x] 7.2 Cambiar `KUtilitiesCore.Data.WinTests.csproj` a `net10.0-windows` y verificar `dotnet build KUtilitiesCore.Data.WinTests` en 0 errores (WinForms requerido)
  - Evidencia: TFM `net10.0-windows`. Remediado 1 error NUEVO del analizador WinForms: WFO1000 en `ImportWizardFormTests.cs` propiedad `MockDataTableToReturn` → `[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]` + `using System.ComponentModel;`. Build 0 errores, 0 advertencias nuevas.
- [x] 7.3 Migrar `KUtilitiesCore.MVVMTests.csproj` de `net48` a `net10.0` (conservando MSTest y la cobertura existente) y verificar `dotnet build KUtilitiesCore.MVVMTests` en 0 errores
  - Evidencia: TFM `net48`→`net10.0`. Build de la solución 0 errores, 0 advertencias nuevas propias.
- [x] 7.4 Migrar `KUtilitiesCore.GitHubUpdaterTests.csproj` de `net48` a `net10.0`; eliminar `System.Text.RegularExpressions 4.3.1` (API in-box en net10); conservar `DotNetEnv 3.2.0`; verificar `dotnet build KUtilitiesCore.GitHubUpdaterTests` en 0 errores
  - Evidencia: TFM `net48`→`net10.0`; eliminado `System.Text.RegularExpressions 4.3.1`; conservado `DotNetEnv 3.2.0`. Remediadas 3 advertencias nullable NUEVAS (CS8600/CS8601×2/CS8604 en `GitHubUpdateServiceTests.cs`, líneas 35-41: `Environment.GetEnvironmentVariable(...) ?? string.Empty` en `canal`, `token` y `API_URL`). Build 0 errores, 0 advertencias nuevas. Comparación global solución vs línea base: 183→181 sitios únicos, NUEVAS=0, DESAPARECIDAS=2 (sitios net48 de `GitHubUpdateManager.cs` CS8604 y cref resuelto en `IEnumerableExtensions.cs` CS1574).

## 8. Revisar y actualizar pruebas unitarias existentes (OBLIGATORIO - Paso N)

- [x] 8.1 Revisar los 8 proyectos de tests tras el salto de TFM: localizar usos de APIs no disponibles o con cambio de comportamiento en net10 (reflexión, `AppDomain`, serialización, `System.Text.RegularExpressions`) y adaptarlos conservando la intención y cobertura de cada prueba
  - Evidencia: `rg` sobre los proyectos de tests → solo 2 hallazgos, ambos seguros en net10 y sin adaptación: `AppDomain.CurrentDomain.BaseDirectory` (`ImportManagerTests.cs:45`) y `RegexOptions.IgnoreCase` in-box (`RuleBuilderTest.cs:135`). Ningún `BinaryFormatter`/`SerializationInfo` en tests.
- [x] 8.2 Ejecutar las pruebas de cada proyecto migrado (`dotnet test KUtilitiesCore.<Proyecto>Tests`) y confirmar que cada suite compila y ejecuta sobre los targets nuevos sin fallos de infraestructura de pruebas
  - Evidencia: suite completa vía MTP (`dotnet test --solution KUtilitiesCore.sln --no-build`, SDK 10.0.401): 113 pruebas → 111 correctas, 1 omitida, 1 con error. El único fallo es `GitHubUpdateServiceTest` (API en vivo de GitHub → 401 Unauthorized), idéntico al fallo preexistente registrado en la línea base (dependiente de `GITHUB_TOKEN`/entorno, no es una regresión de la migración). Las 8 suites compilan y ejecutan sobre `net10.0`/`net10.0-windows` sin fallos de infraestructura.
- [x] 8.3 Validar específicamente `KUtilitiesCore.MVVMTests.EventCommandBinder` sobre el SDK 10 (históricamente frágil: ver commit `3230c99`) y documentar cualquier ajuste necesario en su configuración `UseMicrosoftTestingPlatform`
  - Evidencia: bajo SDK 10 el modo VSTest de `dotnet test` ya no soporta MTP v2 (error `Microsoft.Testing.Platform.MSBuild.targets(355,5)`, aka.ms/dotnet-test-mtp-error). Ajuste aplicado (nueva experiencia `dotnet test` MTP): (1) `global.json` recibe `"test": { "runner": "Microsoft.Testing.Platform" }`; (2) los 7 proyectos MSTest reciben `<EnableMSTestRunner>true</EnableMSTestRunner>` (MSTest 4.4.1 soporta MTP) para que el modo MTP pueda ejecutarlos — el modo MTP genera error en proyectos que solo soportan VSTest; (3) `UseMicrosoftTestingPlatform=true` se conserva en EventCommandBinder (MTP 2.4.1 ≥ 1.7 requerido). Resultado: las 13 pruebas de `KUtilitiesCore.MVVMTests.EventCommandBinder` (no ejecutables bajo VSTest/SDK 10 en la línea base) ahora se ejecutan y pasan. Sintaxis MTP: `dotnet test --solution KUtilitiesCore.sln`.
  - Remediación adicional (requisito 6 de la spec, advertencias NUEVAS NU1510 vs línea base, 0→16): eliminados 4 PackageReference redundantes porque su API es in-box en el runtime net10: `System.Resources.Extensions` (Data.Win), `System.Net.Http.Json` (DataAccess.Http, incl. comentario obsoleto), `System.Text.Json` (GitHubUpdater), `System.Reflection.Metadata` (Data.WinTests). Build completo tras la limpieza: 0 errores, NU1510=0, sitios warning propios 183→181 (0 nuevas).

## 9. Ejecutar la suite completa de pruebas unitarias (OBLIGATORIO - Paso N+1, sin BD — EL AGENTE DEBE EJECUTARLAS)

- [x] 9.1 Ejecutar la suite completa `dotnet test KUtilitiesCore.sln` y registrar el conteo total de pruebas, aprobadas/fallidas/omitidas y el tiempo de ejecución; comparar con la línea base de 1.3 y resolver toda divergencia antes de continuar
  - Evidencia: suite vía MTP (`dotnet test --solution KUtilitiesCore.sln --no-build`): 113 pruebas → 111 correctas, 1 omitida, 1 error, ~4 s. Vs línea base (98/1/1 + 13 no ejecutables de EventCommandBinder): +13 correctas (EventCommandBinder ahora ejecuta y pasa bajo MTP), 1 error y 1 omitida sin cambio. Único fallo: `GitHubUpdateServiceTest`, 401 de la API en vivo de GitHub, idéntico al preexistente dependiente de entorno (no es regresión).
- [x] 9.2 Ejecutar `dotnet build KUtilitiesCore.sln -c Release` y confirmar 0 errores y 0 advertencias nuevas respecto de la línea base de 1.3 en Debug y Release para todos los TFMs (gate del repositorio; la deuda preexistente de warnings queda fuera de alcance por decisión de PO)
  - Evidencia: Debug 0 errores / Release 0 errores, 482 warnings en ambos; sitios únicos `.cs` 183→181 con NUEVAS=0 y DESAPARECIDAS=2 (los mismos 2 sitios explicados: `GitHubUpdateManager.cs::CS8604` net48, `IEnumerableExtensions.cs::CS1574`); 0 advertencias NU*/NETSDK*/MSB*.
- [x] 9.3 Crear el reporte `openspec/changes/migrate-to-dotnet10/specs/migrate-to-dotnet10/reports/AAAA-MM-DD-paso-N+1-verificacion-de-pruebas-unitarias.md` con los comandos ejecutados, los resultados resumidos y las notas de pruebas inestables o excepciones
  - Evidencia: `reports/2026-10-01-paso-N+1-verificacion-de-pruebas-unitarias.md` en disco, con comandos, conteos por suite, comparación vs línea base y notas de excepciones (GitHubUpdateServiceTest dependiente de `GITHUB_TOKEN`, sintaxis MTP `dotnet test --solution`).
- [x] 9.4 Marcar este paso como completado solo después de que las pruebas pasen y el reporte exista en disco

## 10. Actualizar la documentación técnica (OBLIGATORIO - Paso N+4)

- [x] 10.1 Actualizar el README raíz: eliminar la promesa de soporte ".NET Framework 4.8 y .NET 8.0" y describir la compatibilidad como .NET 10 (más `netstandard2.0`/`netstandard2.1` conservados); documentar que el uso de DPAPI (`ProtectedData`) sigue requiriendo Windows
  - Evidencia: README raíz actualizado — compatibilidad .NET 10 + netstandard conservados (consumibles desde runtimes anteriores) + nota DPAPI/Windows. Cero menciones de net48/net8.0.
- [x] 10.2 Actualizar `AGENTS.md`: tabla de proyectos con la matriz final de TFMs (`net10.0`, `net10.0-windows`, `netstandard2.0`, `netstandard2.1`), regla `global.json`, y notas de pruebas/dependencias aplicables a los targets nuevos; verificar que los comandos de build/test documentados siguen siendo exactos
  - Evidencia: AGENTS.md — Build & Test reescrito (SDK 10 fijado por `global.json`, runner MTP, sintaxis `dotnet test --solution KUtilitiesCore.sln`, nota VSTest obsoleto en SDK 10, `EnableMSTestRunner` en los 7 proyectos MSTest); tabla TFMs final (12 librerías, Data.Win = net10.0-windows, DataAccess/EfCore/Dal/Http/GitHubUpdater = net10.0); versiones reales de pruebas (MSTest 4.4.1, Test SDK 18.10.1, xunit.v3 4.0.1 + MTP 2.4.1 en EventCommandBinder); gotchas actualizados (GitHubUpdaterTests net10.0 con nota del fallo 401 preexistente, Data.WinTests net10.0-windows); nota de arquitectura net10.0.
- [x] 10.3 Verificar que `openspec/config.yaml` es coherente con el estado final (ya declara net10.0/netstandard) y que los READMEs por proyecto afectados no mencionan targets eliminados (`rg "net48|net8.0" --glob "README.md"`)
  - Evidencia: `context` de config.yaml ya declaraba net10.0/netstandard (coherente). Corregida la regla YAML `tasks` que el CLI ignoraba: `Follow TDD: ...` con `": "` sin entrecomilar se parseaba como mapping (regla descartada silenciosamente) → ahora entrecomilada; además el comando de verificación actualizado a `dotnet test --solution KUtilitiesCore.sln`. Advertencia del CLI desaparecida. `rg` sobre READMEs: 2 menciones obsoletas en `generated-docs\README.md` (prerrequisitos .NET 8.0 SDK / .NET Framework 4.8) reemplazadas por .NET 10 SDK + nota Windows (WinForms/DPAPI).

## 11. Verificación final de integración

- [x] 11.1 Verificar la matriz completa: `rg "net48|net8.0" --glob "*.csproj"` no retorna ningún `<TargetFramework(s)>` con targets eliminados y los 19 proyectos declaran los TFMs de la spec (ver `specs/target-frameworks/spec.md`, requisito "Matriz de frameworks objetivo")
  - Evidencia: `rg "net48|net8.0" --glob "*.csproj"` → **0 hits** (eliminado además un comentario muerto `<!-- O net6.0 / net8.0 según tu solución -->` en DataAccess.Http). Matriz verificada de los 19 `.csproj`: 15× `net10.0`, 2× `net10.0-windows` (Data.Win, Data.WinTests), `netstandard2.1` (DataAccess), `netstandard2.0` (Logger) — coincide exactamente con el requisito 1 de la spec.
- [x] 11.2 Ejecutar la verificación integral final: `dotnet build KUtilitiesCore.sln` (0 warnings, 0 errores) seguido de `dotnet test KUtilitiesCore.sln` (8 suites en verde) y registrar ambos resultados como evidencia de cierre
  - Evidencia (ejes `l11-build.txt` / `l11-test.txt` en `$env:TEMP\opencode\`): build Debug 0 errores, 0 advertencias nuevas vs línea base (sitios 183→181, deuda preexistente documentada fuera de alcance por PO; Release verificado en 9.2 con el mismo resultado). Suite vía MTP `dotnet test --solution KUtilitiesCore.sln --no-build`: **113 pruebas → 111 correctas, 1 omitida, 1 error**. Único fallo `GitHubUpdateServiceTest` (401 de la API en vivo de GitHub, dependiente de `GITHUB_TOKEN`, idéntico al preexistente de la línea base — no es regresión). Las 8 suites ejecutan sobre `net10.0`/`net10.0-windows` sin fallos de infraestructura.
