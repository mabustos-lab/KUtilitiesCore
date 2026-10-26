# Proposal — Salto del ecosistema KUtilitiesCore a .NET 10

## Why

Las 12 librerías y 8 proyectos de tests de la solución están anclados a runtimes heredados (`net48`, `net8.0`, `net8.0-windows`), mientras que .NET 8 (LTS) llega a su fin de soporte el **10-nov-2026**. Los consumidores de los paquetes NuGet quedarían expuestos a un runtime sin parches de seguridad y el repositorio quedaría desalineado con el runtime LTS vigente (SDK 10 ya instalado en el entorno de desarrollo: 10.0.401).

## What Changes

- **BREAKING** — Eliminación total de `net48` como target en toda la solución: los consumidores sobre .NET Framework 4.8 dejan de ser soportados (implica versión mayor `2.0.0` en la futura publicación NuGet, fuera del alcance de este cambio).
- **BREAKING** — Sustitución de `net8.0` → `net10.0` y `net8.0-windows` → `net10.0-windows` en todos los proyectos.
- **BREAKING** — Migración de los tests heredados `MVVMTests` y `GitHubUpdaterTests` de `net48` a `net10.0`.
- Conservación sin cambios de `KUtilitiesCore.DataAccess` (`netstandard2.1`) y `KUtilitiesCore.Logger` (`netstandard2.0`): ambos son consumibles desde .NET 10.
- Limpieza técnica habilitada por el salto:
  - Eliminación de `PackageReference` condicionales a `net48`: `System.Threading.Tasks.Extensions` (MVVM), `System.Net.Http 4.3.4` (GitHubUpdater).
  - Eliminación de paquetes redundantes en net10: `Microsoft.CSharp 4.7.0` (Core), `System.ComponentModel.Annotations 5.*` (Core, MVVM), `System.Text.RegularExpressions 4.3.1` (GitHubUpdaterTests).
  - Eliminación del `HintPath` muerto de `DataAccess.Http` (referencia a ensamblado de .NET Framework 4.8 en un proyecto `net8.0`-only).
  - Eliminación de los `PropertyGroup` con `WarningLevel 4` condicionados a `net48` (Core).
- Actualización de dependencias a major alineada con net10: `Microsoft.EntityFrameworkCore 9.0.14 → 10.x`, `Microsoft.AspNetCore.WebUtilities 8.0.25 → 10.x`, `MSTest`/`Microsoft.NET.Test.Sdk` a la última versión compatible. El resto (`Microsoft.Data.SqlClient 7.1`, `Moq 4.20.72`, `ClosedXML 0.105.1`, `DotNetEnv 3.2.0`, `Microsoft.Extensions.* 10.0.12`) se conserva si compila y pasa tests en net10.
- Creación de `global.json` fijando el SDK 10 con `rollForward: latestFeature`.
- Actualización de documentación: README raíz (elimina la promesa ".NET Framework 4.8 y .NET 8.0") y `AGENTS.md` (matriz final de TFMs).

## Capabilities

### New Capabilities

- `target-frameworks`: Contrato de frameworks objetivo de la solución — define la matriz de TFMs soportados por cada proyecto (`net10.0` / `net10.0-windows` / `netstandard2.0` / `netstandard2.1`), la eliminación de `net48`/`net8.0`, el gate de build limpio (0 errores y 0 advertencias nuevas respecto de la línea base; la deuda preexistente de warnings queda fuera de alcance) y la validación de la suite de tests sobre los targets nuevos.

### Modified Capabilities

(ninguno — no existen especificaciones previas en `openspec/specs/`)

## Impact

- **Código**: los 19 `.csproj` de la solución (12 librerías + 8 proyectos de tests, contando `Data.WinTests` como proyecto Windows). Sin cambios de código fuente previstos salvo remediación de warnings nuevos del SDK 10 y adaptaciones puntuales que exija el salto net48 → net10 en `MVVMTests` y `GitHubUpdaterTests`.
- **APIs públicas**: sin cambios de superficie pública; el impacto es de compatibilidad binaria (consumidores net48 pierden soporte).
- **Dependencias**: EF Core (major 9→10), MSTest/Test SDK (minor/patch), eliminación de paquetes heredados (ver limpieza técnica). `System.Security.Cryptography.ProtectedData` sigue siendo Windows-only en su uso (DPAPI).
- **Sistemas**: no hay CI activa (`.github/` no existe); la verificación es local con `dotnet build`/`dotnet test`.
- **Documentación**: README raíz, `AGENTS.md`, y `openspec/config.yaml` (ya refleja el estado destino: net10.0, netstandard2.0, netstandard2.1).
