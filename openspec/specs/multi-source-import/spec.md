# Multi-Source-Import Specification

## Purpose

Permite importar en una sola operación transaccional datos de múltiples fuentes que comparten un mismo esquema, entregando un resultado trazable por origen (un `DataSet` con un `DataTable` por fuente).

## Requirements

### Requirement: Importación multi-fuente con esquema común
El sistema DEBE (MUST) permitir importar en una sola operación datos de múltiples fuentes que comparten la misma definición de columnas (mismo esquema).

#### Scenario: Libro Excel con varias hojas del mismo esquema
- **WHEN** se importa un libro Excel con N hojas que comparten el mismo esquema
- **THEN** todas las hojas seleccionadas se procesan en una sola operación y el resultado contiene un `DataTable` por hoja

#### Scenario: Fuentes mixtas del mismo esquema
- **WHEN** se importan fuentes de distinto tipo (p. ej. CSV y Excel) que comparten el mismo esquema
- **THEN** todas las fuentes se procesan en la misma operación con la misma definición de columnas

#### Scenario: Selección sin fuentes
- **WHEN** se invoca la operación multi-fuente sin ninguna fuente configurada
- **THEN** el sistema rechaza la operación con un error de precondición

### Requirement: Resultado trazable por origen
El resultado de la operación DEBE (SHALL) exponer un `DataSet` con un `DataTable` por fuente, donde cada tabla queda identificada con el nombre de su origen.

#### Scenario: Trazabilidad del nombre de origen
- **WHEN** la operación finaliza con éxito
- **THEN** cada `DataTable` del `DataSet` tiene en `TableName` el nombre de su hoja o archivo de origen

#### Scenario: Columnas en distinto orden entre fuentes
- **WHEN** dos fuentes del mismo esquema presentan sus columnas en distinto orden
- **THEN** ambas fuentes se mapean correctamente gracias al emparejamiento de columnas insensible a mayúsculas

### Requirement: Semántica transaccional todo-o-nada
La operación multi-fuente DEBE (MUST) ser transaccional: si cualquier fuente falla al leerse o produce errores de validación, la operación completa falla y no se expone ningún resultado parcial, dejando el estado como antes de la operación.

#### Scenario: Fallo de lectura en una de varias fuentes
- **WHEN** una de las fuentes falla al leerse (archivo no encontrado, error de E/S o acceso denegado)
- **THEN** la operación falla integralmente con una excepción que identifica la fuente fallida y la causa, y no se expone ningún resultado parcial

#### Scenario: Errores de validación en una fuente
- **WHEN** una fuente produce errores de validación de tipos o de reglas de negocio
- **THEN** la operación falla integralmente y los errores reportados identifican su fuente y conservan el índice de fila relativo a esa fuente

#### Scenario: Estado sin cambios tras un fallo
- **WHEN** la operación falla por cualquier causa
- **THEN** el estado del gestor queda exactamente como antes de la operación

### Requirement: Variante asíncrona cancelable
La operación multi-fuente DEBE (SHALL) exponer una variante asíncrona que acepte un token de cancelación.

#### Scenario: Cancelación durante la operación
- **WHEN** se cancela la operación mientras se procesan las fuentes
- **THEN** la operación se aborta sin exponer resultados parciales

### Requirement: Liberación de recursos
El gestor multi-fuente DEBE (SHALL) liberar todos los `DataTable`/`DataSet` internos y de resultado al ser dispuesto.

#### Scenario: Disposición del gestor
- **WHEN** el gestor multi-fuente se dispone después de una operación
- **THEN** todos los recursos de las fuentes procesadas quedan liberados

### Requirement: Compatibilidad con la importación de fuente única
La importación multi-fuente DEBE (MUST) seguir siendo compatible con el contrato actualizado del pipeline de fuente única (`ImportManager`, `ImportWizardForm`), incluida la semántica de propiedad de la tabla provista: el pipeline de fuente única NO DEBE (MUST NOT) disponer las tablas crudas que recibe, y el gestor multi-fuente conserva la propiedad de las tablas de cada fuente.

#### Scenario: Regresión de importación existente
- **WHEN** se ejecutan las suites de tests existentes de importación (`ImportManagerTests`, `ImportWizardFormTests`)
- **THEN** todas pasan, adaptando únicamente aquellas cuyo contrato cambia de forma intencional en el cambio `importwizard-etl-improvements` (propiedad de la tabla provista y semántica de datos vacíos)

#### Scenario: Regresión de importación multi-fuente
- **WHEN** se ejecuta la suite de tests de importación multi-fuente (`MultiSourceImportManagerTests`)
- **THEN** todas las pruebas pasan sin modificación de su código ni de su comportamiento

#### Scenario: Propiedad de las tablas crudas por fuente
- **WHEN** el gestor multi-fuente procesa una fuente con su tabla cruda
- **THEN** la tabla cruda no es dispuesta por el pipeline de fuente única y el gestor multi-fuente conserva su propiedad hasta finalizar la operación
