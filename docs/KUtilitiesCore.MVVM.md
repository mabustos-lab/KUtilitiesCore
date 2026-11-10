---
proyecto: KUtilitiesCore.MVVM
tipo: biblioteca
namespace_raiz: KUtilitiesCore.MVVM
target_framework: net10.0
lenguaje: C# latest (C# 14 con el SDK 10.0.x fijado en global.json)
nullable: enable
paquete_nuget: KUtilitiesCore.MVVM (PackageId implícito; empaquetable, sin GeneratePackageOnBuild explícito)
ruta: KUtilitiesCore.MVVM/KUtilitiesCore.MVVM.csproj
---

<!-- Mantenido por agentes según AGENTS.md. Puede regenerarse por completo con el prompt de Wiki. -->

# KUtilitiesCore.MVVM

> Núcleo MVVM sin dependencia de UI: `ViewModelHelperBase`, `RelayCommand`, enlace de eventos a comandos y contratos de servicios de mensajes y documentos.
> Lo consume `KUtilitiesCore.Data.WPF`. Puntos de entrada: `ViewModelHelperBase`, `RelayCommand<TViewModel>` y `EventCommandBinderCollection<TViewModel>`.

## 1. Descripción general y propósito

- Namespace raíz: `KUtilitiesCore.MVVM`. Otros namespaces públicos: `.Command`, `.Command.Attribs`, `.Command.Binder`, `.Helpers`, `.MessageService`.
- Rol: base independiente del framework de UI. Referenciado por [KUtilitiesCore.Data.WPF](KUtilitiesCore.Data.WPF.md).
- Depende de [KUtilitiesCore](KUtilitiesCore.md) (`using KUtilitiesCore.Data` en el código).
- Patrones evidenciados:

| Patrón | Ejemplo |
|---|---|
| Command (`ICommand` con metadatos) | `Command/RelayCommandBase.cs`, `Command/IViewModelCommand.cs` |
| Template method | `ViewModelHelperBase.cs` (`OnLoaded`, `OnDestroy`, `OnClose`, `UpdateCommands` abstractos) |
| Binder desechable (suscripción/desuscripción de eventos) | `Command/Binder/EventCommandBinderCollection.cs` |
| Convención sobre configuración (`Can` + nombre del método, `On{Prop}Changed`) | `Command/RelayCommand.cs`, `ViewModelExtensions.cs` |
| Servicio de mensajes asíncrono (contrato) | `MessageService/ISupportMessageService.cs` |

```text
KUtilitiesCore.MVVM/
├── Command/            RelayCommand, contratos y Binder/ (EventCommandBinderCollection), Attribs/ (CanExecute)
├── Helpers/            PropertyChangingEventArgs, ViewModelSourceException
├── MessageService/     ISupportMessageService, MessageArgs, MessageIcon, MessageType
├── Messaging/          EXCLUIDA de la compilación (ver KUtilitiesCore.MVVM.Messaging)
└── *.cs                contratos ISupport*/IViewModel*, ViewModelHelperBase, ViewModelExtensions
```

## 2. Guía de inicio rápido

Sin registro DI ni claves de configuración.

**ViewModel con comando enlazado a un evento** (basado en `KUtilitiesCore.MVVMTests.EventCommandBinder`):

