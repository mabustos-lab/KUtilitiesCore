# Arquitectura de KUtilitiesCore

## 📦 Requerimientos de Versión
Para asegurar la compatibilidad y funcionalidad de los patrones implementados, todas las librerías de KUtilitiesCore deben estar en la versión **1.6.8 o superior**.

| Librería | Versión Mínima | Propósito Principal |
| :--- | :--- | :--- |
| `KUtilitiesCore.DataAccess` | 1.6.8+ | Abstracciones Core (IUnitOfWork, IRepository) |
| `KUtilitiesCore.DataAccess.EfCore`| 1.6.8+ | Implementación basada en Entity Framework Core |
| `KUtilitiesCore.Dal` | 1.6.8+ | Implementación UOW para SQL Directo (ADO.NET) |
| `KUtilitiesCore.DataAccess.Http` | 1.6.8+ | Implementación UOW para APIs REST |
| `KUtilitiesCore.MVVM` | 1.6.8+ | Framework para ViewModels y Commands |
| `KUtilitiesCore.Logger` | 1.6.8+ | Servicio de Logging extensible |

## 📂 Mapa de Carpetas y Responsabilidades

| Carpeta/Proyecto | Responsabilidad |
| :--- | :--- |
| `KUtilitiesCore.DataAccess` | Definición de interfaces `IUnitOfWork`, `IRepository` y `ISpecification`. |
| `KUtilitiesCore.DataAccess.EfCore` | Implementación de UOW y Repositorios usando EF Core. |
| `KUtilitiesCore.Dal` | Implementación de UOW y Repositorios usando ADO.NET / SQL Nativo. |
| `KUtilitiesCore.DataAccess.Http` | Implementación de UOW y Repositorios para consumo de APIs. |
| `KUtilitiesCore.MVVM` | Infraestructura para ViewModels, Commands y Messaging. |
| `KUtiitiesCore.Encryption` | Lógica de cifrado y hashing. |
| `KUtilities.Logger` | Framework de logging con múltiples proveedores. |
| `KUtilitiesCore` | Utilidades generales, validaciones y extensiones de tipos. |

## 🔑 Interfaces Públicas y Implementaciones

### Acceso a Datos (Patrón Unit of Work)
- **`IUnitOfWork`**:
    - `EfUnitOfWork` $\rightarrow$ Persistencia vía EF Core.
    - `DaoUnitOfWork` $\rightarrow$ Persistencia vía SQL Directo (ADO.NET).
    - `ApiUnitOfWork` $\rightarrow$ Persistencia vía API REST (HTTP).
- **`IRepository<T>`**:
    - `EfRepository<T>` $\rightarrow$ Repositorio para EF Core.
    - `DaoRepository<T>` $\rightarrow$ Repositorio para SQL Directo.
    - `ApiRepository<T>` $\rightarrow$ Repositorio para API REST.
- **`ISpecification<T>`**:
    - `BaseSpecification<T>` $\rightarrow$ Define filtros y criterios de búsqueda desacoplados.

### MVVM
- **`RelayCommand<TViewModel, TParam>`**: Implementación basada en expresiones para enlace automático de métodos.
- **`IMessenger`**: Sistema de mensajería desacoplada para comunicación entre ViewModels.

## 🔄 Flujo de Datos (CRUD)

### Operación de Escritura (EF Core)
`App` $\rightarrow$ `IUnitOfWork.Repository<T>()` $\rightarrow$ `IRepository.AddEntity(entity)` $\rightarrow$ `IUnitOfWork.SaveChangesAsync()` $\rightarrow$ `DbContext.SaveChangesAsync()`.

### Operación de Lectura (SQL Nativo)
`App` $\rightarrow$ `IUnitOfWork.Repository<T>()` $\rightarrow$ `IRepository.GetEntities(spec)` $\rightarrow$ `DaoRepository.MapParameters()` $\rightarrow$ `IDaoContext.ExecuteQuery()`.

### Operación de Comunicación (API)
`App` $\rightarrow$ `IUnitOfWork.Repository<T>()` $\rightarrow$ `ApiRepository.GetEntities(spec)` $\rightarrow$ `HttpClient.GetAsync()` $\rightarrow$ `JSON Deserialization`.
