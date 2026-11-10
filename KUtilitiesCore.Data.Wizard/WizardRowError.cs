namespace KUtilitiesCore.Data.Wizard;

/// <summary>
/// Proyección neutral de un error de validación por fila: transporta el índice de fila, el
/// nombre interno del campo, su nombre visible (resuelto desde las definiciones, sin mutarlas)
/// y el mensaje, sin tipos de ningún framework de interfaz de usuario.
/// </summary>
/// <remarks>
/// <para>
/// Es la contraparte sin UI del contenido del grid de errores del formulario WinForms: los
/// front-ends la consumen para pintar sus vistas de errores (el WinForms con su grid actual y
/// el WPF con su DataGrid de errores con DisplayName).
/// </para>
/// <para>
/// Los fallos generales (mensajes sin campo asociado) usan <see cref="RowIndex"/> = -1 y
/// <see cref="FieldDisplayName"/> = "General", paridad con la convención del formulario
/// original; los front-ends pueden renderizar "-" para índices negativos.
/// </para>
/// </remarks>
public sealed class WizardRowError
{
    /// <summary>Inicializa la proyección del error con sus datos resueltos.</summary>
    /// <param name="rowIndex">Índice de la fila con error; -1 para fallos generales sin fila.</param>
    /// <param name="fieldName">Nombre interno del campo (<c>FieldName</c>); vacío para fallos generales.</param>
    /// <param name="fieldDisplayName">Nombre visible resuelto desde las definiciones; "General" para fallos generales.</param>
    /// <param name="message">Mensaje de validación original.</param>
    public WizardRowError(int rowIndex, string fieldName, string fieldDisplayName, string message)
    {
        RowIndex = rowIndex;
        FieldName = fieldName;
        FieldDisplayName = fieldDisplayName;
        Message = message;
    }

    /// <summary>Índice de la fila con error; -1 cuando el fallo no está asociado a una fila.</summary>
    public int RowIndex { get; }

    /// <summary>Nombre interno del campo (<c>FieldName</c>); vacío para fallos generales.</summary>
    public string FieldName { get; }

    /// <summary>Nombre visible del campo resuelto desde las definiciones, sin mutarlas.</summary>
    public string FieldDisplayName { get; }

    /// <summary>Mensaje de validación original reportado por el gestor de importación.</summary>
    public string Message { get; }
}