```csharp
using System;
using System.ComponentModel;
using KUtilitiesCore.MVVM;
using KUtilitiesCore.MVVM.Command.Binder;

public sealed class Boton
{
    public event EventHandler? Click;
    public void Pulsar() => Click?.Invoke(this, EventArgs.Empty);
}

public sealed class ContadorViewModel : ViewModelHelperBase
{
    private bool _habilitado;

    public int Conteo { get; private set; }

    public bool Habilitado
    {
        get => _habilitado;
        set => this.SetVMValue(ref _habilitado, value);
    }

    public void Incrementar() => Conteo++;
    public bool CanIncrementar() => Habilitado;   // convención "Can" + nombre del método

    public override void OnLoaded() { }
    public override void OnDestroy() { }
    protected override void UpdateCommands() { }
    protected override void OnClose(CancelEventArgs e) { }
}

public static class EjemploMvvm
{
    public static int Probar()
    {
        var vm = new ContadorViewModel { Habilitado = true };
        var boton = new Boton();
        using var binder = new EventCommandBinderCollection<ContadorViewModel>(vm);
        binder.BindCommand(boton, "Click", _ => { }, v => v.Incrementar());
        boton.Pulsar();
        return vm.Conteo;   // 1
    }
}
```

**Cancelar un cambio de propiedad** con `onPropertyChanging`:

```csharp
using KUtilitiesCore.MVVM;

public sealed class EdadViewModel : ViewModelHelperBase
{
    private int _edad;

    public int Edad
    {
        get => _edad;
        set => this.SetVMValue(ref _edad, value,
            onPropertyChanging: (_, args) => { if (args.NewValue is int n && n < 0) args.Cancel = true; });
    }

    public override void OnLoaded() { }
    public override void OnDestroy() { }
    protected override void UpdateCommands() { }
    protected override void OnClose(System.ComponentModel.CancelEventArgs e) { }
}
```

```mermaid
flowchart LR
    Mvvm["KUtilitiesCore.MVVM"]
    Core["KUtilitiesCore"]
    Wpf["KUtilitiesCore.Data.WPF"]
    Mvvm --> Core
    Wpf --> Mvvm
```

## 3. Referencia del API principal

Sin miembros `[Obsolete]` ni `[Experimental]`. Excluido de la compilación: `ActionResult/**`, `Messaging/**`, `Helpers/ExpressionsExtCore.cs` (`ActionResult` se compila desde [KUtilitiesCore](KUtilitiesCore.md); la mensajería vive en [KUtilitiesCore.MVVM.Messaging](KUtilitiesCore.MVVM.Messaging.md)).

### `KUtilitiesCore.MVVM`

| Tipo | Clase | Propósito |
|---|---|---|
| `OnPropertyChangingDelegate` | delegate | `(object sender, PropertyChangingEventArgs args)` |
| `ISupportCommands` | interface | Registro y actualización de comandos del ViewModel |
| `ISupportParameter` | interface | `object? Parameter` |
| `ISupportParentViewModel` | interface | `ParentViewModel` y evento `ParentViewModelChanged` |
| `IViewModelChanged` / `IViewModelChanging` | interface | `RaisePropertyChanged` / `RaisePropertyChanging` |
| `IViewModelHelper` | interface | Agrupa `IViewModelChanged` e `IViewModelChanging` |
| `IViewModelDataErrorInfo` | interface | `IDataErrorInfo` con `SetError`, `ClearErrors`, `HasValidationErrors` |
| `IViewModelDocumentContent` / `IViewModelDocumentOwner` | interface | Documento hospedado y su propietario (`Close`) |
| `ViewModelHelperBase` | abstract class | Base de ViewModels |
| `ViewModelExtensions` | static class | `SetVMValue`, `RaiseAllPropertiesChanged`, `Get/SetParentViewModel`, `Call_OnPropertyChanged/Changing` |

### Otros namespaces

| Tipo | Clase | Propósito |
|---|---|---|
| `IViewModelCommand` (`.Command`) | interface | `ICommand` con `CommandName` e `IsParametrizedCommand` |
| `RelayCommandBase` (`.Command`) | abstract class | Base de comandos; `RaiseCanExecuteChanged` |
| `RelayCommand<TViewModel>` / `RelayCommand<TViewModel,TParam>` (`.Command`) | class | Comandos creados con `Create(...)` desde expresiones |
| `CanExecuteAttribute` (`.Command.Attribs`) | sealed class | Nombre explícito del método CanExecute |
| `EventCommandBinderCollection<TViewModel>` (`.Command.Binder`) | sealed class | Enlaza eventos a comandos y los desuscribe en `Dispose` |
| `PropertyChangingEventArgs` (`.Helpers`) | class | `PropertyName`, `OldValue`, `NewValue`, `Cancel` |
| `ViewModelSourceException` (`.Helpers`) | class | El objeto no cumple el contrato MVVM |
| `ISupportMessageService`, `MessageArgs`, `MessageIcon`, `MessageType` (`.MessageService`) | interface / class / enum | Mensajes de UI |

