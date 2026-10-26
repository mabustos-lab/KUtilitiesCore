# Spec Delta

## Purpose

Define el contrato de materialización de conexiones de `KUtilitiesCore.Dal`: el hook `OnConnectionOpened` (síncrono y asíncrono) debe poder configurar el contexto de sesión de base de datos (p. ej. SESSION_CONTEXT para RLS/auditoría) de forma segura — sin re-entrada, con disparo único por conexión lógica y con semántica fail-closed.

## ADDED Requirements

### Requirement: El handler resuelve la conexión viva sin re-entrada
Cuando el hook de conexión abierta se invoca, el contexto DEBERÁ (SHALL) exponer la conexión ya abierta que disparó el evento: acceder a la conexión desde dentro del handler DEBERÁ (SHALL) devolver exactamente esa instancia, sin re-ejecutar la creación de conexión ni provocar recursión o desbordamiento de pila.

#### Scenario: Handler configura el contexto de sesión
- **WHEN** un handler configurado ejecuta la utilidad de sesión (p. ej. `sp_set_session_context`) usando la conexión expuesta por el contexto
- **THEN** la operación de sesión se completa sin excepciones y la conexión usada es la misma referencia que emplearán las operaciones posteriores del contexto

#### Scenario: Sin recursión desde el handler
- **WHEN** el handler accede a la propiedad `Connection` del contexto durante la materialización de la conexión
- **THEN** el acceso devuelve la conexión ya abierta sin invocar de nuevo la fábrica de conexiones (sin `StackOverflowException` ni conexiones filtradas)

### Requirement: Disparo único por conexión lógica
El hook DEBERÁ (SHALL) dispararse exactamente una vez por conexión lógica del contexto de datos, en su materialización (primer uso). Los reciclajes del pool de ADO.NET NO DEBERÁN (MUST NOT) re-disparar el hook, y los accesos concurrentes a la conexión DEBERÁN (SHALL) esperar a que el hook haya completado antes de continuar.

#### Scenario: Primer acceso dispara el hook una sola vez
- **WHEN** la conexión se materializa por primer vez en el contexto
- **THEN** el hook se invoca exactamente una vez, antes de que cualquier operación de datos del contexto se ejecute

#### Scenario: Accesos posteriores no re-disparan
- **WHEN** la conexión lógica ya está materializada y se ejecutan múltiples operaciones sobre el contexto
- **THEN** el hook no se vuelve a invocar

#### Scenario: Acceso concurrente durante la materialización
- **WHEN** varios hilos acceden a la conexión mientras el hook aún está ejecutándose
- **THEN** todos los hilos esperan a que el hook complete y reciben la misma conexión abierta

### Requirement: Las conexiones de sonda no disparan el hook
Las conexiones temporales o de sonda creadas para consultas internas (p. ej. comprobar existencia de la base de datos) NO DEBERÁN (MUST NOT) invocar el hook ni ejecutar los handlers configurados, y su ciclo de vida DEBERÁ (SHALL) permanecer independiente de la conexión lógica del contexto.

#### Scenario: Sondeo de existencia de base de datos
- **WHEN** se ejecuta la comprobación de existencia de la base de datos antes de materializar la conexión lógica
- **THEN** el hook no se invoca para la conexión de sonda y la conexión lógica no queda creada ni contaminada por el sondeo

### Requirement: Variante asíncrona del hook con orden determinista
El contexto DEBERÁ (SHALL) ofrecer además un hook asíncrono invocable tras la apertura de la conexión, independiente del hook síncrono y configurable por separado. Cuando ambos hooks estén configurados, DEBERÁN (SHALL) ejecutarse en orden determinista documentado: primero el síncrono, luego el asíncrono.

#### Scenario: Solo el hook asíncrono configurado
- **WHEN** solo el hook asíncrono está configurado y la conexión se materializa
- **THEN** el hook asíncrono se invoca una vez, bloqueando la materialización hasta completar la tarea

#### Scenario: Ambos hooks configurados
- **WHEN** ambos hooks (síncrono y asíncrono) están configurados
- **THEN** el hook síncrono se invoca primero y el asíncrono después, siempre en ese orden

### Requirement: Fallo del handler con semántica fail-closed
Si cualquier hook (síncrono o asíncrono) falla durante la materialización, la conexión abierta DEBERÁ (SHALL) disponerse sin fugas, la referencia publicada DEBERÁ (SHALL) limpiarse y el fallo DEBERÁ (SHALL) quedar cacheado: todo acceso posterior a la conexión del contexto DEBERÁ (SHALL) re-lanzar la excepción cacheada. El contexto NO DEBERÁ (MUST NOT) ejecutar ninguna operación de base de datos sin que el handler haya completado.

#### Scenario: Fallo del handler envenena el contexto
- **WHEN** el hook lanza una excepción durante la materialización de la conexión
- **THEN** la conexión abierta se dispone (sin fugas) y todo acceso posterior a la conexión del mismo contexto re-lanza la excepción cacheada, sin reintentos

#### Scenario: Fail-closed para auditoría
- **WHEN** el hook falla y el consumidor intenta ejecutar una operación de datos con el mismo contexto
- **THEN** la operación no se ejecuta contra la base de datos y el consumidor debe crear un contexto nuevo para continuar
