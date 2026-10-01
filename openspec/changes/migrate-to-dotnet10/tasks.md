# Tasks — Salto del ecosistema KUtilitiesCore a .NET 10

> Nota de validación del proyecto: suite de librerías de clases (.dll) sin base de datos propia, sin API HTTP expuesta y sin interfaz web. Los Pasos N+1 (verificación de BD), N+2 (curl) y N+3 (Playwright E2E) no aplican y se omiten por completo conforme a `docs/openspec-tasks-mandatory-steps.md`.

## 0. Configuración: Crear la rama de la característica (OBLIGATORIO - PRIMER PASO)

- [ ] 0.1 Crear la rama `feature/migrate-to-dotnet10` a partir de la rama principal y cambiarse a ella
- [ ] 0.2 Verificar la creación de la rama y el estado limpio del árbol de trabajo con `git status` y `git branch --show-current`

## 1. Preparación del entorno y línea base

- [ ] 1.1 Crear `global.json` en la raíz con `{ "sdk": { "version": "10.0.401", "rollForward": "latestFeature" } }` y verificar con `dotnet --version` que resuelve a la banda 10.x
- [ ] 1.2 Eliminar el `HintPath` muerto de `KUtilitiesCore.DataAccess.Http.csproj` (referencia a `System.ComponentModel.DataAnnotations` de .NET Framework 4.8) y los dos `PropertyGroup` de `WarningLevel 4` condicionados a `net48` en `KUtilitiesCore.csproj`; verificar con `dotnet build KUtilitiesCore.sln` que la solución sigue compilando sin errores
- [ ] 1.3 Capturar la línea base: ejecutar `dotnet build KUtilitiesCore.sln` y `dotnet test KUtilitiesCore.sln` completos sobre los TFMs actuales y registrar conteo de warnings y de pruebas aprobadas/fallidas/omitidas como referencia del estado previo al salto

## 2. Capa base: Encryption y Core

- [ ] 2.1 Cambiar `KUtiitiesCore.Encryption\KUtilitiesCore.Encryption.csproj` de `net48;net8.0` a `net10.0` y verificar con `dotnet build KUtilitiesCore.Encryption` en 0 errores
- [ ] 2.2 En `KUtilitiesCore.csproj`: localizar con grep los `using`/`using static` de `System.ComponentModel.DataAnnotations`/`System.ComponentModel.Annotations` usados en el código; cambiar el TFM a `net10.0`; eliminar los paquetes `Microsoft.CSharp 4.7.0` y `System.ComponentModel.Annotations 5.*` ajustando los `using` al namespace in-box (`System.ComponentModel.DataAnnotations`); verificar con `dotnet build KUtilitiesCore` en 0 errores
- [ ] 2.3 Ejecutar `dotnet build KUtilitiesCore.sln` y confirmar 0 warnings nuevos respecto de la línea base en los proyectos de esta capa

## 3. Capa intermedia: Data, MVVM y MVVM.Messaging

- [ ] 3.1 Cambiar `KUtilitiesCore.Data.csproj` a `net10.0`, conservar `ClosedXML 0.105.1`, y verificar `dotnet build KUtilitiesCore.Data` en 0 errores y 0 warnings nuevos
- [ ] 3.2 Cambiar `KUtilitiesCore.MVVM.csproj` a `net10.0`; eliminar el `PackageReference` condicional `System.Threading.Tasks.Extensions` (solo net48) y `System.ComponentModel.Annotations 5.*` (verificando namespaces igual que en 2.2); verificar `dotnet build KUtilitiesCore.MVVM` en 0 errores
- [ ] 3.3 Cambiar `KUtilitiesCore.MVVM.Messaging.csproj` a `net10.0` y verificar `dotnet build KUtilitiesCore.MVVM.Messaging` en 0 errores
- [ ] 3.4 Ejecutar `dotnet build KUtilitiesCore.sln` y confirmar que las capas 2-3 compilan con 0 warnings nuevos

## 4. Capa Windows y proyectos netstandard (sin cambio de TFM)