`CommandBinderBase<T>`, `EventCommandBinder<T>` y `ManualCommandBinder<T,THandler>` son `internal`.

### Firmas clave

```csharp
public abstract class ViewModelHelperBase : IViewModelHelper, ISupportParameter, ISupportCommands,
    ISupportParentViewModel, IViewModelDocumentContent, IViewModelDataErrorInfo, INotifyPropertyChanged
{
    public ViewModelHelperBase();
    public ViewModelHelperBase(IViewModelDocumentOwner documentOwner, ISupportMessageService? messageService = null);
    public ISupportMessageService? MessageService { get; internal set; }
    public bool IsLoading { get; set; }
    public string Title { get; }
    public object? Parameter { get; set; }
    public abstract void OnLoaded();
    public abstract void OnDestroy();
    protected abstract void OnClose(CancelEventArgs e);
    protected abstract void UpdateCommands();
    public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "");
    public virtual void Update();
}

public static class ViewModelExtensions
{
    public static bool SetVMValue<TSource, TProperty>(this TSource source, ref TProperty? currentValue, TProperty? newValue,
        OnPropertyChangingDelegate? onPropertyChanging = null, Action? onPropertyChanged = null,
        [CallerMemberName] string propertyName = "");
}

public interface ISupportCommands
{
    void RegisterCommand<TCommand>(TCommand command) where TCommand : IViewModelCommand;
    void UpdateRegisteredCommands();
    RelayCommandBase? GetRegisteredCommand(string commandName);
    bool RemoveRegisteredCommand(string commandName);
}

public class RelayCommand<TViewModel> : RelayCommandBase where TViewModel : class
{
    public static RelayCommand<TViewModel> Create(TViewModel viewModel,
        Expression<Action<TViewModel>> executeExpression, string customCommandName = "");
}
public class RelayCommand<TViewModel, TParam> : RelayCommandBase where TViewModel : class
{
    public static RelayCommand<TViewModel, TParam> Create(TViewModel viewModel,
        Expression<Action<TViewModel, TParam?>> executeExpression,
        Expression<Func<TViewModel, TParam>> parameterPropertyExpression, string customCommandName = "");
}

public sealed class EventCommandBinderCollection<TViewModel> : IDisposable
{
    public EventCommandBinderCollection(TViewModel viewModel);
    public void BindCommand<T>(T targetObject, string eventName, Action<bool> targetStatus, IViewModelCommand command) where T : class;
    public void BindCommand<T>(T targetObject, string eventName, Action<bool> targetStatus,
        Expression<Action<TViewModel>> executeExpression, string customCommandName = "") where T : class;
    public void BindCommand<T, TParam>(T targetObject, string eventName, Action<bool> targetStatus,
        Expression<Action<TViewModel, TParam?>> executeExpression,
        Expression<Func<TViewModel, TParam>> parameterPropertyExpression, string customCommandName = "") where T : class;
    public void BindCommand<T, THandler>(T targetObject, Action<T, THandler> subscribe, Action<T, THandler> unsubscribe,
        Action<bool> targetStatus, IViewModelCommand command) where T : class;
    public void Dispose();
}

public interface ISupportMessageService
{
    ValueTask ShowInfoAsync(string message);
    ValueTask ShowWarningAsync(string message);
    ValueTask ShowSuccessAsync(string message);
    ValueTask ShowErrorAsync(string message);
    ValueTask ShowAsync(MessageArgs messageArgs);
    ValueTask<bool> ConfirmAsync(MessageArgs messageArgs);
}
// MessageIcon: None, Information, Warning, Error, Question. MessageType: Generic, Information, Warning, Error.
```

