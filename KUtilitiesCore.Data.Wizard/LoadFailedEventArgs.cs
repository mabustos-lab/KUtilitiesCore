namespace KUtilitiesCore.Data.Wizard;

/// <summary>
/// Argumentos del evento <see cref="ImportWizardController.LoadFailed"/>.
/// </summary>
/// <remarks>
/// La capa expone el fallo de carga como evento neutral (sin servicio de mensajes): cada
/// front-end decide cómo presentarlo; el campo típicamente se muestra como detalle técnico.
/// </remarks>
public sealed class LoadFailedEventArgs : EventArgs
{
    /// <summary>
    /// Inicializa los argumentos con la excepción que provocó el fallo de carga.
    /// </summary>
    /// <param name="exception">Excepción original del proceso de lectura.</param>
    public LoadFailedEventArgs(Exception exception)
    {
        Exception = exception;
    }

    /// <summary>
    /// Excepción original que provocó el fallo de carga.
    /// </summary>
    public Exception Exception { get; }
}
