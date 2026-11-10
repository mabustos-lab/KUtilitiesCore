# Spec Delta

## MODIFIED Requirements

### Requirement: Resultado de importación tipado y propio del consumidor
El resultado expuesto por el asistente DEBE (SHALL) ser una tabla nueva tipada según las definiciones, sin columnas de control, propiedad del consumidor, y utilizable después de que la vista del asistente (formulario WinForms o ventana WPF) se cierre y disponga.

#### Scenario: Resultado utilizable tras cerrar el formulario
- **WHEN** el consumidor cierra la vista del asistente (formulario WinForms o ventana WPF, incluyendo su disposición) después de una importación exitosa
- **THEN** el resultado expuesto sigue siendo una tabla válida, tipada y sin columnas de control

## ADDED Requirements

### Requirement: Paridad de front-ends sobre la capa compartida
Los asistentes de WinForms y WPF DEBEN (MUST) cumplir todos los requisitos de esta especificación consumiendo la misma capa de orquestación compartida, de modo que el comportamiento observable sea idéntico en ambas plataformas.

#### Scenario: Flujo equivalente en ambos front-ends
- **WHEN** se ejecuta el mismo flujo de carga, mapeo, validación e importación en ambos asistentes
- **THEN** los estados intermedios y el resultado observable son equivalentes en ambas plataformas

#### Scenario: El asistente WinForms conserva su comportamiento
- **WHEN** se ejecuta la suite de pruebas del asistente WinForms tras delegar la orquestación en la capa compartida
- **THEN** todas las pruebas existentes pasan sin modificación de comportamiento observable

### Requirement: Front-end WPF sin dependencia de Windows Forms
El paquete del asistente WPF DEBE (SHALL) implementarse sobre WPF (XAML) y NO DEBE (MUST NOT) depender de Windows Forms, ni directamente ni por referencia al paquete del asistente WinForms.

#### Scenario: Inspección del proyecto WPF
- **WHEN** se inspecciona el proyecto del asistente WPF
- **THEN** declara `UseWPF`, no declara `UseWindowsForms` y no referencia proyectos WinForms

### Requirement: Señalización de errores por celda en la vista previa WPF
La vista previa del asistente WPF DEBE (SHALL) señalizar los errores de validación por celda usando mecanismos nativos de WPF (validación y estilos), y la fila afectada DEBE (MUST) quedar identificada como con error para el filtro de filas.

#### Scenario: Celda con error visible
- **WHEN** la validación marca una celda de la vista previa con error
- **THEN** la celda presenta la señalización visual de validación de WPF y la fila queda disponible en el filtro de filas con error

#### Scenario: Corrección de la celda en la vista filtrada
- **WHEN** se corrige la celda con error en la vista filtrada y los datos se revalidan
- **THEN** la fila deja de estar marcada con error y desaparece del filtro

### Requirement: Servicio de mensajes enriquecido en el front-end WPF
El asistente WPF DEBE (SHALL) notificar información, advertencias y errores a través de una implementación de `ISupportMessageService` presentada en un diálogo WPF propio, con encabezado destacado, pie de página opcional y una sección desplegable para los detalles técnicos del error, y NO DEBE (MUST NOT) depender de diálogos de Windows Forms.

#### Scenario: Error de carga con detalles técnicos
- **WHEN** la carga de un archivo falla y el mensaje incluye los detalles técnicos del error
- **THEN** el diálogo presenta el encabezado, el mensaje principal y los detalles técnicos en una sección desplegable, sin usar diálogos de Windows Forms

#### Scenario: Mensaje sin secciones opcionales
- **WHEN** el mensaje notificado no define pie de página ni detalles técnicos
- **THEN** el diálogo omite esas secciones y presenta solo el encabezado, el mensaje y el icono correspondientes al tipo de mensaje
