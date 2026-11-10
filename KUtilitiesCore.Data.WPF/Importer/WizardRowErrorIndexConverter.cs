using KUtilitiesCore.Data.Wizard;
using System.Globalization;
using System.Windows.Data;

namespace KUtilitiesCore.Data.WPF.Importer;

/// <summary>
/// Convierte el índice de fila de un <see cref="WizardRowError"/> a su texto de presentación
/// en el grid de errores: los índices negativos (fallos generales, sin fila asociada) se
/// muestran como "-".
/// </summary>
/// <remarks>
/// Paridad con el grid de errores del formulario WinForms, que muestra "-" cuando el fallo
/// no tiene fila asociada; el contrato de <see cref="WizardRowError"/> documenta que cada
/// front-end decide cómo presentar los índices negativos.
/// </remarks>
public sealed class WizardRowErrorIndexConverter : IValueConverter
{
    /// <summary>Texto mostrado cuando el fallo no está asociado a una fila (índice negativo).</summary>
    public const string NoRowIndexText = "-";

    /// <summary>Traduce el índice numérico a texto; los índices negativos devuelven "-".</summary>
    /// <param name="value">Índice de fila (<see cref="int"/>) proveniente de <see cref="WizardRowError.RowIndex"/>.</param>
    /// <param name="targetType">Tipo del destino del enlace; no utilizado (se asume texto).</param>
    /// <param name="parameter">Parámetro del enlace; no utilizado.</param>
    /// <param name="culture">Cultura para dar formato al índice; si es <see langword="null"/> se usa la cultura actual.</param>
    /// <returns>El índice como texto, o "-" si es negativo o no numérico.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        => value is int index && index >= 0
            ? index.ToString(culture ?? CultureInfo.CurrentCulture)
            : NoRowIndexText;

    /// <summary>Conversión inversa no usada: el enlace del grid de errores es unidireccional.</summary>
    /// <returns><see cref="Binding.DoNothing"/> siempre.</returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        => Binding.DoNothing;
}
