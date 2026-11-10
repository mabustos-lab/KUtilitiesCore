# Design

## Context

Ver `proposal.md` (sección "Por qué") para la motivación. Estado actual que condiciona el diseño:

- `ImportWizardForm` (Data.Win, ~900 líneas) concentra hoy la orquestación: estado de carga (`_isLoading`), debounce de revalidación (`System.Windows.Forms.Timer`, 400 ms), invalidación por `OptionsChanged`, reset por ruta vacía, clones de definiciones (`BuildActiveDefinitions`), columna auxiliar `_HasError` + flags de error por fila, y guard de cero filas.
- `ImportManager` (Data) es el pipeline UI-agnostic reutilizable: adopta la tabla del llamador (`ReadData(DataTable)`), pinta errores con `SetColumnError` sobre esa tabla y sobre `DataSource`, expone `ValidationErrors` (`ValidationFailure` con `PropertyName=FieldName`, `IndexRow`), y `CreateResultTable()` genera la proyección tipada.
- `IDataSourceReader.ReadDataAsync()` existe y `LoadDataAsync` del formulario lo espera con `ConfigureAwait(true)` (vuelta al hilo de UI — afinidad de `DataTable`/`DataView`).
- `Exporter.ShowExportDataDialog` (Data.Win) mezcla lógica compartida (validación de vacío, nombre por defecto `Exportacion_{fecha}`, filtro, resolución extensión→formato) con el diálogo WinForms.
- La suite `KUtilitiesCore.Data.WinTests` (patrón `TestableImportWizardForm` + hooks `[EditorBrowsable(EditorBrowsableState.Never)]`, sin `InternalsVisibleTo`) es la red de seguridad del refactor.
- Contrato de control de configuración: `IImportConfigControl` (`Initialize(string fileName)`, `GetParsingOptions()`, `OptionsChanged`) hoy en Data.Win.

## Goals / Non-Goals

**Goals:**

- Orquestación del asistente en una sola capa (`KUtilitiesCore.Data.Wizard`, net10.0, sin dependencias de UI ni de MVVM), consumida por WinForms y WPF.
- Front-end WPF MVVM (`KUtilitiesCore.Data.WPF`) con paridad funcional con WinForms, sin dependencia de Windows Forms.
- Exportación DRY: preparación compartida + fachadas delgadas por plataforma.
- Refactor de `ImportWizardForm` conservando su superficie API pública y el verde de `Data.WinTests`.
- Testeabilidad determinista: temporizador inyectable con fakes; tests de capa/VM sin hilo de UI.

**Non-Goals:**

- Dark mode / theming: estilo por defecto de WPF (decisión del usuario).
- `MultiSourceImportManager` y multi-fuente: fuera de alcance (solo CSV/Excel simples).
- Cambios al pipeline `ImportManager` / spec `single-source-import`: se consume tal cual.
- UX nueva no cubierta por el spec `import-wizard` (paridad es el objetivo).
- `Directory.Build.props` / centralización de paquetes: cada `.csproj` sigue gestionando sus versiones.

## Decisions

### D1: Grafo de dependencias — capa compartida como proyecto propio

```
KUtilitiesCore.Data (pipeline ETL, net10.0)
        ▲
KUtilitiesCore.Data.Wizard (orquestación, net10.0)
        ▲                          ▲
KUtilitiesCore.Data.Win      KUtilitiesCore.Data.WPF (net10.0-windows, UseWPF)
(net10.0-windows)                 ▲ ref también Data + KUtilitiesCore.MVVM
```

- `Data.Wizard` referencia solo `KUtilitiesCore.Data`. `Data.Win` añade referencia a `Data.Wizard`. `Data.WPF` referencia `Data.Wizard` + `Data` + `KUtilitiesCore.MVVM`.
- **Alternativa rechazada 1**: orquestación dentro de `KUtilitiesCore.Data` — mezcla flujo de UI (debounce, estado de vista) con el pipeline de datos y rompe la cohesión del paquete ETL.
- **Alternativa rechazada 2**: compartición por fuente/`InternalsVisibleTo` — prohibido por norma del repo (`No InternalsVisibleTo`).

### D2: El orquestador es un componente de estado + eventos, no un ViewModel

