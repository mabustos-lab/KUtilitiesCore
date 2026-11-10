# Propuesta: Asistente de importación ETL para WPF con capa de orquestación compartida

## Por qué

`KUtilitiesCore.Data.Win` es hoy la única fachada UI del pipeline ETL de `KUtilitiesCore.Data`: los consumidores que construyen aplicaciones WPF sobre .NET 10 no tienen asistente de importación y deben arrastrar una dependencia de Windows Forms o re-implementar la experiencia completa. Además, la lógica de orquestación del asistente vive acoplada a la vista (`ImportWizardForm`, ~900 líneas: estado, carga asíncrona, debounce, invalidación, reset) junto con la preparación del diálogo de exportación (`Exporter.cs`), de modo que un port por copia duplicaría esa lógica y abriría divergencia conductual entre front-ends (violación de DRY). Extraer la orquestación a una capa compartida y construir el front-end WPF sobre ella resuelve ambos problemas en un mismo movimiento y deja al asistente WinForms consumiendo la misma fuente de verdad.

## Qué cambia

### Capa compartida (DRY)

- Nuevo proyecto **`KUtilitiesCore.Data.Wizard`** (`net10.0`, agnóstico de UI, `Nullable enable`): orquestación del asistente — estado lógico (tabla cargada, carga en curso, resultado tipado), carga asíncrona no reentrante, revalidación silenciosa con debounce mediante **temporizador inyectable**, invalidación por cambio de opciones de análisis, reset al vaciar la ruta, construcción del mapeo activo sin mutar las definiciones del llamador, y exposición **neutral** del estado de errores por fila para que cada plataforma lo pinte con su mecanismo. Consume `KUtilitiesCore.Data` (pipeline ETL) sin re-implementarlo.
- **Exporter DRY**: la lógica compartida del diálogo de exportación (validación de tabla vacía, nombre de archivo por defecto, filtro de archivos, resolución extensión→formato) se extrae de `Exporter.cs`; quedan fachadas delgadas por plataforma — WinForms (`SaveFileDialog`) y WPF (`Microsoft.Win32.SaveFileDialog`).

### Front-end WPF

- Nuevo proyecto **`KUtilitiesCore.Data.WPF`** (`net10.0-windows`, `UseWPF`): asistente MVVM construido sobre `KUtilitiesCore.Data.Wizard`, con ViewModels de `KUtilitiesCore.MVVM` (RelayCommand, ViewModelHelperBase), controles de configuración CSV (TrimValues, IgnoreEmptyLines) y Excel (hoja seleccionada, rango de filas con fin opcional), vista previa con filtro de filas con error y presentación de errores por celda mediante mecanismos de validación de WPF (sin `SetColumnError`). **Mensajería enriquecida**: implementación de `ISupportMessageService` (MVVM) con un diálogo WPF propio de anatomía TaskDialog — encabezado destacado, pie de página y sección desplegable con los detalles técnicos del error — en lugar del limitado `MessageBox` de WPF. Estilo visual moderno con el look por defecto de WPF (sin theming/dark mode). **Sin dependencia de Windows Forms** (por eso el diálogo es propio: `Microsoft.Win32.TaskDialog` vive en `System.Windows.Forms.dll` y WPF no tiene equivalente nativo).
- Paridad funcional con el asistente WinForms: mismos requisitos observables del spec `import-wizard` (solo fuentes simples CSV/Excel; `MultiSourceImportManager` queda fuera de alcance).

### Refactor WinForms

- `ImportWizardForm` se refactoriza para **delegar la orquestación en la capa compartida**, conservando solo la interacción de vista (pintado de errores, hospedaje de controles de configuración, diálogos). La superficie API pública del formulario se conserva; `KUtilitiesCore.Data.WinTests` debe seguir en verde sin cambios de comportamiento observable.

### Libros

- Nuevo proyecto de pruebas **`KUtilitiesCore.Data.WPFTests`** (`net10.0-windows`) con tests de ViewModels/capa compartida.
- `AGENTS.md` actualizado (tabla de proyectos y notas de arquitectura, 19 → 22 `.csproj`).
- Deltas de `target-frameworks` y `build-hygiene` para los recuentos de proyectos.

## Capacidades

### Nuevas

- `import-wizard-core`: Orquestación compartida del asistente de importación, agnóstica de UI — contrato de estado y transiciones (carga, invalidación, reset), temporizador de debounce inyectable, exposición neutral de errores por fila, resultado tipado propio del consumidor y notificaciones dirigidas a las vistas.
- `data-export-dialog`: Preparación compartida del diálogo de exportación — validación de datos vacíos, nombre por defecto, filtro de archivos y resolución de formato por extensión — con fachadas delgadas por plataforma (WinForms y WPF).

### Modificadas

- `import-wizard`: Los requisitos del asistente pasan a regir **ambos front-ends** (WinForms y WPF) construidos sobre la capa compartida; se generaliza el wording atado a "formulario", se añade el requisito de que el front-end WPF no dependa de Windows Forms y el del servicio de mensajes enriquecido (implementación de `ISupportMessageService` con diálogo propio: encabezado, pie de página y detalles técnicos desplegables).
- `target-frameworks`: La matriz de TFMs se amplía de 19 a 22 `.csproj`: `KUtilitiesCore.Data.Wizard` (`net10.0`), `KUtilitiesCore.Data.WPF` y `KUtilitiesCore.Data.WPFTests` (`net10.0-windows`).
- `build-hygiene`: Los recuentos de proyectos cambian de 12 a 14 librerías y se incorpora el nuevo proyecto de pruebas a la exclusión explícita de la meta de 0 warnings.

## Impacto

- **Proyectos afectados**: nuevos `KUtilitiesCore.Data.Wizard`, `KUtilitiesCore.Data.WPF`, `KUtilitiesCore.Data.WPFTests`; modificados `KUtilitiesCore.Data.Win` (`ImportWizardForm`, `Exporter`); `KUtilitiesCore.sln` y `AGENTS.md`.
- **Contratos públicos**: sin cambios rompientes — `ImportWizardForm` conserva su superficie API (la migración a la capa compartida es interna); `Exporter` conserva su firma en `Data.Win`; la validación del pipeline (`ImportManager`) se reutiliza sin cambios (`single-source-import` no se modifica).
- **Nuevos paquetes NuGet**: `KUtilitiesCore.Data.Wizard` y `KUtilitiesCore.Data.WPF`.
- **Compatibilidad**: el comportamiento observable del asistente WinForms no cambia (red de seguridad: `KUtilitiesCore.Data.WinTests` en verde).
- **Riesgos**: abstracción del temporizador y del contrato de exposición de errores por fila (decisión crítica que cierra `design.md`); refactor de 900 líneas de `ImportWizardForm` mitigado por la suite existente; pruebas de UI WPF requieren STA — se mitiga testeando ViewModels sin hilo de UI y usando fakes para el temporizador.
