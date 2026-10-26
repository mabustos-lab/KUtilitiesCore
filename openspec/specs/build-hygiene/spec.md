# Build-Hygiene Specification

## Purpose

Garantiza la higiene de compilación de las 12 librerías de KUtilitiesCore: build sin warnings, tipos de referencia que aceptan valores NULL habilitados en todos los proyectos de librería, documentación XML pública completa en español y sin APIs obsoletas.

## Requirements

### Requirement: Compilación sin warnings en proyectos de librería

Todos los proyectos de librería de la solución DEBEN (MUST) compilar sin emit warnings; los proyectos de test quedan explícitamente fuera de este requisito. La verificación DEBE (MUST) hacerse con un build completo de la solución, no incremental, para evitar falsos negativos por caché de compilación.

#### Scenario: Build completo de la solución

- **WHEN** se ejecuta `dotnet build KUtilitiesCore.sln --no-incremental`
- **THEN** la compilación finaliza con 0 errores y 0 warnings atribuibles a los 12 proyectos de librería
- **AND** los 28 warnings preexistentes de los 6 proyectos de test permanecen sin cambios (fuera de alcance)

### Requirement: Nulabilidad habilitada en todos los proyectos de librería

Los proyectos de librería `KUtilitiesCore.Dal` y `KUtilitiesCore.Data` DEBEN (SHALL) declarar `<Nullable>enable</Nullable>`, igualando al resto de librerías. Las correcciones de nulabilidad (CS86xx/CS87xx) DEBEN (MUST) resolver el flujo real de valores NULL del código; suprimir el warning con el operador `!` o inicializadores `= null!` sin corrección subyacente NO se considera válido salvo justificación documentada.

#### Scenario: Configuración de Nullable en los csproj

- **WHEN** se inspeccionan los `.csproj` de `KUtilitiesCore.Dal` y `KUtilitiesCore.Data`
- **THEN** ambos declaran `<Nullable>enable</Nullable>`

#### Scenario: Warnings de nulabilidad eliminados

- **WHEN** se compila la solución tras habilitar Nullable y aplicar las correcciones
- **THEN** no se emite ningún warning de la familia CS86xx/CS87xx en los proyectos de librería, incluidos los que emerjan al habilitar Nullable en `Dal` y `Data`

#### Scenario: Comportamiento en runtime preservado

- **WHEN** se ejecuta la suite de tests tras las correcciones de nulabilidad
- **THEN** el resultado es idéntico al previo al cambio (sin nuevos fallos ni excepciones en tiempo de ejecución)

### Requirement: Documentación XML pública completa y válida

Todo miembro público de los proyectos de librería DEBE (MUST) estar documentado con comentarios XML en español, redactados para explicar el propósito y la razón de uso (no solo el "qué"), e incluyendo `<example>` cuando el patrón de uso no sea obvio. Los comentarios XML existentes DEBEN (MUST) ser sintácticamente válidos: sin etiquetas desalineadas, sin `cref` no resolubles y sin etiquetas `param`/`typeparam` que no correspondan a parámetros reales. Los `.csproj` DEBEN (MUST) conservar `GenerateDocumentationFile=True`.

#### Scenario: Miembros públicos documentados

- **WHEN** se compila la solución
- **THEN** no se emite ningún warning CS1591 (documentación XML faltante) en los proyectos de librería

#### Scenario: Documentación existente válida

- **WHEN** se compila la solución
- **THEN** no se emite ningún warning CS1570, CS1572, CS1573, CS1574, CS1711 ni CS1587 en los proyectos de librería

### Requirement: Ausencia de APIs obsoletas

Los proyectos de librería NO DEBEN (MUST NOT) usar APIs marcadas como obsoletas por la plataforma que generen warnings de compilación (p. ej. SYSLIB0051: constructor de serialización binaria de `Exception`).

#### Scenario: Compilación sin SYSLIB0051

- **WHEN** se compila la solución
- **THEN** no se emite ningún warning SYSLIB0051 ni de otras APIs obsoletas en los proyectos de librería

### Requirement: Sin supresiones globales de warnings

La meta de 0 warnings NO PODRÁ (MUST NOT) alcanzarse mediante `NoWarn` global, supresiones `#pragma` injustificadas ni eliminando `GenerateDocumentationFile`. Cualquier supresión puntual DEBE (MUST) quedar justificada en el commit que la introduce.

#### Scenario: Verificación de supresiones

- **WHEN** se inspeccionan los `.csproj` y el código fuente de las librerías tras el cambio
- **THEN** no aparece ningún `NoWarn` nuevo que silencie las familias CS1591/CS86xx/CS87xx, y toda directiva `#pragma warning disable` existente cuenta con justificación documentada en su commit
