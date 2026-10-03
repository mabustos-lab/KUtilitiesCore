# Spec Delta

## Purpose

Define el contrato del pipeline de importación de fuente única: mapeo de la tabla provista sin clonar (con propiedad del llamador), pintado de errores sobre esa tabla, semántica coherente con datos vacíos y generación de una tabla resultado tipada sin columnas de control.

## ADDED Requirements

### Requirement: Mapeo sin clonación con propiedad del llamador
El pipeline de fuente única DEBE (MUST) mapear y validar usando directamente la tabla provista por el llamador, sin crear copias internas de la misma. La propiedad de la tabla provista permanece en el llamador y el pipeline NO DEBE (MUST NOT) disponerla.

#### Scenario: Errores de validación pintados en la tabla provista
- **WHEN** se valida una tabla provista que contiene un valor no convertible para una columna tipada
- **THEN** la fila correspondiente de la tabla provista queda marcada con error de columna (`HasErrors = true`) y las filas válidas no

#### Scenario: Valor por defecto aplicado sobre la tabla provista
- **WHEN** una definición con valor por defecto recibe una celda vacía en la tabla provista
- **THEN** la celda de la tabla provista queda con el valor por defecto aplicado tras la validación

#### Scenario: Dispose del gestor no libera la tabla provista
- **WHEN** el gestor de importación se dispone después de mapear una tabla provista
- **THEN** la tabla provista NO queda dispuesta y el llamador puede seguir usándola

#### Scenario: Argumento nulo
- **WHEN** se invoca el mapeo con una tabla nula
- **THEN** el pipeline rechaza la operación con `ArgumentNullException`

### Requirement: Semántica coherente con datos vacíos
La validación de tipos DEBE (SHALL) ser coherente cuando la tabla mapeada no contiene filas: el retorno del método y el estado de validez del resultado DEBEN (MUST) indicar lo mismo.

#### Scenario: Tabla sin filas
- **WHEN** se valida una tabla mapeada que tiene esquema pero cero filas
- **THEN** la validación retorna `false` y el resultado de validación queda inválido (`IsValid = false`)

### Requirement: Tabla resultado tipada sin columnas de control
El pipeline DEBE (SHALL) exponer la generación de una tabla resultado nueva, propiedad del llamador, con una columna tipada por definición (`TargetType`, nombrada por `FieldName`), SIN columnas de control internas (índice de fila / marcador de validez).

#### Scenario: Columnas tipadas sin columnas de control
- **WHEN** se genera la tabla resultado con una definición de tipo entero
- **THEN** la columna correspondiente es de tipo entero, no existen columnas de control, y los valores convertibles quedan tipados

#### Scenario: Valor vacío en campo obligatorio con valor por defecto
- **WHEN** una celda vacía pertenece a una definición que no admite nulos y define valor por defecto
- **THEN** la tabla resultado contiene el valor por defecto en esa celda

#### Scenario: Valor vacío sin valor por defecto
- **WHEN** una celda vacía pertenece a una definición sin valor por defecto
- **THEN** la tabla resultado contiene `DBNull` en esa celda

#### Scenario: La tabla resultado sobrevive al gestor
- **WHEN** el gestor de importación se dispone después de generar la tabla resultado
- **THEN** la tabla resultado NO queda dispuesta y el llamador puede seguir usándola
