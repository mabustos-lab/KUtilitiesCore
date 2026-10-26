---
description: Enforce mandatory steps from openspec/config.yaml when creating tasks.md artifacts and ensure agent executes all manual tests
alwaysApply: true
---
# Tareas de OpenSpec: aplicación de los pasos obligatorios

Al crear o actualizar los artefactos `tasks.md` en los cambios de OpenSpec, DEBES:

## 1. Lee primero el archivo openspec/config.yaml

**ANTES** de crear o actualizar cualquier archivo `tasks.md`, DEBES leer `openspec/config.yaml` para comprender:
- Los pasos obligatorios específicos del backend y del frontend
- Las convenciones de nomenclatura de las ramas
- Los requisitos de estructura de las tareas
- Los requisitos de pruebas y documentación

## 2. Pasos obligatorios

Todas las tareas de implementación DEBEN incluir estos pasos en el orden correcto. La aplicabilidad de cada paso depende del **tipo de proyecto** — ver la Nota de Validación a continuación, que es de lectura obligatoria antes de generar `tasks.md`.
### Paso 0: Crear la rama de la característica (OBLIGATORIO COMO PRIMER PASO)
- **Ubicación**: Debe ser el primer paso (Paso 0)
- **Nombre de la rama**: `feature/[ticket-id]` o `feature/[nombre-del-cambio]`
- **Acción**: Crear y cambiarse a la rama de la característica (_feature branch_) antes de realizar cualquier cambio en el código

### **Nota de Validación del Proyecto (Antes de iniciar) — LECTURA OBLIGATORIA

