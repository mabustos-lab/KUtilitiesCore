Actúa como un Arquitecto de Software Senior en .NET y C#. Necesito realizar una refactorización profunda en la capa de acceso a datos (`KUtilitiesCore.Dal`) para unificar el manejo de `IDataReader`.



\*\*Contexto:\*\*



Actualmente, `ReaderResultSet` y `DataReaderConverter` compiten en responsabilidades. Quiero que `ReaderResultSet` sea el \*\*único\*\* orquestador que itera sobre los resultados de un `IDataReader` (soporte para múltiples `SELECT`), pero que delegue la transformación específica de cada resultado a una configuración previa.



\*\*Archivos a modificar:\*\*



1\. `KUtilitiesCore.Dal/Helpers/IReaderResultSet.cs` y `ReaderResultSet.cs`

&nbsp;   

2\. `KUtilitiesCore.Dal/Helpers/IDataReaderConverter.cs` y `DataReaderConverter.cs`

&nbsp;   

3\. `KUtilitiesCore.Dal/DaoContext.cs` (y su implementación de `IDaoContext`).

&nbsp;   



\*\*Requerimientos Técnicos de la Refactorización:\*\*



1\. \*\*Abstracción de Transformación:\*\*

&nbsp;   

&nbsp;   Crea una estructura interna (puede ser una interfaz privada o delegado) que represente una "Estrategia de Mapeo" para un `ResultSet` individual.

&nbsp;   

&nbsp;   - Debe haber una estrategia para `DataTable` (comportamiento actual).

&nbsp;       

&nbsp;   - Debe haber una estrategia para `List<T>` u `Object` (usando la lógica de `DataReaderConverter`).

&nbsp;       

2\. \*\*Refactorizar `ReaderResultSet`:\*\*

&nbsp;   

&nbsp;   - Debe mantener una `Queue` (Cola) de transformaciones esperadas.

&nbsp;       

&nbsp;   - El método `Load(IDataReader reader)` debe iterar usando `reader.NextResult()`.

&nbsp;       

&nbsp;   - En cada iteración, debe desencolar la siguiente estrategia configurada y ejecutarla.

&nbsp;       

&nbsp;   - Si no hay estrategia configurada, debe usar una estrategia por defecto (ej. ignorar o cargar como `DataTable` según configuración global `WithDefaultDataTable`).

&nbsp;       

&nbsp;   - Debe almacenar los resultados en una colección genérica (ej. `List<object>` o un diccionario) que luego se pueda consultar.

&nbsp;       

3\. \*\*Refactorizar `DataReaderConverter`:\*\*

&nbsp;   

&nbsp;   - Asegúrate de que sus métodos de extensión o utilitarios sean accesibles para ser invocados desde dentro de `ReaderResultSet` cuando la estrategia sea "Mapear a Objeto".

&nbsp;       

4\. \*\*Actualizar `DaoContext`:\*\*

&nbsp;   

&nbsp;   - Los métodos `WithResult<T>()` deben encolar una "Estrategia de Objeto <T>" en el `ReaderResultSet`.

&nbsp;       

&nbsp;   - El método `WithDefaultDataTable()` debe indicar que cualquier resultado no explícito se convierta a `DataTable`.

&nbsp;       

&nbsp;   - El método `Execute` debe instanciar el `ReaderResultSet`, pasarle el `IDataReader` real de la BD, y luego devolver el orquestador lleno de datos.

&nbsp;       



\*\*Objetivo del código resultante:\*\*



Quiero poder hacer esto en `DaoContext`:



C#



```C#

var result = \_daoContext.CreateCommand("sp\_GetData")

&nbsp;   .WithResult<UserDto>()      // El primer select es Users

&nbsp;   .WithResult<InvoiceDto>()   // El segundo select es Invoices

&nbsp;   .WithDefaultDataTable()     // Si hay un tercero, que sea DataTable

&nbsp;   .Execute();



var users = result.Get<UserDto>(); // Obtiene la lista del primer resultado

var table = result.GetTable(2);    // Obtiene el DataTable del tercer resultado

```

