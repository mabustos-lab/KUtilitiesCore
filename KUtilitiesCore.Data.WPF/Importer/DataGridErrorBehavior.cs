namespace KUtilitiesCore.Data.WPF.Importer;

using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

/// <summary>
/// Attached behavior que señaliza en el <see cref="DataGrid"/> de vista previa los
/// errores que el pipeline de validación pintó sobre el <see cref="DataView"/>
/// enlazado: borde y tooltip por celda, y tooltip a nivel de fila, leyéndolos
/// exclusivamente con <see cref="RowErrorDescriptor"/>.
/// </summary>
/// <remarks>
/// <para>
/// El pipeline pinta con <c>SetColumnError</c>/<c>RowError</c> sobre la tabla adoptada
/// (única fuente de verdad, decisión de diseño D5); este behavior solo proyecta esos
/// errores a estilos nativos de WPF sobre la celda afectada (borde rojo + tooltip),
/// sin reglas de validación por columna. Al revalidar, el pipeline limpia y repinta
/// los errores de la misma fila, de modo que la celda corregida pierde el borde y la
/// fila desaparece del filtro de errores.
/// </para>
/// <para>
/// <b>Refresco</b>: <c>LoadingRow</c> pinta cada fila al materializarse (incluidas las
/// que entran al aplicar el filtro de errores); <see cref="Refresh"/> repinta las
/// filas ya realizadas y el host la invoca cuando el ViewModel notifica que cambió la
/// exposición del veredicto. Solo se alcanzan las celdas realizadas (visibles); las
/// restantes se pintan al materializarse mediante <c>LoadingRow</c>.
/// </para>
/// </remarks>
/// <example>
/// Activación sobre el grid de vista previa y repinte tras cada revalidación
/// notificada por el ViewModel (desde XAML también puede declararse como propiedad
/// adjunta <c>DataGridErrorBehavior.Enabled</c>):
/// <code>
/// previewGrid.SetValue(DataGridErrorBehavior.EnabledProperty, true);
/// DataGridErrorBehavior.Refresh(previewGrid);
/// </code>
/// </example>
public static class DataGridErrorBehavior
{
    /// <summary>Identificador de la propiedad adjunta que activa el behavior sobre un grid.</summary>
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled",
        typeof(bool),
        typeof(DataGridErrorBehavior),
        new PropertyMetadata(false, OnEnabledChanged));

    /// <summary>Obtiene si el behavior está activo sobre el grid dado.</summary>
    /// <param name="element">Grid de vista previa enlazado al DataView con errores.</param>
    /// <returns><see langword="true"/> si el behavior está activo.</returns>
    public static bool GetEnabled(DataGrid element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (bool)element.GetValue(EnabledProperty);
    }

    /// <summary>Activa o desactiva la señalización de errores sobre el grid dado.</summary>
    /// <param name="element">Grid de vista previa enlazado al DataView con errores.</param>
    /// <param name="value"><see langword="true"/> para pintar borde y tooltip en las celdas con error.</param>
    public static void SetEnabled(DataGrid element, bool value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(EnabledProperty, value);
    }

    /// <summary>Repinta las filas realizadas del grid con los errores vigentes de la tabla enlazada.</summary>
    /// <param name="dataGrid">Grid cuya fuente es el DataView con los errores pintados por el pipeline.</param>
    /// <remarks>
    /// Pensado para invocarse tras cada revalidación: el pipeline pinta sobre las
    /// mismas instancias de fila sin disparar eventos de WPF, así que el host repinta
    /// al recibir la notificación del ViewModel. <c>LoadingRow</c> cubre las filas
    /// aún no realizadas.
    /// </remarks>
    public static void Refresh(DataGrid dataGrid)
    {
        ArgumentNullException.ThrowIfNull(dataGrid);
        foreach (var item in dataGrid.Items)
        {
            if (dataGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
            {
                ApplyRow(row);
            }
        }
    }

    private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid)
        {
            return;
        }

        if (Equals(e.NewValue, true))
        {
            dataGrid.LoadingRow += OnLoadingRow;
        }
        else
        {
            dataGrid.LoadingRow -= OnLoadingRow;
        }
    }

    private static void OnLoadingRow(object? sender, DataGridRowEventArgs e)
        => ApplyRow(e.Row);

    private static void ApplyRow(DataGridRow row)
    {
        if (ItemsControl.ItemsControlFromItemContainer(row) is not DataGrid dataGrid)
        {
            return;
        }

        ClearRowVisuals(dataGrid, row);
        if (row.Item is not DataRowView dataRowView)
        {
            return;
        }

        var descriptors = RowErrorDescriptor.Describe(dataRowView.Row);
        if (descriptors.Count == 0)
        {
            return;
        }

        foreach (var descriptor in descriptors)
        {
            if (descriptor.ColumnName.Length == 0)
            {
                row.ToolTip = descriptor.Message;
                continue;
            }

            var column = FindColumn(dataGrid, descriptor.ColumnName);
            if (column?.GetCellContent(row) is { } content && ResolveCell(content) is { } cell)
            {
                cell.BorderBrush = Brushes.Red;
                cell.BorderThickness = new Thickness(1);
                cell.ToolTip = descriptor.Message;
            }
        }
    }

    private static void ClearRowVisuals(DataGrid dataGrid, DataGridRow row)
    {
        row.ClearValue(FrameworkElement.ToolTipProperty);
        foreach (var column in dataGrid.Columns)
        {
            if (column.GetCellContent(row) is { } content && ResolveCell(content) is { } cell)
            {
                cell.ClearValue(Control.BorderBrushProperty);
                cell.ClearValue(Control.BorderThicknessProperty);
                cell.ClearValue(FrameworkElement.ToolTipProperty);
            }
        }
    }

    private static DataGridCell? ResolveCell(FrameworkElement content)
    {
        DependencyObject current = content;
        while (current is not null and not DataGridCell)
        {
            current = VisualTreeHelper.GetParent(current);
        }

        return current as DataGridCell;
    }

    private static DataGridColumn? FindColumn(DataGrid dataGrid, string columnName)
    {
        foreach (var column in dataGrid.Columns)
        {
            if (string.Equals(column.SortMemberPath, columnName, StringComparison.Ordinal))
            {
                return column;
            }

            if (column is DataGridBoundColumn boundColumn
                && boundColumn.Binding is Binding binding
                && string.Equals(binding.Path?.Path, columnName, StringComparison.Ordinal))
            {
                return column;
            }
        }

        return null;
    }
}
