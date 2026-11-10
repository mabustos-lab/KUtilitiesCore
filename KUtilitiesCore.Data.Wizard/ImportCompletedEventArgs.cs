namespace KUtilitiesCore.Data.Wizard;

/// <summary>
/// Argumentos del evento neutral de importación terminada: expone el veredicto de la validación
/// y el conteo de errores sin involucrar servicios de mensajes ni tipos de UI.
/// </summary>
/// <remarks>
/// Los front-ends traducen estos datos a su presentación: el WinForms mantiene sus
/// <c>MessageBox</c> y el WPF enruta el mensaje a su servicio de diálogos propio (capa de
/// mensajería rica), decidiendo el texto según <see cref="IsValid"/> y <see cref="ErrorCount"/>.
/// </remarks>
public sealed class ImportCompletedEventArgs : EventArgs
{
    /// <summary>Inicializa los argumentos con el veredicto de validación y el número de errores.</summary>
    /// <param name="isValid">Indica si todos los datos validaron correctamente.</param>
    /// <param name="errorCount">Número de errores de validación detectados; cero cuando es válido.</param>
    public ImportCompletedEventArgs(bool isValid, int errorCount)
    {
        IsValid = isValid;
        ErrorCount = errorCount;
    }

    /// <summary>Indica si la importación terminó con datos válidos y resultado tipado disponible.</summary>
    public bool IsValid { get; }

    /// <summary>Número de errores de validación detectados en la última ejecución.</summary>
    public int ErrorCount { get; }
}
