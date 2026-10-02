# Proposal

## Why

Tras la migración a .NET 10, la solución compila sin errores pero emite **482 warnings** (454 en las 12 librerías, 28 en proyectos de test, verificado con `dotnet build KUtilitiesCore.sln --no-incremental`). Esta deuda técnica tiene tres frentes: flujos de nulabilidad sin corregir (riesgo real de `NullReferenceException` en runtime), documentación XML ausente o malformada (todos los `.csproj` tienen `GenerateDocumentationFile=True`, pero ~267 miembros públicos carecen de docs y ~36 están rotas), y una API obsoleta en uso (SYSLIB0051). En este repositorio el build limpio es la verificación estándar (AGENTS.md: "Build warnings = the verification gate"), por lo que alcanzar 0 warnings en las librerías es condición para mantener la calidad de la base de código.

## What Changes

- **Habilitar `<Nullable>enable</Nullable>` en `KUtilitiesCore.Dal` y `KUtilitiesCore.Data`** (decisión del PO; actualmente `Nullable disable`). Nota: esto revelará CS86xx adicionales a los 454 contados hoy — el objetivo "0 warnings" se mide tras habilitarlo. `KUtilitiesCore.DataAccess` y `KUtilitiesCore.DataAccess.Http` mantienen `Nullable disable` (sus warnings son solo de documentación / cero).
- Corregir los warnings de nulabilidad (CS8600/02/03/04/18/25, CS8613, CS8766/67 — ~172 visibles + emergentes de Dal/Data) con fixes reales de flujo y anotaciones; el uso de `!` o `= null!` exige justificación en el commit.
- Redactar la documentación XML faltante (CS1591, ~267) **en español**, explicando propósito y "por qué" (no el "qué"), con `<example>` cuando el patrón de uso no sea obvio — según convención del repositorio.
- Corregir la documentación XML malformada (CS1570/1572/1573/1574, CS1711, CS1587 — ~36): etiquetas desalineadas, `cref` no resueltos, `param`/`typeparam` huérfanos.
- Eliminar el constructor de serialización obsoleto en `KUtilitiesCore.Data` (SYSLIB0051, `EmptyDataSourceException.cs`).
- **Fuera de alcance** (decisión del PO): los 28 warnings de los 6 proyectos de test (incluidos los 4 MSTEST0056), `KUtilitiesCore.DataAccess.Http` (0 warnings), y la activación de `TreatWarningsAsErrors` (la meta se verifica con un build limpio, sin blindaje posterior).

## Capabilities

### New Capabilities

- `build-hygiene`: Higiene de compilación de las librerías — requisitos que garantizan que los 12 proyectos de librería compilen con 0 warnings: nulabilidad habilitada y correcta, documentación XML completa en español y ausencia de APIs obsoletas.

### Modified Capabilities

(ninguna — no existen specs previas; `openspec/specs/` está vacío)

## Impact

- **Código afectado**: los 12 proyectos de librería (`KUtilitiesCore`, `Encryption`, `MVVM`, `MVVM.Messaging`, `Data`, `Data.Win`, `DataAccess`, `Dal`, `DataAccess.EfCore`, `DataAccess.Http`*, `Logger`, `GitHubUpdater`; *Http sin cambios por no tener warnings). Con mayor concentración: `Dal` (~175) y `Core` (~142).
- **API pública**: las anotaciones de nulabilidad (`T?`, `[NotNullWhen]`, etc.) son parte del contrato visible por los consumidores en tiempo de compilación; no se esperan cambios de comportamiento en runtime.
- **Tests**: `dotnet test --solution KUtilitiesCore.sln` debe seguir al 100% (excluyendo el fallo 401 preexistente de `GitHubUpdateServiceTest` sin `GITHUB_TOKEN`). La suite de tests es la red de seguridad contra regresiones de nulabilidad en `Dal` (usa mocks de `DbDataReader`, sin SQL Server real).
- **Convenciones**: este cambio revoca puntualmente la convención "match the existing Nullable setting" para `Dal` y `Data`; el resto de proyectos no cambia de setting.
- **No se tocan**: proyectos de test, paquetes NuGet, estructura de solución, ni configuración CI.