- [ ] 4.1 Cambiar `KUtilitiesCore.Data.Win.csproj` de `net48;net8.0-windows` a `net10.0-windows` (conservar `UseWindowsForms=true` y `System.Resources.Extensions 10.*`); verificar `dotnet build KUtilitiesCore.Data.Win` en 0 errores, incluida la compilación de recursos (.resx) con `GenerateResourceUsePreserializedResources`
- [ ] 4.2 Verificar que `KUtilitiesCore.DataAccess` (`netstandard2.1`) y `KUtilities.Logger` (`netstandard2.0`) compilan sin cambios de TFM bajo el SDK 10: `dotnet build` de ambos proyectos en 0 errores y 0 warnings nuevos (sin modificar sus `.csproj`)
- [ ] 4.3 Ejecutar `dotnet build KUtilitiesCore.sln` completo en 0 errores tras esta capa

## 5. Implementaciones de acceso a datos

- [ ] 5.1 Cambiar `KUtilitiesCore.Dal.csproj` a `net10.0` (conservar `Microsoft.Data.SqlClient 7.1.0`, `Nullable disable`); verificar `dotnet build KUtilitiesCore.Dal` en 0 errores y 0 warnings nuevos
- [ ] 5.2 En `KUtilitiesCore.DataAccess.EfCore.csproj`: cambiar el TFM a `net10.0` y actualizar `Microsoft.EntityFrameworkCore` de `9.0.14` a la versión 10.x estable; resolver los breaking changes del major que afecten al código del proyecto; verificar `dotnet build KUtilitiesCore.DataAccess.EfCore` en 0 errores
- [ ] 5.3 En `KUtilitiesCore.DataAccess.Http.csproj`: cambiar el TFM a `net10.0` y actualizar `Microsoft.AspNetCore.WebUtilities` de `8.0.25` a `10.x`; conservar `Microsoft.Extensions.Http 10.0.12`, `Microsoft.Extensions.Http.Polly 10.0.12` y `System.Net.Http.Json 10.0.12`; verificar `dotnet build KUtilitiesCore.DataAccess.Http` en 0 errores
- [ ] 5.4 Ejecutar `dotnet build KUtilitiesCore.sln` completo en 0 errores tras esta capa

## 6. GitHubUpdater y limpieza de paquetes condicionales

- [ ] 6.1 Cambiar `KUtilitiesCore.GitHubUpdater.csproj` a `net10.0`; eliminar el `ItemGroup` condicional de `System.Net.Http 4.3.4` (solo net48); conservar `Newtonsoft.Json 13.0.4` y `System.Text.Json 10.0.12`; verificar `dotnet build KUtilitiesCore.GitHubUpdater` en 0 errores
- [ ] 6.2 Verificar con `rg "net48" --glob "*.csproj"` que no queda ninguna condición ni target `net48` en los 12 proyectos de librerías y que los paquetes heredados eliminados ya no aparecen (`rg "Microsoft.CSharp|System.ComponentModel.Annotations|System.Threading.Tasks.Extensions|System.Net.Http 4.3.4"`)

## 7. Migración de los 8 proyectos de tests

- [ ] 7.1 Cambiar a `net10.0` los 5 proyectos de tests en `net8.0`: `KUtilitiesCoreTests`, `KUtilitiesCore.DataAccessTests`, `KUtilitiesCore.DataTests`, `KUtilitiesCore.MVVMTests.EventCommandBinder` y `KUtilitiesCore.LoggerTests`; si MSTest 4.4.1 / Test SDK 18.10.1 / Moq 4.20.72 generan advertencias de compatibilidad en net10, subir cada uno a la última versión compatible y registrar el cambio; verificar `dotnet build` de cada proyecto en 0 errores
- [ ] 7.2 Cambiar `KUtilitiesCore.Data.WinTests.csproj` a `net10.0-windows` y verificar `dotnet build KUtilitiesCore.Data.WinTests` en 0 errores (WinForms requerido)
- [ ] 7.3 Migrar `KUtilitiesCore.MVVMTests.csproj` de `net48` a `net10.0` (conservando MSTest y la cobertura existente) y verificar `dotnet build KUtilitiesCore.MVVMTests` en 0 errores
- [ ] 7.4 Migrar `KUtilitiesCore.GitHubUpdaterTests.csproj` de `net48` a `net10.0`; eliminar `System.Text.RegularExpressions 4.3.1` (API in-box en net10); conservar `DotNetEnv 3.2.0`; verificar `dotnet build KUtilitiesCore.GitHubUpdaterTests` en 0 errores