`Data.Wizard` expone una clase orquestadora (p. ej. `ImportWizardController`) con estado observable (`FileName`, `LoadedDataView`, `IsLoading`, `IsDataStale`, `ResultData`, `CanImport`) y eventos .NET planos (`LoadCompleted`, `LoadFailed`, `ErrorsChanged`, `ImportCompleted`). WinForms consume el controlador directamente; en WPF un `ImportWizardViewModel` (con `ViewModelHelperBase`/`RelayCommand` de `KUtilitiesCore.MVVM`) lo envuelve y traduce eventos a `PropertyChanged`/`ObservableCollection`.

- **Razón**: no imponer MVVM a WinForms ni referenciar MVVM desde la capa (DIP: la capa no conoce al consumidor).
- **Alternativa rechazada**: controlador ViewModel base con `INotifyPropertyChanged` — acoplaría `Data.Wizard` a un patrón de presentación y a la librería MVVM.

### D3: Temporizador de debounce inyectable con afinidad de hilo preservada

- Abstracción en `Data.Wizard`: `IWizardDebounceTimer` (`Start(interval)`, `Stop()`, evento `Elapsed`). Adapters: `WinFormsDebounceTimer` (envuelve `System.Windows.Forms.Timer`, en Data.Win), `DispatcherDebounceTimer` (envuelve `DispatcherTimer`, en Data.WPF), `FakeDebounceTimer` en tests (disparo manual determinista).
- El intervalo por defecto (400 ms, valor vigente del asistente WinForms) vive en la capa como constante configurable.
- Carga asíncrona: el orquestador espera `reader.ReadDataAsync()` **sin** `ConfigureAwait(false)`, para que la continuación regrese al contexto del llamador (hilo de UI) — el contrato de afinidad de `DataTable`/`DataView` exige single-thread. Se documenta que todo el estado del orquestador se manipula desde el hilo del consumidor; no hay locks por diseño.
- **Alternativa rechazada**: `Task.Delay` + `CancellationToken` para el debounce — el disparo debe ocurrir en el hilo de UI (edición de `DataTable`); un timer de infraestructura por plataforma es el mecanismo natural y ya probado en WinForms.

### D4: Contrato de configuración compartido

`IImportConfigControl` (misma forma: `Initialize`, `GetParsingOptions`, `OptionsChanged`) se define en `Data.Wizard` en un namespace propio. El `IImportConfigControl` de Data.Win **hereda** del compartido (la interfaz WinForms queda compatible: las implementaciones existentes lo satisfacen sin cambios de firma), y el orquestador consume únicamente el contrato compartido para hospedar el panel de configuración según extensión de archivo.

- **Alternativa rechazada**: mover la interfaz WinForms a `Data.Wizard` y eliminar la de Data.Win — cambio rompiente de superficie pública; la herencia de interfaz logra DRY sin romper consumidores.

### D5: Fuente única de errores + proyección neutral para WPF

- El pintado de errores sigue siendo responsabilidad del pipeline (`ImportManager.SetColumnError` sobre la tabla adoptada y sobre `DataSource`): **una sola fuente de verdad**.
- La capa añade la proyección neutral: eventos/exposición con (índice de fila, `FieldName`/`DisplayName`, mensaje) derivada de `ValidationErrors`, y los helpers `_HasError` (`EnsureHasErrorColumn`, `UpdateRowErrorFlags`, filtro `[_HasError] = true`) migran del formulario al orquestador para que ambas vistas solo fijen `DataView.RowFilter`.
- WPF no llama a `SetColumnError`: un **comportamiento adjunto** (attached behavior) sobre el `DataGrid` lee `RowError`/`GetColumnError` de cada `DataRow` del `DataView` enlazado y aplica estilos de validación (borde de error + tooltip con el mensaje).
- **Alternativa rechazada**: re-pintar errores con `ValidationRule` por columna en WPF — duplicaría la fuente de verdad del pipeline y desincronizaría la vista previa.

### D6: Mensajería por front-end; servicio de mensajes enriquecido en WPF, eventos neutrales en la capa