> IMPORTANTE: Antes de proceder, el agente DEBE inspeccionar la estructura del repositorio y su documentación de referencia (p. ej. AGENTS.md, code-base.md) para determinar qué componentes tiene realmente el proyecto:
> 
>¿Usa base de datos o algún tipo de persistencia?
>¿Expone una API HTTP con endpoints (REST, GraphQL, etc.)?
>¿Tiene una interfaz web / frontend con flujos de usuario navegables?
>
>Los pasos N+1 (únicamente su sección de base de datos), N+2 y N+3 son condicionales, no incondicionales. Si el proyecto es una librería aislada (por ejemplo, una .dll de C#/.NET consumida como referencia de otro proyecto, un paquete npm, una librería de cálculo sin capa de servicio, etc.) sin API HTTP expuesta, sin base de datos y sin interfaz web, entonces:
>
>Estos pasos —junto con todos sus sub-pasos, plantillas de reporte, tablas de dependencias y notas asociadas— deben omitirse por completo y automáticamente, sin romper el flujo de ejecución.
>El agente no debe incluirlos en tasks.md, ni siquiera como pasos marcados "N/A". Si no aplican, simplemente no aparecen en el archivo.
>Esta condición de omisión aplica exactamente igual a lo descrito en las secciones 3, 4, 6 y 7 de esta norma: todo lo que ahí se etiqueta como "OBLIGATORIO" u "OBLIGATORIO QUE EL AGENTE LO EJECUTE" para BD/curl/Playwright está sujeto a esta misma condición de aplicabilidad, aunque en el texto original de esas secciones no se repita cada vez.
>
>Ejemplo aplicado: para un proyecto tipo "librería DLL de cálculo, sin base de datos, sin API HTTP y sin frontend" (ver Apéndice al final de este documento), el Paso N+1 se aplica solo en su parte de pruebas unitarias (la parte de base de datos se omite), y los Pasos N+2 y N+3 se omiten en su totalidad.
### Pasos Obligatorios (deben incluirse según corresponda):

- **Paso N**: Revisar y actualizar las pruebas unitarias existentes (OBLIGATORIO siempre)
- **Paso N+1**: Ejecutar pruebas unitarias y verificar el estado de la base de datos — la ejecución de pruebas unitarias es OBLIGATORIA siempre; la verificación de base de datos es OBLIGATORIA **solo si el proyecto utiliza base de datos** (si no la usa, esa sub-sección se omite por completo, sin dejar rastro en `tasks.md`)
- **Paso N+2**: Pruebas manuales de endpoints con curl — OBLIGATORIO **solo si el proyecto expone una API/endpoints HTTP**. **Se omite por completo** (no se incluye en `tasks.md`) en librerías o paquetes sin capa de API
- **Paso N+3**: Pruebas E2E con Playwright MCP — OBLIGATORIO **solo si el proyecto tiene interfaz web o flujos de extremo a extremo navegables en navegador**. **Se omite por completo** si no hay frontend
- **Paso N+4**: Actualizar la documentación técnica (OBLIGATORIO siempre)

## 3. Requisitos de pruebas manuales — Crítico: el agente debe ejecutarlas (cuando el paso aplique)

**IMPORTANTE**: Cuando un paso de prueba manual SÍ aplique según la Nota de Validación del Proyecto, el agente de código (IA) DEBE ejecutar por sí mismo todos los pasos de prueba manual. **NUNCA delegar las pruebas al usuario**. Estas pruebas deben ser ejecutadas por el agente para poder marcar las tareas correspondientes como completadas en `tasks.md`.

Si un paso NO aplica (por ejemplo, un proyecto sin API HTTP o sin frontend, como una librería DLL aislada), esta sección no impone ninguna obligación sobre ese paso: simplemente no se incluye en `tasks.md` y no hay nada que ejecutar ni que reportar al respecto.

### ### Paso N+1: Ejecutar pruebas unitarias y verificar el estado de la base de datos (OBLIGATORIO)

> Condición de omisión: Si el proyecto no utiliza persistencia (por ejemplo, una librería DLL aislada), la sección de verificación de base de datos se omite de forma automática, pero la ejecución de pruebas unitarias y la generación del reporte siguen siendo obligatorias.

**Responsabilidad del agente**: el agente de programación DEBE ejecutar las pruebas unitarias, validar la integridad de la base de datos antes y después de la ejecución (solo si aplica), y generar un reporte de pruebas como artefacto en la carpeta de especificación del cambio. Esto NO es opcional y no se puede delegar al usuario.

**Pasos de implementación** (el agente debe realizar lo siguiente):

1. Preparar el entorno de pruebas:
    - Asegurar que los servicios requeridos estén disponibles (base de datos, caché, dependencias). _(Omitir base de datos si no aplica)_.
    - Capturar el estado de la base de datos previo a las pruebas que sea relevante para el cambio (conteos, registros clave, sumas de verificación o instantáneas/snapshots). _(Omitir si no aplica)_.
    - Documentar los comandos exactos de prueba que se van a ejecutar.
2. Ejecutar primero las pruebas unitarias específicas:
    - Ejecutar pruebas enfocadas en los módulos modificados y comportamientos relacionados.
    - Confirmar que los fallos se hayan resuelto y que no aparezcan nuevas regresiones en el alcance específico.
    - Capturar el resumen del resultado del comando (aprobadas/falladas/omitidas).
3. Ejecutar la suite general de pruebas unitarias:
    - Ejecutar la suite de pruebas unitarias del proyecto requerida por `openspec/config.yaml` (o un subconjunto justificado si está configurado).
    - Registrar el conteo total de pruebas, fallos, tiempo de ejecución y cualquier comportamiento inestable (_flaky_) observado.
4. Verificar el estado de la base de datos posterior a las pruebas _(omitir por completo este paso si el proyecto no usa base de datos)_:
    - Volver a comprobar los mismos indicadores de la base de datos capturados antes de las pruebas.
    - Confirmar que no queden mutaciones no deseadas después de que se completen las pruebas.
    - Si ocurrió alguna mutación, restaurar el estado y documentar dicha restauración.
5. Crear el reporte de verificación de pruebas unitarias en la carpeta Spec:
    - Guardar el reporte bajo la carpeta del cambio actual en `specs/<change-name>/reports/`.
    - Usar este patrón de nombre de archivo: `AAAA-MM-DD-paso-N+1-verificacion-de-pruebas-unitarias-y-bd.md` (ajustar el nombre si se omitió la BD, p. ej. `...-verificacion-de-pruebas-unitarias.md`).
    - Incluir los comandos ejecutados, los resultados resumidos, la comparación del antes/después de la base de datos (si aplica) y las acciones de limpieza realizadas.
6. Marcar la tarea como completada: solo después de que las pruebas unitarias se hayan aprobado (o se documenten excepciones aprobadas), se haya verificado/restaurado el estado de la base de datos (si aplica) y se haya creado el archivo del reporte, marcar el Paso N+1 como completado en `tasks.md`.

**Plantilla de informe** (guardar en `specs/<change-name>/reports/`):
```markdown
# Informe del paso N+1: pruebas unitarias y verificación de la base de datos

- Fecha: AAAA-MM-DD
- Cambio: <nombre-del-cambio>
- Agente: <nombre-del-agente>

## Comandos ejecutados
- `<comando 1>`
- `<comando 2>`

## Resultados de las pruebas unitarias
- Pruebas específicas: X aprobadas, Y fallidas, Z omitidas
- Conjunto completo/obligatorio: X aprobadas, Y fallidas, Z omitidas
- Tiempo de ejecución: <duración>
- Notas: <pruebas poco fiables, reintentos, excepciones>

## Verificación del estado de la base de datos (solo si el proyecto usa base de datos; de lo contrario, omitir esta sección completa)
- Línea base previa a la prueba:
  - <métrica/tabla/verificación>: <valor>
- Validación posterior a la prueba:
  - <métrica/tabla/verificación>: <valor>
- Estado restablecido: Sí/No
- Acciones de restauración (si las hubiera): <acciones>

## Resultado
- Estado del paso N+1: APROBADO / FALLÓ
- Problemas de bloqueo: <ninguno o lista>
```

**Dependencias**:
- Instalar dependencias de pruebas del proyecto
- Acceso a la base de datos para la verificación o restauración del estado _(Omitir si no aplica)_.
- Permiso para crear archivos de informe en `specs/<change-name>/reports/`

**Notas**:

- **El agente DEBE ejecutar las pruebas por sí mismo**; nunca se debe pedir al usuario que las ejecute.
- Este paso es obligatorio incluso cuando los cambios en el código parezcan insignificantes.
- La denominación del nombre de los informes debe seguir el patrón exigido para garantizar la trazabilidad.
- **La finalización de la tarea en `tasks.md` solo se puede marcar tras la creación del informe.**
- En proyectos sin base de datos, todo lo relativo a "verificación de BD" en la plantilla y en las dependencias se omite; el resto del paso (pruebas unitarias + reporte) sigue siendo obligatorio.

### ### Paso N+2: Pruebas manuales de endpoints con curl (OBLIGATORIO **solo si el proyecto expone una API HTTP**)

> **Condición de omisión — leer antes de aplicar este paso**: este paso completo (todos sus sub-pasos, dependencias y notas) aplica **únicamente** cuando el proyecto expone endpoints HTTP (REST, GraphQL u otro protocolo similar) que un cliente externo pueda invocar. Si el proyecto es una **librería aislada sin capa de API** (por ejemplo, una DLL de C#/.NET consumida directamente por otro proyecto, sin servidor HTTP propio), este paso se **omite por completo y no debe aparecer en `tasks.md`**, ni siquiera como referencia.

**Responsabilidad del agente**: cuando este paso aplique, el agente de código DEBE ejecutar todos los comandos curl y verificar las respuestas. Esto NO es opcional y no se puede delegar al usuario.

**Pasos de implementación** (el agente debe realizar lo siguiente, solo si el paso aplica):

1. **Preparar el entorno de pruebas**:
    - Asegurar que el servidor backend esté en ejecución (iniciarlo si es necesario).
    - Verificar que la conexión a la base de datos esté activa (si el proyecto usa base de datos).
    - Anotar el estado actual de la base de datos (si se van a probar endpoints de CREATE/UPDATE/DELETE).
2. **Probar endpoints GET** (si los hay):
    - Crear el comando curl para probar el endpoint GET.
    - Ejecutar el comando: `curl -X GET [url-del-endpoint] [headers]`.
    - Verificar el código de estado de la respuesta (200, 404, etc.).
    - Verificar la estructura y el contenido del cuerpo de la respuesta.
    - Documentar el comando curl y la respuesta en la tarea completada.
3. **Probar endpoints POST** (operaciones CREATE):
    - Crear el comando curl con el cuerpo de la petición: `curl -X POST [url-del-endpoint] -H "Content-Type: application/json" -d '[cuerpo-json]'`.
    - Ejecutar el comando y capturar la respuesta.
    - Verificar el código de estado (201, 400, 422, etc.).
    - Verificar que el cuerpo de la respuesta contenga el recurso creado.
    - **Restaurar el estado de la base de datos**: después de la prueba, eliminar el registro creado para restaurar la base de datos a su estado original.
    - Documentar el comando curl, la respuesta y la acción de limpieza.
4. **Probar endpoints PUT/PATCH** (operaciones UPDATE):
    - Crear el comando curl con los datos actualizados: `curl -X PUT [url-del-endpoint] -H "Content-Type: application/json" -d '[cuerpo-json]'`.
    - Ejecutar el comando y capturar la respuesta.
    - Verificar el código de estado (200, 404, 400, etc.).
    - Verificar que el cuerpo de la respuesta contenga el recurso actualizado.
    - **Restaurar el estado de la base de datos**: después de la prueba, revertir el registro actualizado a sus valores originales.
    - Documentar el comando curl, la respuesta y la acción de limpieza.
5. **Probar endpoints DELETE**:
    - Crear el comando curl: `curl -X DELETE [url-del-endpoint]`.
    - Ejecutar el comando y capturar la respuesta.
    - Verificar el código de estado (200, 204, 404, etc.).
    - Verificar que la eliminación fue exitosa.
    - **Restaurar el estado de la base de datos**: después de la prueba, recrear el registro eliminado con sus valores originales.
    - Documentar el comando curl, la respuesta y la acción de limpieza.
6. **Probar casos de error**:
    - Probar con datos inválidos (errores de validación).
    - Probar con recursos inexistentes (errores 404).
    - Probar con acceso no autorizado (si aplica).
    - Verificar que el formato de la respuesta de error coincida con la especificación de la API.
7. **Marcar la tarea como completada**: solo después de que todas las pruebas curl pasen y el estado de la base de datos esté restaurado, marcar la tarea como completada en `tasks.md`.

**Dependencias** (solo si el paso aplica):

- Servidor backend en ejecución (el agente debe iniciarlo si es necesario)
- Acceso a la base de datos para la restauración del estado (si el proyecto usa base de datos)
- Herramienta de línea de comandos curl

**Notas**:

- Este paso es OBLIGATORIO para todos los endpoints nuevos, **únicamente cuando el proyecto expone una API HTTP**.
- **El agente DEBE ejecutar todos los comandos curl por sí mismo**; nunca se debe pedir al usuario que ejecute las pruebas.
- Todas las operaciones CREATE/UPDATE/DELETE deben restaurar la base de datos a su estado original después de la prueba (si aplica base de datos).
- Documentar todos los comandos curl y respuestas para referencia futura, en un reporte dentro de la carpeta del cambio, con el nombre adecuado.
- No omitir las pruebas manuales aunque las pruebas unitarias pasen.
- **La finalización de la tarea en `tasks.md` solo se puede marcar tras la ejecución exitosa de todas las pruebas curl.**
- **Si el proyecto no expone una API HTTP (p. ej. una librería DLL aislada), todo este Paso N+2 —incluidas estas dependencias y notas— se omite y no se agrega a `tasks.md`.**

### ### Paso N+3: Pruebas E2E con Playwright MCP (OBLIGATORIO **solo si el proyecto tiene interfaz web**)

> **Condición de omisión — leer antes de aplicar este paso**: este paso completo (todos sus sub-pasos, dependencias y notas) aplica **únicamente** cuando el proyecto tiene una interfaz web / frontend con flujos de usuario navegables en un navegador. Si el proyecto no tiene frontend (por ejemplo, una librería DLL de cálculo sin UI, como es el caso de una biblioteca de clases .NET consumida por otros programas), este paso se **omite por completo y no debe aparecer en `tasks.md`**, ni siquiera como referencia.

**Cuándo aplica este paso**:

- Cambios de frontend que afectan flujos de usuario.
- Integración entre frontend y endpoints de backend.
- Funcionalidades de cara al usuario que requieren interacción en navegador.

**Responsabilidad del agente**: cuando este paso aplique, el agente de código DEBE ejecutar todas las pruebas E2E usando las herramientas de Playwright MCP. Esto NO es opcional y no se puede delegar al usuario.

**Pasos de implementación** (el agente debe realizar lo siguiente, solo si el paso aplica):

1. **Preparar el entorno de pruebas**:
    - Asegurar que tanto el frontend como el backend estén en ejecución (iniciarlos si es necesario).
    - Verificar que la base de datos esté en un estado conocido (si aplica).
    - Comprobar las herramientas de Playwright MCP disponibles.
2. **Navegar a la aplicación**:
    - Usar la herramienta Playwright MCP `browser_navigate` para abrir la URL de la aplicación.
    - Esperar a que la página cargue completamente.
    - Tomar una instantánea (_snapshot_) para verificar el estado inicial.
3. **Ejecutar flujos de usuario**:
    - Usar las herramientas de Playwright MCP para interactuar con la interfaz:
        - `browser_click` para clics en botones y navegación.
        - `browser_type` o `browser_fill` para llenar formularios.
        - `browser_snapshot` para verificar cambios de estado.
        - `browser_wait` para operaciones asíncronas.
    - Probar el flujo de usuario completo, de inicio a fin.
    - Verificar los resultados esperados en cada paso.
4. **Probar escenarios de error**:
    - Probar errores de validación de formularios.
    - Probar que los mensajes de error se muestren correctamente.
    - Probar los flujos de recuperación ante errores.
5. **Verificar la persistencia de datos**:
    - Después de crear/actualizar datos mediante la interfaz, verificar que persistan correctamente.
    - Comprobar que el estado de la base de datos coincida con el estado mostrado en la interfaz.
    - Verificar que los datos aparezcan correctamente en las vistas de listado/detalle.
6. **Restaurar el entorno de pruebas**:
    - Limpiar cualquier dato de prueba creado durante las pruebas E2E.
    - Restaurar la base de datos a su estado original.
    - Cerrar las sesiones del navegador.
7. **Marcar la tarea como completada**: solo después de que todas las pruebas E2E pasen y el entorno esté restaurado, marcar la tarea como completada en `tasks.md`.

**Dependencias** (solo si el paso aplica):

- Servidor frontend en ejecución (el agente debe iniciarlo si es necesario)
- Servidor backend en ejecución (el agente debe iniciarlo si es necesario)
- Herramientas de Playwright MCP disponibles
- Acceso a la base de datos para verificación y limpieza (si aplica)

**Notas**:

- **El agente DEBE ejecutar todas las pruebas E2E por sí mismo**; nunca se debe pedir al usuario que las ejecute.
- Usar esperas incrementales (1-3 segundos) con verificaciones de instantánea en lugar de esperas largas.
- Restaurar siempre el estado de la base de datos después de pruebas que modifiquen datos.
- Documentar los escenarios de prueba y sus resultados en un reporte dentro de la carpeta del cambio, con el nombre adecuado.
- **La finalización de la tarea en `tasks.md` solo se puede marcar tras la ejecución exitosa de todas las pruebas E2E.**
- **Si el proyecto no tiene interfaz web (p. ej. una librería DLL aislada), todo este Paso N+3 —incluidas estas dependencias y notas— se omite y no se agrega a `tasks.md`.**

## 4. Lista de verificación

Antes de finalizar cualquier archivo `tasks.md`, verificar:

- [ ]  El Paso 0 (Crear rama de la característica) es el PRIMER paso.
- [ ]  Se incluyen todos los pasos obligatorios de `openspec/config.yaml` **que apliquen según el tipo de proyecto** (ver Nota de Validación).
- [ ]  Los pasos están numerados secuencialmente.
- [ ]  Los pasos obligatorios están claramente marcados con la etiqueta "(OBLIGATORIO)".
- [ ]  La nomenclatura de la rama sigue la convención: `feature/[nombre]-backend` (u otra definida en `openspec/config.yaml`).
- [ ]  El Paso N+1 incluye la ruta y convención de nombre del reporte en `specs/<nombre-del-cambio>/reports/`.
- [ ]  Cuando el Paso N+2 y/o N+3 apliquen, los pasos de prueba manual indican explícitamente "EL AGENTE DEBE EJECUTARLAS".
- [ ]  Cuando el proyecto usa base de datos, las tareas incluyen pasos de restauración del estado de la base de datos.
- [ ]  El paso de pruebas E2E (N+3) se incluye únicamente si el proyecto tiene cambios de frontend; en caso contrario, **no se incluye en absoluto** (no basta con marcarlo "N/A": se omite del archivo).
- [ ]  El paso de pruebas con curl (N+2) se incluye únicamente si el proyecto expone una API HTTP; en caso contrario, **no se incluye en absoluto**.
- [ ]  Si el proyecto es una librería aislada (DLL, paquete, etc.) sin base de datos, sin API HTTP y sin frontend, el archivo `tasks.md` generado **no contiene** ningún sub-paso de verificación de BD, ningún Paso N+2 ni ningún Paso N+3 — únicamente Paso 0, Paso N (pruebas unitarias), Paso N+1 (solo pruebas unitarias + reporte) y Paso N+4 (documentación).

## 5. Cuándo aplica esta norma

Esta norma aplica cuando:
- Se crea `tasks.md` mediante `/opsx:ff` (fast-forward) o el skill `openspec-ff-change`.
- Se crea `tasks.md` mediante `/opsx:continue` (continuar cambio) o el skill `openspec-continue-change`.
- Se actualizan archivos `tasks.md` existentes.
- Cualquier creación de tareas que involucre cambios de backend.
- Se implementan tareas de `tasks.md` mediante `/opsx:apply` o el skill `openspec-apply-change` — el agente debe ejecutar las pruebas manuales que apliquen.

En todos los casos, la aplicabilidad de los Pasos N+1 (sección BD), N+2 y N+3 se determina primero según la Nota de Validación del Proyecto de la sección 2.
## 6. Estructura de ejemplo

### 6.1 Ejemplo para un proyecto full-stack (API + base de datos + frontend)

Este ejemplo aplica cuando el proyecto SÍ tiene base de datos, SÍ expone API HTTP y SÍ tiene frontend. **No usar este ejemplo tal cual para librerías aisladas** — ver la sección 6.2.

```markdown
## 0. Configuración: Crear rama de la característica (OBLIGATORIO - PRIMER PASO)

- [ ] 0.1 Crear la rama `feature/update-position-backend` a partir de main/master
- [ ] 0.2 Verificar la creación de la rama y el estado de la rama actual

## 1. Backend: Pruebas del validador (TDD)
...

## 8. Backend: Revisar y actualizar pruebas unitarias existentes (OBLIGATORIO)
...

## 9. Backend: Ejecutar pruebas unitarias y verificar el estado de la base de datos (OBLIGATORIO)
- [ ] 9.1 Capturar la línea base de la base de datos previa a la prueba para las entidades afectadas
- [ ] 9.2 Ejecutar las pruebas unitarias específicas de los módulos modificados
- [ ] 9.3 Ejecutar la suite de pruebas unitarias obligatoria según la configuración
- [ ] 9.4 Verificar el estado de la base de datos posterior a la prueba y restaurar si es necesario
- [ ] 9.5 Crear el reporte `specs/<nombre-del-cambio>/reports/AAAA-MM-DD-paso-N+1-verificacion-de-pruebas-unitarias-y-bd.md`
- [ ] 9.6 Marcar el paso como completado solo después de que las pruebas pasen y el reporte exista

## 10. Backend: Pruebas manuales de endpoints con curl (OBLIGATORIO - EL AGENTE DEBE EJECUTARLAS)
- [ ] 10.1 Asegurar que el servidor backend esté en ejecución
- [ ] 10.2 Probar endpoints GET con curl y verificar las respuestas
- [ ] 10.3 Probar endpoints POST con curl, verificar la creación y restaurar el estado de la BD
- [ ] 10.4 Probar endpoints PUT/PATCH con curl, verificar las actualizaciones y restaurar el estado de la BD
- [ ] 10.5 Probar endpoints DELETE con curl, verificar la eliminación y restaurar el estado de la BD
- [ ] 10.6 Probar casos de error (errores de validación, 404, etc.)
- [ ] 10.7 Documentar todos los comandos curl y sus respuestas
- [ ] 10.8 Verificar que el estado de la BD coincida con el estado previo a la prueba

## 11. Frontend: Pruebas E2E con Playwright MCP (OBLIGATORIO si aplica - EL AGENTE DEBE EJECUTARLAS)
- [ ] 11.1 Asegurar que el frontend y el backend estén en ejecución
- [ ] 11.2 Navegar a la aplicación con Playwright MCP `browser_navigate`
- [ ] 11.3 Ejecutar el flujo de usuario completo con las herramientas de Playwright MCP
- [ ] 11.4 Probar escenarios de error y validación
- [ ] 11.5 Verificar la persistencia de datos y el estado de la UI
- [ ] 11.6 Restaurar el entorno de pruebas y el estado de la base de datos
- [ ] 11.7 Documentar los escenarios de prueba y sus resultados

## 16. Actualizar documentación técnica (OBLIGATORIO)
...
```
### 6.2 Ejemplo para una librería aislada sin BD, sin API y sin frontend (p. ej. una DLL de C#/.NET)

Este es el caso de proyectos como una biblioteca de clases .NET consumida por otro programa, sin persistencia, sin servidor HTTP y sin interfaz web. Nótese que **no existen** pasos equivalentes a "verificación de BD", "10. Pruebas con curl" ni "11. Pruebas E2E":

```markdown
## 0. Configuración: Crear rama de la característica (OBLIGATORIO - PRIMER PASO)

- [ ] 0.1 Crear la rama `feature/add-decline-curve-formula` a partir de main/master
- [ ] 0.2 Verificar la creación de la rama y el estado de la rama actual

## 1. Pruebas del validador / componente (TDD)
...

## 8. Revisar y actualizar pruebas unitarias existentes (OBLIGATORIO)
...

## 9. Ejecutar pruebas unitarias (OBLIGATORIO)
- [ ] 9.1 Ejecutar las pruebas unitarias específicas de los módulos modificados
- [ ] 9.2 Ejecutar la suite de pruebas unitarias obligatoria según la configuración
- [ ] 9.3 Crear el reporte `specs/<nombre-del-cambio>/reports/AAAA-MM-DD-paso-N+1-verificacion-de-pruebas-unitarias.md`
- [ ] 9.4 Marcar el paso como completado solo después de que las pruebas pasen y el reporte exista

## 10. Actualizar documentación técnica (OBLIGATORIO)
...
```
## 7. Requisitos de ejecución del agente (solo cuando N+2 y/o N+3 apliquen)

Todo lo indicado en esta sección aplica **exclusivamente** a los Pasos N+2 y N+3 cuando estén presentes en `tasks.md` según la condicionalidad definida en la sección 2. Para proyectos sin API HTTP y sin frontend (p. ej. una librería DLL aislada), esta sección no impone ninguna obligación, ya que dichos pasos no existen en `tasks.md`.

**CRÍTICO**: Al implementar tareas de `tasks.md` (mediante el skill `openspec-apply-change` o el comando `/opsx:apply`), cuando los Pasos N+2 y/o N+3 estén presentes, el agente de código DEBE:

1. **Ejecutar todas las pruebas manuales**: nunca pedir al usuario que ejecute comandos curl o pruebas E2E. El agente debe:
    - Iniciar los servidores necesarios (backend, frontend).
    - Ejecutar todos los comandos curl para probar los endpoints.
    - Ejecutar todas las pruebas E2E usando las herramientas de Playwright MCP.
    - Verificar todas las respuestas y resultados.
    - Restaurar el estado de la base de datos después de las pruebas.
2. **Marcar las tareas como completadas**: las tareas de estos pasos SOLO pueden marcarse como completadas (`[x]`) DESPUÉS de que:
    - El agente haya ejecutado exitosamente todas las pruebas requeridas.
    - Todos los resultados de prueba hayan sido verificados.
    - El estado de la base de datos haya sido restaurado (para operaciones CREATE/UPDATE/DELETE).
    - Todos los resultados de las pruebas hayan sido documentados.
3. **Nunca delegar las pruebas**: el agente nunca debe:
    - Pedir al usuario que ejecute comandos curl.
    - Pedir al usuario que pruebe endpoints manualmente.
    - Pedir al usuario que ejecute pruebas E2E.
    - Marcar tareas como completadas sin haber ejecutado las pruebas.
    - Omitir pasos de prueba manual que sí apliquen según la Nota de Validación.
4. **Documentar la ejecución de las pruebas**: el agente debe documentar:
    - Todos los comandos curl ejecutados.
    - Todas las respuestas recibidas.
    - Todos los escenarios de prueba E2E ejecutados.
    - Las acciones de restauración del estado de la base de datos.
    - Cualquier incidencia encontrada y su resolución.

## Incumplimiento

Si se crean tareas sin seguir estos pasos obligatorios (incluyendo la correcta omisión de los pasos que no apliquen), el usuario tendrá que corregir manualmente el archivo `tasks.md`. Siempre leer primero `openspec/config.yaml` y asegurarse de incluir todos los pasos obligatorios que correspondan al tipo de proyecto — ni de más (pasos que no aplican) ni de menos (pasos que sí aplican).

**Si se implementan tareas sin ejecutar las pruebas manuales que sí aplican, se está violando esta norma. El agente debe ejecutar todas las pruebas correspondientes para poder marcar las tareas como completadas.**

___
## Apéndice: aplicación a un proyecto tipo "librería DLL sin BD, sin API y sin frontend"

Para un proyecto como una biblioteca de clases (DLL) en C#/.NET que no usa base de datos, no expone una API HTTP y no tiene interfaz web:

|Paso|¿Aplica?|Motivo|
|---|---|---|
|Paso 0 (rama)|Sí|Siempre obligatorio|
|Paso N (pruebas unitarias)|Sí|Siempre obligatorio|
|Paso N+1 — pruebas unitarias|Sí|Siempre obligatorio|
|Paso N+1 — verificación de BD|**No**|El proyecto no usa base de datos|
|Paso N+2 — curl|**No**|El proyecto no expone API HTTP|
|Paso N+3 — Playwright E2E|**No**|El proyecto no tiene interfaz web|
|Paso N+4 (documentación)|Sí|Siempre obligatorio|

En este caso, `tasks.md` solo debe contener: Paso 0, los pasos de implementación TDD, Paso N/N+1 (solo pruebas unitarias + reporte, sin sección de BD) y Paso N+4. No debe aparecer ningún paso de curl ni de Playwright, ni un paso de BD marcado como "N/A" — simplemente no se listan.
