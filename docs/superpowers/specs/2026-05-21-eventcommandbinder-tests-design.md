# EventCommandBinderCollection — Proyecto de Pruebas Unitarias

**Fecha:** 2026-05-21
**Estado:** Aprobado

## Objetivo

Crear un proyecto de pruebas unitarias con xUnit para validar `EventCommandBinderCollection<TViewModel>`, usando objetos falsos simples con eventos `EventHandler` en lugar de controles WinForms reales.

## Estructura del Proyecto

```
KUtilitiesCore.MVVMTests.EventCommandBinder/
├── KUtilitiesCore.MVVMTests.EventCommandBinder.csproj
├── Usings.cs
├── Fakes/
│   └── FakeButton.cs
├── Models/
│   └── TestViewModel.cs
└── EventCommandBinderCollectionTests.cs
```

- **Target:** `net8.0` (no requiere WinForms, el binder es genérico)
- **Framework:** xUnit (primer proyecto xUnit en la solución)
- **Referencia:** `KUtilitiesCore.MVVM`

## Componentes

### FakeButton

Objeto simple con evento `EventHandler Click` y método `SimulateClick()` para dispararlo programáticamente. Simula el comportamiento de un botón sin depender de `System.Windows.Forms`.

### TestViewModel

Hereda de `ViewModelHelperBase`. Expone:
- `ExecuteAction()` / `CanExecuteAction()` — comando principal
- `ExecuteSecondary()` — comando secundario
- `IsToggleEnabled` — propiedad booleana que controla CanExecute
- `ToggleState()` — alterna `IsToggleEnabled`

Los métodos `Can*` se descubren por convención (reflexión en `RelayCommand`).

## Escenarios de Test

| # | Test | Validación |
|---|------|------------|
| 1 | Click → ejecuta comando | `ExecuteCount` incrementa |
| 2 | `CanExecute = false` → no ejecuta | `ExecuteCount` no cambia |
| 3 | Múltiples botones → distintos comandos | Cada contador independiente |
| 4 | Toggle `IsToggleEnabled` → refresca `CanExecuteChanged` | Estado se refleja en `targetStatus` |
| 5 | `Dispose()` → eventos desuscritos | Click post-dispose no ejecuta |
| 6 | ViewModel null → `ArgumentNullException` | Excepción en constructor |
| 7 | `BindCommand<T, TParam>` con parámetro | Parámetro del ViewModel usado |
| 8 | `BindCommand` con `commandName` | Comando registrado correctamente |

## Convenciones

- Comentarios explicativos en todo el código
- xUnit: `[Fact]` para tests, `Assert` para validaciones
- `IClassFixture` o constructor/dispose para setup/cleanup