Los fallos de carga y el resultado de la importación se notifican como eventos neutrales (`LoadFailedEventArgs`, `ImportCompletedEventArgs`). La capa no referencia `MessageService` de MVVM (sería una dependencia de presentación) ni conoce diálogos. Cada front-end decide cómo presentar los mensajes:

- **WinForms**: mapea a `MessageBox` + `MessageBoxIcon` (comportamiento vigente, sin cambios).
- **WPF**: implementa `ISupportMessageService` de `KUtilitiesCore.MVVM` (ya referenciado por D1) con un **diálogo WPF propio de anatomía TaskDialog** (p. ej. `MessageDialogWindow`): `Caption` → título de la ventana, `Heading` → encabezado de fuente grande, `Message` → texto principal, `Footnote` → pie de página, `Expander` → sección desplegable con los detalles técnicos del error, `MessageIcon` → icono. `MessageArgs` ya modela esos campos 1:1 con esa semántica (sus XML docs describen footnote y expander desplegable), por lo que el contrato de MVVM encaja sin cambios. Los métodos del servicio devuelven `ValueTask`; el diálogo es modal bloqueante (`ShowDialog`) y la implementación completa el `ValueTask` de forma síncrona (contrato documentado en XML docs). El `ImportWizardViewModel` consume el servicio para notificar fallos de carga (con el detalle técnico en el expander) y el resultado de la importación.

- **Alternativa rechazada 1**: usar `Microsoft.Win32.TaskDialog` (`TaskDialogPage`, `TaskDialogButtonCollection`, `TaskDialogFootnote`, `TaskDialogExpander`) — esos tipos viven en `System.Windows.Forms.dll` y exigen `UseWindowsForms`; violaría el requisito "Front-end WPF sin dependencia de Windows Forms". WPF no tiene TaskDialog nativo: solo `System.Windows.MessageBox`, que carece de encabezado, pie de página, expander y botones personalizados.
- **Alternativa rechazada 2**: P/Invoke directo a `TaskDialogIndirect` (comctl32 v6) sin WinForms — mantendría el requisito literal, pero a costa de structs `TASKDIALOGCONFIG`, callbacks y marshalado manual: interop pesado, frágil y sin testeabilidad frente a un diálogo XAML con lógica extraída.

### D7: Exportación DRY — preparación en `KUtilitiesCore.Data.DataExporter`

La lógica compartida sale de `Exporter.cs` a una clase pública de preparación (p. ej. `ExportDialogPreparation`) en `KUtilitiesCore.Data.DataExporter`: `ValidateSource(DataTable)` (nulo/cero filas), `GetDefaultFileName()` (patrón `Exportacion_{yyyyMMdd_HHmmss}`), `FileDialogFilter` (constante con los formatos vigentes) y `ResolveFormat(path)` (extensión→formato). Quedan fachadas delgadas: `Exporter` WinForms (`SaveFileDialog`) con su firma pública intacta, y un `Exporter` WPF (`Microsoft.Win32.SaveFileDialog`) delegando en la misma preparación y en los exportadores existentes.

- **Alternativa rechazada**: preparación en `Data.Wizard` — la exportación no es orquestación del asistente; `Data` ya es la casa de los exportadores.

### D8: Pruebas — un solo proyecto nuevo (`KUtilitiesCore.Data.WPFTests`)

Los tests del orquestador y de los ViewModels viven en `KUtilitiesCore.Data.WPFTests` (net10.0-windows, MSTest 4.4.1 + `EnableMSTestRunner`, como el resto). Un proyecto net10.0-windows consume sin problema la capa net10.0; con `FakeDebounceTimer` y eventos del orquestador, los tests de capa/VM no requieren hilo de UI ni STA. Para eventuales pruebas de la ventana WPF se usaría `[STATestMethod]` + subclase `TestableImportWizardWindow` (patrón de `Data.WinTests`), sin `InternalsVisibleTo`.

- **Alternativa rechazada 1**: `KUtilitiesCore.Data.WizardTests` (net10.0) propio — llevaría la matriz a 23 `.csproj`, rompiendo el delta `target-frameworks` acordado (22).
- **Alternativa rechazada 2**: tests de la capa en `DataTests` — acoplaría la cobertura de una librería nueva a un proyecto existente y ocultaría el costo de QA del cambio.

