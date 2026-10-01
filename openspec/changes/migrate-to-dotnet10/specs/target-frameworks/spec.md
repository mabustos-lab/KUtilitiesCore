# Spec Delta — target-frameworks

## Purpose

Define el contrato de frameworks objetivo (TFM) de la solución KUtilitiesCore: qué runtimes soporta cada proyecto tras el salto a .NET 10, la eliminación del soporte a .NET Framework 4.8 y .NET 8, y las condiciones verificables de construcción y pruebas sobre los targets nuevos.

## ADDED Requirements

### Requirement: Matriz de frameworks objetivo de la solución
Todos los proyectos de la solución DEBERÁN (SHALL) declarar exclusivamente los siguientes TargetFrameworks: `net10.0` para las librerías y tests multiplataforma, `net10.0-windows` para los proyectos Windows (`KUtilitiesCore.Data.Win`, `KUtilitiesCore.Data.WinTests`), `netstandard2.1` para `KUtilitiesCore.DataAccess` y `netstandard2.0` para `KUtilitiesCore.Logger`. Ningún proyecto declarará `net48`, `net8.0` ni `net8.0-windows`.

#### Scenario: Inspección de la matriz de TFMs
- **WHEN** se inspeccionan los 19 archivos `.csproj` de la solución
- **THEN** cada proyecto declara exactamente el TFM asignado en la matriz y ningún archivo contiene `net48`, `net8.0` ni `net8.0-windows` en `<TargetFramework>`/`<TargetFrameworks>`

#### Scenario: Proyectos netstandard conservan su TFM
- **WHEN** se inspeccionan los proyectos de abstracciones y logging
- **THEN** `KUtilitiesCore.DataAccess` declara `netstandard2.1` y `KUtilitiesCore.Logger` declara `netstandard2.0`, sin adición de otros targets

### Requirement: Eliminación del soporte a .NET Framework 4.8
Ningún proyecto de la solución DEBERÁ (MUST) declarar `net48` como target, ni mantener referencias, condiciones de compilación o paquetes exclusivos de .NET Framework 4.8 tras el salto.
**Reason**: El ecosistema realiza un salto completo a .NET 10; mantener `net48` condiciona las versiones de dependencias y las features de lenguaje utilizables.
**Migration**: Los consumidores sobre .NET Framework 4.8 DEBERÁN permanecer en la última versión publicada de los paquetes con soporte net48 (o migrar a .NET 10). La futura publicación NuGet de este salto DEBERÁ usar una versión mayor (`2.0.0`).

#### Scenario: Consumidor .NET Framework 4.8
- **WHEN** un proyecto que targeting `net48` intenta consumir los paquetes generados tras el salto
- **THEN** la restauración falla porque los artefactos ya no exponen activos compatibles con `net48`

#### Scenario: Referencias condicionales a net48 eliminadas
- **WHEN** se inspeccionan los `.csproj` buscando `PackageReference`, `PropertyGroup` o `Reference` condicionados a `'$(TargetFramework)' == 'net48'`
- **THEN** no existe ninguna condición de compilación ni paquete exclusivo de `net48` en la solución

### Requirement: Construcción limpia bajo el SDK de .NET 10
La solución completa DEBERÁ (SHALL) compilar con el SDK de .NET 10 produciendo cero advertencias y cero errores en todos los targets, manteniendo el gate de verificación del repositorio ("build warnings = verificación").

#### Scenario: Build de la solución sin warnings
- **WHEN** se ejecuta `dotnet build KUtilitiesCore.sln` con el SDK 10 en configuración Debug y Release
- **THEN** la compilación de todos los proyectos y targets finaliza con 0 errores y 0 advertencias, incluyendo las advertencias introducidas por los analizadores nuevos del SDK 10 remediadas en el código

