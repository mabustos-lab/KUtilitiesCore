# Spec Delta

## Purpose

Define la preparación compartida del diálogo de exportación de datos: validación de la fuente, nombre de archivo por defecto, filtro de archivos soportados y resolución del formato por extensión, con fachadas delgadas por plataforma (WinForms y WPF) que aportan únicamente su diálogo nativo de guardado.

## ADDED Requirements

### Requirement: Rechazo de fuente vacía sin abrir el diálogo
La preparación DEBE (MUST) rechazar la exportación cuando la tabla es nula o no contiene filas, mostrando un aviso al usuario y retornando fracaso, sin abrir el diálogo de guardado.

#### Scenario: Tabla sin filas
- **WHEN** se solicita exportar una tabla nula o con cero filas
- **THEN** se muestra un aviso al usuario y no se abre el diálogo de guardado

### Requirement: Nombre de archivo por defecto
La preparación DEBE (SHALL) generar un nombre de archivo por defecto con marca temporal cuando el consumidor no provee uno.

#### Scenario: Sin nombre provisto
- **WHEN** se prepara la exportación sin nombre de archivo
- **THEN** el nombre propuesto sigue el patrón por defecto con la fecha y hora de la solicitud

### Requirement: Filtro y resolución de formato compartidos
La preparación DEBE (SHALL) construir el filtro de archivos soportados (CSV, TSV, PSV, XLSX, XLS) y resolver el formato de exportación a partir de la extensión del archivo seleccionado por el usuario.

#### Scenario: Extensión Excel seleccionada
- **WHEN** el usuario guarda con extensión `.xlsx` o `.xls`
- **THEN** la exportación se delega en el exportador de Excel

#### Scenario: Extensión de texto seleccionada
- **WHEN** el usuario guarda con extensión `.csv`, `.tsv` o `.psv`
- **THEN** la exportación se delega en el exportador de texto con el separador correspondiente

#### Scenario: Extensión no soportada
- **WHEN** la extensión del archivo seleccionado no corresponde a un formato soportado
- **THEN** la preparación lo indica al usuario y no se ejecuta ninguna exportación

### Requirement: Fachadas delgadas por plataforma
Las fachadas de exportación de WinForms y WPF DEBEN (SHALL) delegar la validación, el nombre por defecto, el filtro y la resolución de formato en la preparación compartida, aportando únicamente el diálogo nativo de su plataforma.

#### Scenario: Paridad de preparación entre plataformas
- **WHEN** ambas fachadas preparan la exportación con las mismas entradas
- **THEN** ambas proponen el mismo nombre por defecto, el mismo filtro y la misma resolución de formato

#### Scenario: Diálogo nativo por plataforma
- **WHEN** se exporta desde una aplicación WinForms o desde una aplicación WPF
- **THEN** cada fachada utiliza el diálogo de guardado nativo de su plataforma
