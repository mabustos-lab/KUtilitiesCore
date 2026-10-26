# KUtilitiesCore

KUtilitiesCore es un ecosistema de librerías C# diseñado para proporcionar funcionalidades transversales, patrones de diseño robustos y herramientas de utilidad para aplicaciones empresariales, con un fuerte enfoque en la desacoplación y la extensibilidad.

## 🚀 Resumen del Proyecto
El proyecto ofrece una suite de herramientas que abarcan desde el acceso a datos avanzado hasta la gestión de UI con MVVM, seguridad y logging. Su objetivo es reducir la redundancia de código en proyectos .NET proporcionando implementaciones estándar de patrones como **Unit of Work**, **Repository** y **Specification**.

## 🏗️ Arquitectura
El proyecto está dividido en módulos especializados para permitir un consumo selectivo:

### Capas y Módulos
- **KUtilitiesCore.DataAccess**: Define los contratos core (`IUnitOfWork`, `IRepository`, `ISpecification`). Es la capa de abstracción pura.
- **KUtilitiesCore.DataAccess.EfCore**: Implementación concreta de los contratos utilizando **Entity Framework Core**.
- **KUtilitiesCore.Dal**: Implementación de acceso a datos nativa (ADO.NET/SQL) con soporte para transacciones y inserciones masivas.
- **KUtilitiesCore.MVVM**: Framework ligero para el patrón Model-View-ViewModel, incluyendo un sistema avanzado de `RelayCommand` basado en expresiones.
- **KUtilitiesCore.Encryption**: Servicios de cifrado (AES, Base64, DPAPI) y hashing.
- **KUtilitiesCore.Logger**: Sistema de logging extensible con proveedores para SQL, Archivo y Consola.

### Diagrama Lógico de Dependencias
```text
[ App Consumidora ] 
       │
       ▼
[ KUtilitiesCore.MVVM ] ────► [ KUtilitiesCore ]
       │
       ▼
[ KUtilitiesCore.DataAccess ] ◄─── [ KUtilitiesCore.DataAccess.EfCore ]
       ▲                                 │
       └─────────────────────────────────┴─── [ KUtilitiesCore.Dal ]
```

## 🛠️ Instalación
### Prerrequisitos
- .NET 10 SDK (la solución fija la banda 10.x mediante `global.json`)
- Windows para los módulos WinForms (`KUtilitiesCore.Data.Win`) y para DPAPI (`KUtilitiesCore.Encryption`)

### Build
```bash
# Restaurar dependencias
dotnet restore KUtilitiesCore.sln

# Compilar la solución
dotnet build KUtilitiesCore.sln -c Release
```

## 📖 Uso Rápido

### Configuración de Acceso a Datos con EF Core
```csharp
// En Program.cs o Startup.cs
services.AddKUtilitiesEfCore<MyDbContext>();
```

### Uso de Unit of Work
```csharp
public class UserService {
    private readonly IUnitOfWork _uow;
    public UserService(IUnitOfWork uow) => _uow = uow;

    public async Task CreateUser(User user) {
        var repo = _uow.Repository<User>();
        repo.AddEntity(user);
        await _uow.SaveChangesAsync();
    }
}
```

## 📦 Cómo usar UnitOfWork

El sistema soporta múltiples implementaciones según la tecnología de persistencia:

### 1. EF Core (`EfUnitOfWork`)
Ideal para aplicaciones modernas que requieren un ORM potente.
- **Registro**: `services.AddKUtilitiesEfCore<TContext>()`.
- **Características**: Soporta `ISpecification` para filtros complejos y `AsNoTracking`.

### 2. SQL Nativo (`DaoUnitOfWork`)
Optimizado para alto rendimiento y control total sobre el SQL.
- **Implementación**: `KUtilitiesCore.Dal`.
- **Características**: Gestión de transacciones ADO.NET y soporte para `IRawRepository` para queries personalizadas.

## 🛠️ Cómo Extender
1. **Nuevo Repositorio**: Crea una clase que herede de `IRepository<T>` o `DaoRepository<T>`.
2. **Registro en DI**: Para EF Core, el registro es automático vía `AddKUtilitiesEfCore`. Para DAL, instancia `DaoUnitOfWork` pasando un `IDaoContext`.
3. **Publicar NuGet**: Sigue las instrucciones en `docs/NUGET.md`.

## 🧪 Testing
El proyecto incluye suites de pruebas unitarias exhaustivas.
```bash
dotnet test KUtilitiesCore.sln
```

## 🤝 Contribuir
- **Estándares**: Seguir las convenciones de C# y .NET.
- **Formateo**: Utilizar el archivo `.editorconfig` del proyecto.
- **PRs**: Incluir tests unitarios para cualquier nueva funcionalidad.

## 🔍 Hallazgos y Observaciones
- **Deuda Técnica**: Se detectaron algunas implementaciones de `DefaultDaoRepository` que lanzan `NotImplementedException`, indicando que se espera que el desarrollador implemente la lógica específica de la tabla.
- **Seguridad**: No se detectaron secretos en texto plano en el análisis inicial.