### Requirement: Suite de pruebas en verde sobre los targets nuevos
Los 8 proyectos de pruebas DEBERÁN (SHALL) ejecutarse y pasar en su totalidad sobre los targets migrados: los 6 proyectos `net8.0` pasan a `net10.0`, `Data.WinTests` a `net10.0-windows`, y los 2 proyectos heredados `net48` (`KUtilitiesCore.MVVMTests`, `KUtilitiesCore.GitHubUpdaterTests`) pasan a `net10.0` manteniendo su framework de pruebas y su cobertura de casos.

#### Scenario: Ejecución completa de la suite
- **WHEN** se ejecuta `dotnet test KUtilitiesCore.sln` sobre los targets migrados
- **THEN** las 8 suites de pruebas (KUtilitiesCoreTests, DataAccessTests, DataTests, MVVMTests, MVVMTests.EventCommandBinder, GitHubUpdaterTests, Data.WinTests, LoggerTests) se ejecutan con 0 fallos

#### Scenario: Tests heredados net48 migrados
- **WHEN** se ejecutan `KUtilitiesCore.MVVMTests` y `KUtilitiesCore.GitHubUpdaterTests` tras el salto
- **THEN** ambos proyectos compilan y ejecutan sobre `net10.0` con el mismo conjunto de pruebas y resultado equivalente al estado previo

### Requirement: SDK fijado mediante global.json
La solución DEBERÁ (SHALL) incluir un `global.json` en la raíz que fije la versión mayor 10 del SDK con una política `rollForward` que permita resolver parches/características superiores de la misma banda de versión.

#### Scenario: Resolución del SDK en la raíz del repositorio
- **WHEN** se ejecuta cualquier comando `dotnet` desde la raíz del repositorio
- **THEN** el SDK resuelto pertenece a la banda `10.x` conforme al `global.json` y no a los SDK 8.x instalados en la máquina

### Requirement: Dependencias compatibles con .NET 10
Todas las referencias de paquetes de la solución DEBERÁN (SHALL) restaurar y compilar sobre los targets nuevos. Las dependencias alineadas con el salto son: `Microsoft.EntityFrameworkCore` major 10 (en `DataAccess.EfCore`) y `Microsoft.AspNetCore.WebUtilities` major 10 (en `DataAccess.Http`); las demás se conservan en su versión actual si compilan y pasan pruebas sobre net10. Los paquetes redundantes en .NET moderno (`Microsoft.CSharp`, `System.ComponentModel.Annotations`, `System.Text.RegularExpressions`, `System.Net.Http`, `System.Threading.Tasks.Extensions` heredados) DEBERÁN (SHALL) eliminarse cuando la API que aportan forme parte del runtime net10.

#### Scenario: Restauración de paquetes sobre net10
- **WHEN** se ejecuta `dotnet restore KUtilitiesCore.sln` con el SDK 10
- **THEN** todas las referencias se restauran sin errores ni advertencias de incompatibilidad con los targets net10

#### Scenario: Paquetes heredados eliminados
- **WHEN** se inspeccionan los `.csproj` tras el salto
- **THEN** no existen referencias a `Microsoft.CSharp 4.7.0`, `System.ComponentModel.Annotations`, `System.Threading.Tasks.Extensions` (condicional net48), `System.Net.Http 4.3.4` (condicional net48) ni `System.Text.RegularExpressions 4.3.1`

### Requirement: Documentación alineada con la matriz final
La documentación pública del repositorio (README raíz y `AGENTS.md`) DEBERÁ (SHALL) reflejar la matriz final de TFMs, eliminando cualquier promesa de soporte para .NET Framework 4.8 y .NET 8.

#### Scenario: README raíz actualizado
- **WHEN** se lee el README raíz tras el salto
- **THEN** describe la compatibilidad como .NET 10 (más los netstandard conservados) y ya no menciona soporte para ".NET Framework 4.8" ni ".NET 8.0" como targets vigentes

#### Scenario: AGENTS.md actualizado
- **WHEN** se lee `AGENTS.md` tras el salto
- **THEN** la tabla de proyectos y las notas de arquitectura reflejan los TFMs `net10.0`/`net10.0-windows` y las reglas de nullable y pruebas aplicables a los targets nuevos