## 4. Dependencias y paquetes

| Paquete | Versión | Uso en este proyecto |
|---|---|---|
| (ninguno) | — | Sin `PackageReference` |

- Referencias a proyectos: [KUtilitiesCore](KUtilitiesCore.md).
- `FrameworkReference`: ninguna. Analizadores: ninguno configurado. Sin Central Package Management.
- Sin acoplamiento a WPF: `ICommand`, `INotifyPropertyChanged` e `IDataErrorInfo` provienen del BCL.

## 5. Reglas de arquitectura y restricciones

| Regla | Evidencia |
|---|---|
| `Nullable` = `enable`, `ImplicitUsings` = `enable`, `LangVersion` = `latest` | `KUtilitiesCore.MVVM/KUtilitiesCore.MVVM.csproj` |
| El CanExecute se resuelve por `[CanExecute("...")]` o por convención `Can{NombreMétodo}` | `Command/RelayCommand.cs` |
| Los comandos se crean con expresiones (`Expression<Action<TViewModel>>`), no con delegados | `Command/RelayCommand.cs` |
| Los binders concretos son `internal`; se usan solo a través de la colección `IDisposable` | `Command/Binder/*.cs` |
| Los binders capturan el `SynchronizationContext` | `Command/Binder/CommandBinderBase.cs` |
| `SetVMValue` exige que el objeto implemente `IViewModelChanged` o `IViewModelChanging`; si no, lanza `ViewModelSourceException` | `ViewModelExtensions.cs` |
| El servicio de mensajes es asíncrono con `ValueTask` | `MessageService/ISupportMessageService.cs` |
| Sin logging ni DI propios | `KUtilitiesCore.MVVM.csproj` (sin paquetes) |
| Carpetas excluidas de la compilación: `ActionResult/**`, `Messaging/**`, `Helpers/ExpressionsExtCore.cs` | `KUtilitiesCore.MVVM.csproj` |

**Qué NO hacer**

| Prohibición | Evidencia |
|---|---|
| No instanciar `EventCommandBinder<T>`, `ManualCommandBinder<T,THandler>` ni `CommandBinderBase<T>` (son `internal`) | `Command/Binder/*.cs` |
| No omitir `Dispose` de `EventCommandBinderCollection`: libera las suscripciones a eventos | `Command/Binder/EventCommandBinderCollection.cs`, `CommandBinderBase.cs` |
| No llamar `SetVMValue` desde un tipo que no implemente `IViewModelChanged`/`IViewModelChanging` | `ViewModelExtensions.cs` |
| No asignar `MessageService`, `CommandName` ni `IsParametrizedCommand` desde fuera (setters `internal`): usar el constructor con `ISupportMessageService` y `Create(...)` | `ViewModelHelperBase.cs`, `Command/RelayCommandBase.cs` |
| No añadir código en `Messaging/` ni `ActionResult/` de este proyecto: no se compila | `KUtilitiesCore.MVVM.csproj` |

**Puntos de extensión**

- Derivar de `ViewModelHelperBase` e implementar `OnLoaded`, `OnDestroy`, `OnClose`, `UpdateCommands`.
- Implementar `ISupportMessageService` e `IViewModelDocumentOwner` en la capa de UI y pasarlos al constructor de `ViewModelHelperBase`.
- Derivar de `RelayCommandBase` para comandos propios; usar `[CanExecute("Metodo")]` para nombres no convencionales.
- Definir métodos `On{Propiedad}Changing` / `On{Propiedad}Changed` y llamarlos con `Call_OnPropertyChanging` / `Call_OnPropertyChanged`.
