# Spec Delta

## MODIFIED Requirements

### Requirement: Compilación sin warnings en proyectos de librería

Todos los proyectos de librería de la solución DEBEN (MUST) compilar sin emit warnings; los proyectos de test quedan explícitamente fuera de este requisito. La verificación DEBE (MUST) hacerse con un build completo de la solución, no incremental, para evitar falsos negativos por caché de compilación.

#### Scenario: Build completo de la solución

- **WHEN** se ejecuta `dotnet build KUtilitiesCore.sln --no-incremental`
- **THEN** la compilación finaliza con 0 errores y 0 warnings atribuibles a los 14 proyectos de librería (incluidos los nuevos `KUtilitiesCore.Data.Wizard` y `KUtilitiesCore.Data.WPF`)
- **AND** los warnings preexistentes de los proyectos de test permanecen sin cambios (fuera de alcance) y el nuevo `KUtilitiesCore.Data.WPFTests` se incorpora partiendo de cero warnings

## ADDED Requirements

### Requirement: Nulabilidad habilitada en los proyectos nuevos
Los proyectos de librería `KUtilitiesCore.Data.Wizard` y `KUtilitiesCore.Data.WPF` DEBEN (SHALL) declarar `<Nullable>enable</Nullable>` desde su creación, igualando al resto de librerías.

#### Scenario: Configuración de Nullable en los csproj nuevos
- **WHEN** se inspeccionan los `.csproj` de `KUtilitiesCore.Data.Wizard` y `KUtilitiesCore.Data.WPF`
- **THEN** ambos declaran `<Nullable>enable</Nullable>`
