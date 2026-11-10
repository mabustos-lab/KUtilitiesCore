# Spec Delta

## Purpose

Define el contrato de la capa de orquestación compartida del asistente de importación: un componente agnóstico de UI que concentra el estado lógico y las transiciones del asistente (carga, revalidación con debounce, invalidación, reset) para que los front-ends WinForms y WPF consuman una única fuente de verdad sin duplicar lógica.

## ADDED Requirements

### Requirement: Contrato de orquestación neutral a la plataforma
La capa de orquestación DEBE (SHALL) exponer el estado lógico del asistente (ruta de archivo, tabla cargada, carga en curso, datos obsoletos, resultado) y sus operaciones (cargar, revalidar, invalidar, reiniciar) sin tipos ni dependencias de ningún framework de UI.

#### Scenario: Estado consultable sin hilo de UI
- **WHEN** se consulta el estado de la orquestación tras completar una carga
- **THEN** la ruta, la tabla cargada y el indicador de carga en curso son observables sin invocar ningún tipo de framework de UI

#### Scenario: Dependencias de la capa
- **WHEN** se inspecciona el proyecto de la capa de orquestación
- **THEN** no declara `UseWindowsForms` ni `UseWPF` ni referencia proyectos de UI

#### Scenario: Orquestación única para ambos front-ends
- **WHEN** el asistente WinForms y el asistente WPF ejecutan el mismo flujo de carga y validación
- **THEN** ambos llegan al mismo estado observable porque delegan en la misma capa de orquestación

### Requirement: Temporizador de debounce inyectable
La capa DEBE (SHALL) implementar la revalidación silenciosa con debounce mediante una abstracción de temporizador inyectable, de modo que cada front-end aporte su temporizador nativo y las pruebas aporten fakes deterministas.

#### Scenario: Disparo tras la ventana de estabilización
- **WHEN** se programa una revalidación silenciosa con un temporizador de prueba y transcurre la ventana de estabilización
- **THEN** la capa revalida exactamente una vez y sin mostrar diálogos

#### Scenario: Validación explícita cancela el debounce pendiente
- **WHEN** se dispara una validación explícita existiendo una revalidación silenciosa pendiente
- **THEN** la revalidación pendiente queda cancelada y no se ejecuta después

#### Scenario: Ediciones sucesivas coalescen en una sola revalidación
- **WHEN** se programan varias revalidaciones dentro de la ventana de estabilización
- **THEN** la ventana se reinicia con cada edición y solo se ejecuta una revalidación al final

### Requirement: Exposición neutral de errores por fila
La capa DEBE (SHALL) exponer el estado de errores de validación por fila de forma neutral (identificador de fila, campo afectado con su nombre visible y mensaje), sin pintar los errores en ningún control, para que cada front-end los presente con su mecanismo nativo.

#### Scenario: Fallos expuestos tras la validación
- **WHEN** la validación detecta fallos en filas de los datos cargados
- **THEN** la capa expone cada fallo con su fila, el nombre visible del campo afectado y el mensaje de error

#### Scenario: La corrección se refleja en la exposición
- **WHEN** se corrige la celda causante del fallo y los datos se revalidan sin errores
- **THEN** la exposición neutral ya no contiene el fallo de esa fila

### Requirement: Anti-reentrada de la carga propiedad de la capa
La capa DEBE (MUST) rechazar una solicitud de carga mientras otra carga está en curso, garantizando una sola operación de lectura a la vez con independencia del front-end que la solicite.

#### Scenario: Segunda carga durante una carga en curso
- **WHEN** se solicita una segunda carga mientras la capa está cargando datos
- **THEN** la capa no inicia una nueva lectura y la primera carga completa normalmente
