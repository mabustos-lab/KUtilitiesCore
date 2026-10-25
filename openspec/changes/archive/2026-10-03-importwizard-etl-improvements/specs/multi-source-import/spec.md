# Spec Delta

## MODIFIED Requirements

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
