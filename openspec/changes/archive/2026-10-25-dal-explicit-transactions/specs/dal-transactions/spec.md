# Spec Delta — dal-transactions

## Purpose

Define el contrato transaccional de `KUtilitiesCore.Dal`: las transacciones son siempre explícitas —creadas por el consumidor y jamás por el Unit of Work como efecto secundario—, se enlistan por parámetro opcional en cada operación de datos y se revierten automáticamente si se disponen sin confirmar.

## ADDED Requirements

### Requirement: El Unit of Work nunca abre transacciones implícitas
El Unit of Work de Dal NO DEBERÁ (MUST NOT) crear transacciones por sí mismo: `IDaoUowContext.Transaction` DEBERÁ (SHALL) devolver únicamente la transacción registrada explícitamente; sin registro DEBERÁ (SHALL) devolver null y no abrirá transacción alguna como efecto secundario del acceso a la propiedad, ni directa ni indirectamente a través de los accesos que los repositorios exponen.

#### Scenario: Acceso sin transacción registrada
- **WHEN** se accede a `Transaction` —directamente o vía los accesos de los repositorios— sin transacción registrada
- **THEN** devuelve null en todos los accesos y el contexto de datos no abre transacción alguna

#### Scenario: Ni SaveChanges ni Rollback abren transacción
- **WHEN** se invoca `SaveChanges` o `Rollback` sin transacción registrada
- **THEN** ninguno de los dos abre una transacción como efecto secundario

### Requirement: Creación explícita de transacciones
El consumidor DEBERÁ (SHALL) poder crear transacciones explícitas de dos formas: mediante `IDaoContext.BeginTransaction(IsolationLevel)` —conservando su default actual (`Snapshot`)— o mediante la API aditiva `DaoUnitOfWork.BeginTransaction(IsolationLevel)` cuyo default DEBERÁ (SHALL) ser `ReadCommitted`. La transacción creada vía UoW DEBERÁ (SHALL) quedar registrada en él; la creada directamente en el contexto de datos no.

#### Scenario: Creación directa en el contexto de datos
- **WHEN** el consumidor invoca `BeginTransaction` sobre el contexto de datos
- **THEN** obtiene una transacción independiente del estado del UoW (sin quedar registrada)

#### Scenario: Creación vía UoW con default ReadCommitted
- **WHEN** el consumidor invoca `DaoUnitOfWork.BeginTransaction()` sin argumentos
- **THEN** se crea una transacción `ReadCommitted` y queda registrada en el UoW

#### Scenario: Nivel de aislamiento explícito vía UoW
- **WHEN** el consumidor invoca `DaoUnitOfWork.BeginTransaction(isolationLevel)` con un nivel explícito
- **THEN** la transacción se crea con ese nivel y queda registrada en el UoW

### Requirement: Enlistamiento opcional por parámetro
Toda operación de datos del Dal DEBERÁ (SHALL) aceptar un parámetro de transacción opcional: omitido, el comando se ejecutará sin transacción (autocommit); provisto, el comando se enlistará en esa transacción; y una transacción ajena a la conexión del comando DEBERÁ (SHALL) fallar con un error explícito antes de ejecutarse.

#### Scenario: Operación sin transacción (compatibilidad SP Gateway)
- **WHEN** se ejecuta una operación —incluidos procedimientos almacenados gateway— omitiendo el parámetro de transacción
- **THEN** el comando se ejecuta sin enlistarse en transacción alguna, preservando el autocommit actual

#### Scenario: Operación enlistada
- **WHEN** se ejecuta una operación pasando una transacción viva de la misma conexión
- **THEN** el comando se ejecuta enlistado en esa transacción

#### Scenario: Transacción de otra conexión
- **WHEN** se pasa una transacción perteneciente a una conexión distinta a la del comando
- **THEN** la operación falla con un error explícito sin ejecutar el comando

### Requirement: Finalización explícita con reversión automática
Una transacción persistirá únicamente con `Commit` explícito; disponerla sin confirmar DEBERÁ (SHALL) revertirla automáticamente. `Commit`, `Rollback` y `Dispose` DEBERÁN (SHALL) ser idempotentes y toda invocación posterior a la finalización será no-op.

#### Scenario: Dispose sin Commit
- **WHEN** se dispone una transacción sin haberla confirmado
- **THEN** la transacción se revierte automáticamente

#### Scenario: Commit explícito persiste
- **WHEN** se invoca `Commit` y posteriormente se dispone la transacción
- **THEN** los cambios quedan persistidos y el Dispose no revierte nada

#### Scenario: Finalización idempotente
- **WHEN** se invoca `Commit`, `Rollback` o `Dispose` sobre una transacción ya finalizada
- **THEN** la llamada es no-op y no lanza excepciones

### Requirement: SaveChanges y Rollback según transacción registrada
`SaveChanges` DEBERÁ (SHALL) confirmar y devolver 1 únicamente cuando exista una transacción registrada viva; sin transacción registrada —o con la transacción ya finalizada— DEBERÁ (SHALL) devolver 0 silenciosamente. `Rollback` DEBERÁ (SHALL) revertir la transacción registrada viva y dejar el UoW sin transacción; sin transacción será no-op silencioso.

#### Scenario: Sin transacción registrada
- **WHEN** se invoca `SaveChanges` o `Rollback` sin transacción registrada
- **THEN** `SaveChanges` devuelve 0 y `Rollback` es no-op, ambos silenciosos

#### Scenario: SaveChanges con transacción registrada viva
- **WHEN** se invoca `SaveChanges` existiendo una transacción registrada viva
- **THEN** la transacción se confirma y el método devuelve 1

#### Scenario: SaveChanges con transacción ya finalizada
- **WHEN** la transacción registrada fue confirmada o revertida y se invoca `SaveChanges`
- **THEN** se comporta como si no hubiese transacción: devuelve 0 sin confirmar nada

#### Scenario: Rollback con transacción registrada viva
- **WHEN** se invoca `Rollback` existiendo una transacción registrada viva
- **THEN** la transacción se revierte y el UoW queda sin transacción

### Requirement: Limpieza de recursos sin fugas
Disponer el UoW DEBERÁ (SHALL) disponer la transacción registrada —revirtiéndola automáticamente si no fue confirmada— y el contexto de datos; toda eliminación de la transacción registrada DEBERÁ (SHALL) dejar al UoW sin transacción; y las eliminaciones repetidas NO DEBERÁN (MUST NOT) lanzar excepciones ni duplicar liberaciones.

#### Scenario: Dispose del UoW con transacción sin confirmar
- **WHEN** se dispone el UoW existiendo una transacción registrada no confirmada
- **THEN** la transacción se revierte automáticamente y sus recursos se liberan junto con el contexto

#### Scenario: Dispose repetido
- **WHEN** se dispone el UoW o la transacción más de una vez
- **THEN** ninguna llamada lanza excepciones ni duplica la liberación de recursos
