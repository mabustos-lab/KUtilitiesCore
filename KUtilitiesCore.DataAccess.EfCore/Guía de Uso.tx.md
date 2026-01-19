# Guía de Uso: Inyección de Dependencias y Repositorios

Esta guía explica cómo integrar `KUtilitiesCore.DataAccess.EfCore` en tu aplicación .NET utilizando el contenedor de Inyección de Dependencias (DI) nativo.

## 1. Configuración Inicial (`Program.cs` / `Startup.cs`)

El primer paso es registrar tu `DbContext` y luego activar la infraestructura de KUtilities con una sola línea de código.

### Ejemplo en .NET 6+ (Minimal APIs)

```C#
using KUtilitiesCore.DataAccess.EfCore.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configura tu DbContext como siempre lo haces (SQL Server, PostgreSQL, etc.)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. ¡Activa KUtilities! 
// Esto registra automáticamente IUnitOfWork y IRepository<> vinculados a AppDbContext
builder.Services.AddKUtilitiesEfCore<AppDbContext>();

// ... resto de servicios
var app = builder.Build();
```

## 2. Escenarios de Uso

Una vez registrado, puedes inyectar las interfaces en tus Controladores o Servicios. Existen dos formas principales de usar la librería dependiendo de tu necesidad.

### Escenario A: Uso de `IUnitOfWork` (Recomendado para Escritura)

Usa el `IUnitOfWork` cuando necesites realizar operaciones transaccionales (guardar cambios en múltiples tablas al mismo tiempo) o cuando tu servicio maneje múltiples entidades.

**Ventaja:** Garantiza que `SaveChanges` se llame una sola vez al final, asegurando integridad de datos.

```C#
public class ProductService
{
    private readonly IUnitOfWork _uow;

    // Inyectamos IUnitOfWork. El contenedor nos da la instancia configurada con AppDbContext.
    public ProductService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task CreateProductWithCategoryAsync(string productName, string categoryName)
    {
        // 1. Obtenemos los repositorios necesarios bajo demanda
        var productRepo = _uow.Repository<Product>();
        var categoryRepo = _uow.Repository<Category>();

        // 2. Lógica de negocio
        var newCategory = new Category { Name = categoryName };
        categoryRepo.Add(newCategory); // Aún no se guarda en BD

        var newProduct = new Product 
        { 
            Name = productName, 
            Category = newCategory // EF Core maneja la relación
        };
        productRepo.Add(newProduct);   // Aún no se guarda en BD

        // 3. Confirmamos TODAS las operaciones en una sola transacción
        await _uow.SaveChangesAsync(); 
    }
}
```

### Escenario B: Uso Directo de `IRepository<T>` (Recomendado para Lectura)

Si tu servicio solo necesita leer datos de una entidad específica, puedes inyectar el repositorio directamente. Esto hace el código más limpio y directo.

```C#
public class ProductQueryService
{
    // Inyectamos directamente el repositorio de productos
    private readonly IRepository<Product> _productRepository;

    public ProductQueryService(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Product> GetProductDetailsAsync(int id)
    {
        // Usamos el método de extensión para buscar por ID
        // Internamente usa la Specification por defecto
        var product = await _productRepository.GetByIdAsync(p => p.Id == id);
        
        return product;
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        // Creamos una especificación al vuelo o usamos una clase definida
        var spec = new ActiveProductsSpecification();
        
        // Ejecutamos la consulta
        return await _productRepository.ListAsync(spec);
    }
}
```

## 3. Entendiendo la Magia (Specification Pattern)

Para sacar el máximo provecho, recuerda que ya no filtras en el servicio, sino que defines **Especificaciones**.

**Ejemplo de Especificación:**

```C#
public class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification() 
        : base(p => p.IsActive) // Criterio Base
    {
        AddInclude(p => p.Category); // Incluir categoría (JOIN)
        ApplyOrderBy(p => p.Name);   // Ordenar por nombre
        
        // Opcional: Optimizar para solo lectura
        // ApplyNoTracking(); 
    }
}
```

**Uso:**

```C#
var spec = new ActiveProductsSpecification();
var products = await repository.ListAsync(spec);
```

## Resumen de Beneficios

1. **Desacoplamiento:** Tu código de negocio (`ProductService`) no depende de Entity Framework, solo de `IRepository`.
    
2. **Consistencia:** Todas las consultas siguen las mismas reglas definidas en las Especificaciones.
    
3. **Simplicidad:** Configuración en una sola línea en `Program.cs`.