# Spec Delta

## Purpose

Define el comportamiento observable del asistente de importación de archivos: feedback de errores en la vista previa, resultado de importación utilizable por el consumidor, escalabilidad de la interacción (carga, revalidación, invalidación) y exposición de las opciones de análisis de CSV y Excel.

## ADDED Requirements

### Requirement: Filtro de filas con errores funcional con propagación de ediciones
El asistente DEBE (MUST) mostrar en la vista previa las filas con error de validación de los datos cargados, de modo que el filtro "solo filas con errores" muestre exactamente las filas con error y que las ediciones realizadas en la vista filtrada se propaguen a los datos subyacentes.

#### Scenario: El filtro muestra solo las filas con error
- **WHEN** se activa el filtro tras una validación que detecta errores en una de N filas
- **THEN** la vista previa muestra únicamente las filas con error y al desactivarlo vuelve a mostrar todas

#### Scenario: Las ediciones en la vista filtrada propagan
- **WHEN** se corrige en la vista filtrada la celda que causaba el error y se revalida
- **THEN** la fila deja de estar marcada con error y desaparece del filtro

### Requirement: Resultado de importación tipado y propio del consumidor
El resultado expuesto por el asistente DEBE (SHALL) ser una tabla nueva tipada según las definiciones, sin columnas de control, propiedad del consumidor, y utilizable después de que el formulario se cierre y disponga.

#### Scenario: Resultado utilizable tras cerrar el formulario
- **WHEN** el consumidor cierra el formulario (incluyendo su disposición) después de una importación exitosa
- **THEN** el resultado expuesto sigue siendo una tabla válida, tipada y sin columnas de control

### Requirement: Liberación del lector de datos
El asistente DEBE (MUST) liberar el lector de datos creado para cargar el archivo, tanto en el éxito como en el fallo de la carga.

#### Scenario: El archivo no queda retenido tras la carga
- **WHEN** se completa (o falla) la carga de un archivo Excel
- **THEN** el recurso de lectura queda liberado y el archivo puede modificarse o eliminarse sin bloqueo del proceso

### Requirement: No mutación de las definiciones del llamador
El asistente NO DEBE (MUST NOT) modificar las definiciones de campos provistas por el llamador al construir el mapeo activo.

#### Scenario: Las definiciones originales permanecen intactas
- **WHEN** se importa con un mapeo que asigna columnas de origen distintas a las definiciones
- **THEN** las definiciones originales del llamador conservan sus valores de columna de origen

### Requirement: Guard de archivo sin filas
El asistente DEBE (MUST) rechazar la importación de un archivo sin filas de datos con un aviso claro, sin exponer ruido de errores de validación.

#### Scenario: Archivo vacío
- **WHEN** se cargan datos con cero filas y se intenta importar
- **THEN** se muestra un aviso al usuario y el grid de errores no contiene entradas

#### Scenario: Botón de importación deshabilitado
- **WHEN** los datos cargados no contienen filas
- **THEN** la acción de importar permanece deshabilitada

### Requirement: Carga asíncrona no reentrante
El asistente DEBE (SHALL) cargar los datos de forma asíncrona sin bloquear el hilo de interfaz y DEBE (MUST) impedir que una carga se dispare dos veces por reentrada.

#### Scenario: Carga sin bloqueo
- **WHEN** el usuario solicita la carga de un archivo
- **THEN** la interfaz permanece responsive durante la lectura

#### Scenario: Doble clic no dispara doble carga
- **WHEN** se pulsa dos veces la acción de carga mientras una carga está en curso
- **THEN** solo se ejecuta una operación de carga

### Requirement: Revalidación silenciosa con debounce
El asistente DEBE (SHALL) revalidar silenciosamente (sin diálogos) tras un intervalo de estabilización cuando el usuario edita una celda de la vista previa o cambia el mapeo de columnas; una validación explícita DEBE (MUST) cancelar cualquier revalidación pendiente.

#### Scenario: Edición de celda revalida en silencio
- **WHEN** el usuario termina de editar una celda y transcurre el intervalo de estabilización
- **THEN** los datos se revalidan sin mostrar diálogo al usuario

#### Scenario: Cambio de mapeo revalida en silencio
- **WHEN** el usuario cambia la columna de origen de una definición en la grid de mapeo
- **THEN** los datos se revalidan en silencio tras el intervalo de estabilización

### Requirement: Invalidación por cambio de opciones de análisis
El asistente DEBE (MUST) reaccionar a los cambios de opciones de análisis (separador, codificación, hoja, rango de filas) marcando los datos cargados como obsoletos e impidiendo la importación hasta recargar.

#### Scenario: Cambio de opciones con datos cargados
- **WHEN** cambia una opción de análisis habiendo datos cargados
- **THEN** el resultado previo se invalida, se muestra una advertencia y la acción de importar se deshabilita hasta recargar los datos

#### Scenario: Cambio de opciones sin datos cargados
- **WHEN** cambia una opción de análisis sin datos cargados
- **THEN** el asistente no muestra advertencias

### Requirement: Exposición de opciones de análisis CSV
El control de configuración CSV DEBE (SHALL) exponer las opciones TrimValues (quitar espacios) e IgnoreEmptyLines (ignorar líneas vacías) y DEBE (MUST) notificar los cambios de opciones.

#### Scenario: Opciones CSV mapeadas
- **WHEN** se solicitan las opciones de análisis del control CSV
- **THEN** el resultado refleja los valores de TrimValues e IgnoreEmptyLines seleccionados en la UI

### Requirement: Selección de hoja y rango de filas Excel
El control de configuración Excel DEBE (MUST) usar la hoja realmente seleccionada por el usuario y DEBE (SHALL) exponer el rango de filas a leer (fila inicial y fila final opcional, donde cero significa "hasta el final").

#### Scenario: La hoja seleccionada se refleja en las opciones
- **WHEN** el usuario selecciona una hoja distinta de la primera y se solicitan las opciones de análisis
- **THEN** el nombre de la hoja del resultado es la hoja seleccionada

#### Scenario: Rango de filas mapeado
- **WHEN** el usuario define fila inicial y fila final en la UI y se solicitan las opciones de análisis
- **THEN** el resultado refleja ese rango, y una fila final en cero se traduce como "hasta el final" (sin límite)

### Requirement: Presentación coherente de errores
El grid de errores del asistente DEBE (SHALL) mostrar los fallos de validación identificados por el nombre visible (`DisplayName`) de la definición afectada, con un identificador consistente para todos los tipos de fallo.

#### Scenario: DisplayName en el grid de errores
- **WHEN** la validación produce fallos en un campo con nombre visible definido
- **THEN** el grid de errores muestra el nombre visible del campo en la columna Campo

### Requirement: Reset de estado al vaciar el archivo
El asistente DEBE (MUST) reiniciar su estado (vista previa, resultado, mapeo y acciones) cuando la ruta de archivo se asigna vacía.

#### Scenario: Asignación de ruta vacía
- **WHEN** se asigna una ruta de archivo vacía
- **THEN** la vista previa, el resultado y la grid de mapeo quedan limpios y las acciones de carga e importar quedan deshabilitadas
