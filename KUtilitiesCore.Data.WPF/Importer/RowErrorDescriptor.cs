namespace KUtilitiesCore.Data.WPF.Importer;

using System.Data;

/// <summary>
/// Lógica pura y testeable de señalización: proyecta los errores que el pipeline de
/// validación pintó sobre una <see cref="DataRow"/> en descriptores neutrales de
/// (columna, mensaje) que la vista consume para aplicar estilos de error.
/// </summary>
/// <remarks>
/// <para>
/// El pipeline (<c>ImportManager.ValidateDataTypes</c>) es la única fuente de verdad:
/// pinta los errores directamente sobre la tabla adoptada del asistente con
/// <c>SetColumnError</c> y <c>RowError</c>. Este descriptor solo <b>lee</b>
/// (<c>GetColumnError</c>/<c>RowError</c>), nunca muta la fila; así, corregir una
/// celda y revalidar desmarca la fila sin lógica duplicada de validación.
/// </para>
/// <para>
/// El error a nivel de fila se expone como descriptor final con
/// <see cref="CellErrorDescriptor.ColumnName"/> vacío; el resto de descriptores
/// corresponde a columnas concretas de la tabla.
/// </para>
/// </remarks>
public static class RowErrorDescriptor
{
    /// <summary>Extrae los errores de columna y de fila que el pipeline pintó sobre la fila dada.</summary>
    /// <param name="row">Fila adoptada por el asistente, ya pintada por el pipeline de validación.</param>
    /// <returns>
    /// Un descriptor por columna con error (<c>GetColumnError</c>) y, de existir, uno
    /// final con <c>ColumnName</c> vacío para el error a nivel de fila
    /// (<c>RowError</c>); colección vacía cuando la fila no tiene errores.
    /// </returns>
    public static IReadOnlyList<CellErrorDescriptor> Describe(DataRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        if (!row.HasErrors)
        {
            return Array.Empty<CellErrorDescriptor>();
        }

        var descriptors = new List<CellErrorDescriptor>();
        foreach (DataColumn column in row.Table.Columns)
        {
            var message = row.GetColumnError(column);
            if (!string.IsNullOrEmpty(message))
            {
                descriptors.Add(new CellErrorDescriptor(column.ColumnName, message));
            }
        }

        var rowError = row.RowError;
        if (!string.IsNullOrEmpty(rowError))
        {
            descriptors.Add(new CellErrorDescriptor(string.Empty, rowError));
        }

        return descriptors;
    }
}
