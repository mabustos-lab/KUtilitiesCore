# Guía de Uso Detallada

## 💉 Registro de Servicios (DI)

### Para EF Core
```csharp
// KUtilitiesCore.DataAccess.EfCore.Extensions
services.AddKUtilitiesEfCore<AppDbContext>();
```

## 💾 Uso de UnitOfWork

### Ejemplo con EF Core
```csharp
// Inyectar IUnitOfWork
public class ProductService {
    private readonly IUnitOfWork _uow;
    public ProductService(IUnitOfWork uow) => _uow = uow;

    public async Task UpdatePrice(int id, decimal newPrice) {
        var product = await _uow.Repository<Product>().GetFirstOrDefaultAsync(new ProductSpec(id));
        product.Price = newPrice;
        _uow.Repository<Product>().UpdateEntity(product);
        await _uow.SaveChangesAsync();
    }
}
```

### Ejemplo con SQL Nativo (DAL)
```csharp
// Instanciación manual o vía DI
var context = new SqlDaoContext(connectionString);
using (var uow = new DaoUnitOfWork(context)) {
    var repo = uow.Repository<Customer>();
    var customers = repo.GetEntities(new CustomerSpec { Status = "Active" });
    uow.SaveChanges();
}
```

## 🎨 Uso en App MVVM
Para usar los comandos avanzados de `KUtilitiesCore.MVVM`:

```csharp
public class MainViewModel : ViewModelBase, ISupportCommands {
    public ICommand SaveCommand { get; private set; }

    public MainViewModel() {
        // Crea un comando que ejecuta el método 'SaveData' pasando la propiedad 'SelectedUser'
        SaveCommand = RelayCommand<MainViewModel, User>.Create(
            this, 
            (vm, user) => vm.SaveData(user), 
            vm => vm.SelectedUser
        );
    }

    private void SaveData(User user) {
        // Lógica de guardado
    }
}
```
