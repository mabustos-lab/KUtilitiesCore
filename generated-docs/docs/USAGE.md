# Guía de Uso Detallada

## 💉 Registro de Servicios (DI)

### Para EF Core
```csharp
// KUtilitiesCore.DataAccess.EfCore.Extensions
services.AddKUtilitiesEfCore<AppDbContext>();
```

### Para Acceso API (Http)
```csharp
// KUtilitiesCore.DataAccess.Http.Extensions
services.AddKUtilitiesApiCore(options => {
    options.BaseUrl = "https://api.miservicio.com/v1";
    options.ApiKey = "tu_api_key_aqui";
});
```

## 💾 Uso de UnitOfWork (UOW)

El patrón Unit of Work en KUtilitiesCore permite interactuar con diferentes fuentes de datos usando la misma interfaz `IUnitOfWork`.

### 1. Ejemplo con EF Core (`EfUnitOfWork`)
Ideal para aplicaciones que utilizan Entity Framework Core.
```csharp
public class ProductService {
    private readonly IUnitOfWork _uow;
    public ProductService(IUnitOfWork uow) => _uow = uow;

    public async Task UpdatePrice(int id, decimal newPrice) {
        // Uso de Specification para filtrar
        var spec = new ProductByIdSpec(id);
        var product = await _uow.Repository<Product>().GetFirstOrDefaultAsync(spec);
        
        if (product != null) {
            product.Price = newPrice;
            _uow.Repository<Product>().UpdateEntity(product);
            await _uow.SaveChangesAsync();
        }
    }
}
```

### 2. Ejemplo con SQL Nativo (`DaoUnitOfWork`)
Optimizado para alto rendimiento y control total mediante ADO.NET.
```csharp
// Instanciación manual o vía DI
var context = new SqlDaoContext(connectionString);
using (var uow = new DaoUnitOfWork(context)) {
    // El repositorio de SQL directo puede usar parámetros nativos vía Specifications
    var spec = new CustomerStatusSpec { Status = "Active", Region = "North" };
    var customers = uow.Repository<Customer>().GetEntities(spec);
    
    // Ejemplo de guardado transaccional
    uow.Repository<Customer>().AddEntity(newCustomer);
    uow.SaveChanges(); // Confirma la transacción SQL
}
```

### 3. Ejemplo con API REST (`ApiUnitOfWork`)
Permite tratar una API externa como si fuera un repositorio de datos local.
```csharp
public class ExternalDataService {
    private readonly IUnitOfWork _uow;
    public ExternalDataService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<UserDto>> GetRemoteUsers(string city) {
        // La especificación se traduce automáticamente a QueryParams en la URL
        var spec = new UserCitySpec(city);
        return await _uow.Repository<UserDto>().GetEntitiesAsync(spec);
    }

    public async Task SyncUser(UserDto user) {
        _uow.Repository<UserDto>().UpdateEntity(user);
        await _uow.SaveChangesAsync(); // Ejecuta el PUT/POST a la API
    }
}
```

## 🎨 Uso en App MVVM

Para aprovechar el sistema de comandos basado en expresiones de `KUtilitiesCore.MVVM`:

```csharp
public class MainViewModel : ViewModelBase, ISupportCommands {
    public ICommand SaveCommand { get; private set; }
    public User SelectedUser { get; set; }

    public MainViewModel() {
        // Crea un comando que:
        // 1. Ejecuta el método 'SaveData'
        // 2. Pasa automáticamente la propiedad 'SelectedUser' como parámetro
        // 3. Valida la ejecución basándose en el método 'CanSaveData' (convención)
        SaveCommand = RelayCommand<MainViewModel, User>.Create(
            this, 
            (vm, user) => vm.SaveData(user), 
            vm => vm.SelectedUser
        );
    }

    private void SaveData(User user) {
        // Lógica de persistencia
    }

    // Este método es detectado automáticamente por RelayCommand
    public bool CanSaveData(User user) {
        return user != null && !string.IsNullOrEmpty(user.Name);
    }
}
```

## 🔐 Seguridad y Cifrado (`KUtilitiesCore.Encryption`)

Ejemplos rápidos para cifrado de datos sensibles:

```csharp
// Uso de Factory para obtener el servicio deseado (AES, Base64, etc.)
IEncryptionService encryption = EncryptionServiceFactory.Create(EncryptionType.Aes);

string original = "Dato Sensible 123";
string encrypted = encryption.Encrypt(original);
string decrypted = encryption.Decrypt(encrypted);

// Hashing para contraseñas
IHashService hasher = HashServiceFactory.Create();
string passwordHash = hasher.ComputeHash("MiPasswordSeguro");
bool isValid = hasher.VerifyHash("MiPasswordSeguro", passwordHash);
```

## 📝 Logging (`KUtilitiesCore.Logger`)

Implementación de registro flexible:

```csharp
// Inyectar ILoggerServiceProvider
public class OrderProcessor {
    private readonly ILoggerService _logger;
    public OrderProcessor(ILoggerServiceProvider loggerProvider) {
        _logger = loggerProvider.GetLogger(LoggerType.Sql); // O LoggerType.File
    }

    public void Process(Order order) {
        try {
            _logger.Info($"Procesando orden {order.Id}");
            // ... lógica ...
        } catch (Exception ex) {
            _logger.Error("Error procesando orden", ex);
        }
    }
}
```