### D9: Refactor WinForms incremental con el orquestador

`ImportWizardForm` conserva su superficie pública (`ctor(FieldDefinitionCollection, ImportManager?)`, `FileName`, `ResultData`, `AutoCloseOnSuccess`, `LoadedDataTable`, `LoadDataAsync`) y delega estado/transiciones al orquestador; conserva solo vista: hospedaje del panel de configuración, pintado, diálogos de archivo y `MessageBox`. Los hooks de prueba existentes (`TestableImportWizardForm`) deben seguir compilando; `Data.WinTests` sin cambios de comportamiento es el gate de cada fase.

## Risks / Trade-offs

- [Regresión del asistente WinForms al extraer 900 líneas] → Refactor por fases con `dotnet test KUtilitiesCore.Data.WinTests` como gate tras cada una; escenarios del spec `import-wizard` como criterio de paridad.
- [Semántica de "commit" de edición distinta en `DataGrid` WPF (commit por foco/fila, no por celda)] → Forzar `CommitEdit`/`EndCurrentEdit` equivalente antes de cada revalidación, igual que `CommitPendingGridEdit` hace en WinForms; escenario "Corrección de la celda en la vista filtrada" lo cubre.
- [`SetColumnError` no renderiza nativo en `DataGrid` WPF] → Comportamiento adjunto (D5) con respaldo en la proyección neutral de errores; si el comportamiento resulta frágil, la vista puede pintar solo desde la proyección sin tocar el pipeline.
- [Colisión de nombres `IImportConfigControl` en dos namespaces] → XML docs en español aclarando el contrato compartido y la herencia WinForms; el orquestador solo conoce el compartido.
- [STA/Dispatcher en tests WPF] → Tests a nivel orquestador/VM con fakes (sin UI); `[STATestMethod]` solo si se testea la ventana directamente.
- [Costo de construir un diálogo de mensajes propio en WPF] → Anatomía acotada (ventana + plantilla: heading, texto, footnote, expander, botones); el mapeo `MessageArgs` → modelo de diálogo se extrae como función pura testeable y la ventana queda delgada; paridad de contenido con `MessageArgs` existente, sin extensiones del contrato MVVM.
- [Crecimiento de tiempos de build/test (+3 proyectos)] → Matriz acordada 22 `.csproj`; las librerías nuevas son pequeñas y los tests dirigidos por proyecto mantienen el ciclo corto.

## Migration Plan

1. Rama `feature/wpf-import-wizard` (paso 0 de `tasks.md`).
2. Fase A (TDD): contratos + orquestador en `Data.Wizard` con `FakeDebounceTimer`; proyecto de tests `Data.WPFTests` nace aquí.
3. Fase B: refactor de `ImportWizardForm` (y migración de helpers `_HasError`) sobre el orquestador; gate: `Data.WinTests` en verde sin cambios.
4. Fase C: extracción de la preparación de exportación en `Data.DataExporter` + fachada WPF; `Exporter` WinForms reducido a fachada.
5. Fase D: `Data.WPF` — XAML, ViewModels, controles de configuración CSV/Excel, comportamiento adjunto de errores, ventana del asistente y servicio de mensajes (`ISupportMessageService` con diálogo propio de anatomía TaskDialog).
6. Fase E: suite completa (`dotnet test --solution KUtilitiesCore.sln`), `AGENTS.md`, reportes de verificación, y al archivar: sincronizar `Purpose` de `build-hygiene` ("las 12 librerías" → 14) y de `target-frameworks` en los specs principales (los deltas no pueden modificar `Purpose`).
- **Rollback**: todo vive en la rama feature; revertir la rama restaura el estado previo. Ningún paquete existente cambia su contrato público.

## Open Questions

- ¿La ventana WPF será modal (`ShowDialog`) con `AutoCloseOnSuccess` como en WinForms? — Se asumirá modal por paridad; decidible en implementación sin tocar specs.
- ¿Diseño XAML definitivo (layout, agrupación de pasos)? — Menor; estilo por defecto de WPF ya decidido.
- ¿README/packaging de los paquetes nuevos (`PackageReadmeFile`)? — Se seguirá el patrón `Data.csproj`; ajustable al archivar.
