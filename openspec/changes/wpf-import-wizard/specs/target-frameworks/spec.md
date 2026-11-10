# Spec Delta

## MODIFIED Requirements

### Requirement: Matriz de frameworks objetivo de la solución
Todos los proyectos de la solución DEBERÁN (SHALL) declarar exclusivamente los siguientes TargetFrameworks: `net10.0` para las librerías y tests multiplataforma (incluido `KUtilitiesCore.Data.Wizard`), `net10.0-windows` para los proyectos Windows (`KUtilitiesCore.Data.Win`, `KUtilitiesCore.Data.WinTests`, `KUtilitiesCore.Data.WPF`, `KUtilitiesCore.Data.WPFTests`), `netstandard2.1` para `KUtilitiesCore.DataAccess` y `netstandard2.0` para `KUtilitiesCore.Logger`. Ningún proyecto declarará `net48`, `net8.0` ni `net8.0-windows`.

#### Scenario: Inspección de la matriz de TFMs
- **WHEN** se inspeccionan los 22 archivos `.csproj` de la solución
- **THEN** cada proyecto declara exactamente el TFM asignado en la matriz y ningún archivo contiene `net48`, `net8.0` ni `net8.0-windows` en `<TargetFramework>`/`<TargetFrameworks>`

#### Scenario: Proyectos netstandard conservan su TFM
- **WHEN** se inspeccionan los proyectos de abstracciones y logging
- **THEN** `KUtilitiesCore.DataAccess` declara `netstandard2.1` y `KUtilitiesCore.Logger` declara `netstandard2.0`, sin adición de otros targets

### Requirement: Suite de pruebas en verde sobre los targets nuevos
Los 9 proyectos de pruebas DEBERÁN (SHALL) ejecutarse y pasar en su totalidad sobre los targets migrados: los 6 proyectos `net8.0` pasan a `net10.0`, `Data.WinTests` y `Data.WPFTests` a `net10.0-windows`, y los 2 proyectos heredados `net48` (`KUtilitiesCore.MVVMTests`, `KUtilitiesCore.GitHubUpdaterTests`) pasan a `net10.0` manteniendo su framework de pruebas y su cobertura de casos.

#### Scenario: Ejecución completa de la suite
- **WHEN** se ejecuta `dotnet test KUtilitiesCore.sln` sobre los targets migrados
- **THEN** las 9 suites de pruebas (KUtilitiesCoreTests, DataAccessTests, DataTests, MVVMTests, MVVMTests.EventCommandBinder, GitHubUpdaterTests, Data.WinTests, Data.WPFTests, LoggerTests) se ejecutan con 0 fallos

#### Scenario: Tests heredados net48 migrados
- **WHEN** se ejecutan `KUtilitiesCore.MVVMTests` y `KUtilitiesCore.GitHubUpdaterTests` tras el salto
- **THEN** ambos proyectos compilan y ejecutan sobre `net10.0` con el mismo conjunto de pruebas y resultado equivalente al estado previo
