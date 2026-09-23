# Arquitectura de KUtilitiesCore

## 📂 Mapa de Carpetas y Responsabilidades

| Carpeta/Proyecto | Responsabilidad |
| :--- | :--- |
| `KUtilitiesCore.DataAccess` | Definición de interfaces `IUnitOfWork`, `IRepository` y `ISpecification`. |
| `KUtilitiesCore.DataAccess.EfCore` | Implementación de UOW y Repositorios usando EF Core. |
| `KUtilitiesCore.Dal` | Implementación de UOW y Repositorios usando ADO.NET / SQL Nativo. |
| `KUtilitiesCore.MVVM` | Infraestructura para ViewModels, Commands y Messaging. |
| `KUtiitiesCore.Encryption` | Lógica de cifrado y hashing. |
| `KUtilities.Logger` | Framework de logging con múltiples proveedores. |
| `KUtilitiesCore` | Utilidades generales, validaciones y extensiones de tipos. |

## 🔑 Interfaces Públicas y Implementaciones

### Acceso a Datos
- **`IUnitOfWork`**:
    - `EfUnitOfWork` (EF Core)
    - `DaoUnitOfWork` (SQL Nativo)
- **`IRepository<T>`**:
    - `EfRepository<T>` (EF Core)
    - `DaoRepository<T>` (SQL Nativo)
- **`ISpecification<T>`**:
    - `BaseSpecification<T>`

### MVVM
- **`RelayCommand<TViewModel, TParam>`**: Implementación basada en expresiones para enlace automático de métodos.
- **`IMessenger`**: Sistema de mensajería desacoplada para comunicación entre ViewModels.

## 🔄 Flujo de Datos (CRUD)

### Operación de Escritura (EF Core)
`App` $ightarrow$ `IUnitOfWork.Repository<T>()` $ightarrow$ `IRepository.AddEntity(entity)` $ightarrow$ `IUnitOfWork.SaveChangesAsync()` $ightarrow$ `DbContext.SaveChangesAsync()`.

### Operación de Lectura (SQL Nativo)
`App` $ightarrow$ `IUnitOfWork.Repository<T>()` $ightarrow$ `IRepository.GetEntities(spec)` $ightarrow$ `DaoRepository.MapParameters()` $ightarrow$ `IDaoContext.ExecuteQuery()`.
