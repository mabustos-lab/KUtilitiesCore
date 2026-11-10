namespace KUtilitiesCore.Data.WPF.Importer;

/// <summary>
/// Par (columna, mensaje) extraído de los errores que el pipeline de validación
/// pintó sobre una fila de datos; <see cref="ColumnName"/> vacío designa un error
/// a nivel de fila (<c>DataRow.RowError</c>).
/// </summary>
/// <remarks>
/// Tipo de solo lectura: el pipeline (<c>ImportManager</c>) es la única fuente de
/// verdad porque pinta los errores directamente sobre la tabla adoptada con
/// <c>SetColumnError</c>/<c>RowError</c>; los descriptores se limitan a proyectar
/// esa información para que la vista la señalice sin duplicar lógica de validación.
/// </remarks>
public sealed class CellErrorDescriptor
{
    /// <summary>Inicializa el descriptor con la columna afectada y el mensaje pintado.</summary>
    /// <param name="columnName">Columna afectada; vacío cuando el error es a nivel de fila.</param>
    /// <param name="message">Mensaje de error pintado por el pipeline de validación.</param>
    public CellErrorDescriptor(string columnName, string message)
    {
        ColumnName = columnName;
        Message = message;
    }

    /// <summary>Columna afectada; vacío designa el error a nivel de fila (<c>RowError</c>).</summary>
    public string ColumnName { get; }

    /// <summary>Mensaje de error pintado por el pipeline de validación.</summary>
    public string Message { get; }
}