## 8. Revisar y actualizar pruebas unitarias existentes (OBLIGATORIO - Paso N)

- [ ] 8.1 Revisar los 8 proyectos de tests tras el salto de TFM: localizar usos de APIs no disponibles o con cambio de comportamiento en net10 (reflexión, `AppDomain`, serialización, `System.Text.RegularExpressions`) y adaptarlos conservando la intención y cobertura de cada prueba
- [ ] 8.2 Ejecutar las pruebas de cada proyecto migrado (`dotnet test KUtilitiesCore.<Proyecto>Tests`) y confirmar que cada suite compila y ejecuta sobre los targets nuevos sin fallos de infraestructura de pruebas
- [ ] 8.3 Validar específicamente `KUtilitiesCore.MVVMTests.EventCommandBinder` sobre el SDK 10 (históricamente frágil: ver commit `3230c99`) y documentar cualquier ajuste necesario en su configuración `UseMicrosoftTestingPlatform`

## 9. Ejecutar la suite completa de pruebas unitarias (OBLIGATORIO - Paso N+1, sin BD — EL AGENTE DEBE EJECUTARLAS)

- [ ] 9.1 Ejecutar la suite completa `dotnet test KUtilitiesCore.sln` y registrar el conteo total de pruebas, aprobadas/fallidas/omitidas y el tiempo de ejecución; comparar con la línea base de 1.3 y resolver toda divergencia antes de continuar
- [ ] 9.2 Ejecutar `dotnet build KUtilitiesCore.sln -c Release` y confirmar 0 errores y 0 warnings en Debug y Release para todos los TFMs (gate del repositorio)
- [ ] 9.3 Crear el reporte `openspec/changes/migrate-to-dotnet10/specs/migrate-to-dotnet10/reports/AAAA-MM-DD-paso-N+1-verificacion-de-pruebas-unitarias.md` con los comandos ejecutados, los resultados resumidos y las notas de pruebas inestables o excepciones
- [ ] 9.4 Marcar este paso como completado solo después de que las pruebas pasen y el reporte exista en disco

## 10. Actualizar la documentación técnica (OBLIGATORIO - Paso N+4)

- [ ] 10.1 Actualizar el README raíz: eliminar la promesa de soporte ".NET Framework 4.8 y .NET 8.0" y describir la compatibilidad como .NET 10 (más `netstandard2.0`/`netstandard2.1` conservados); documentar que el uso de DPAPI (`ProtectedData`) sigue requiriendo Windows
- [ ] 10.2 Actualizar `AGENTS.md`: tabla de proyectos con la matriz final de TFMs (`net10.0`, `net10.0-windows`, `netstandard2.0`, `netstandard2.1`), regla `global.json`, y notas de pruebas/dependencias aplicables a los targets nuevos; verificar que los comandos de build/test documentados siguen siendo exactos
- [ ] 10.3 Verificar que `openspec/config.yaml` es coherente con el estado final (ya declara net10.0/netstandard) y que los READMEs por proyecto afectados no mencionan targets eliminados (`rg "net48|net8.0" --glob "README.md"`)

## 11. Verificación final de integración

- [ ] 11.1 Verificar la matriz completa: `rg "net48|net8.0" --glob "*.csproj"` no retorna ningún `<TargetFramework(s)>` con targets eliminados y los 19 proyectos declaran los TFMs de la spec (ver `specs/target-frameworks/spec.md`, requisito "Matriz de frameworks objetivo")
- [ ] 11.2 Ejecutar la verificación integral final: `dotnet build KUtilitiesCore.sln` (0 warnings, 0 errores) seguido de `dotnet test KUtilitiesCore.sln` (8 suites en verde) y registrar ambos resultados como evidencia de cierre
