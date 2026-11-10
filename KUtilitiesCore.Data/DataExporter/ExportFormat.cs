namespace KUtilitiesCore.Data.DataExporter
{
    /// <summary>
    /// Formato de exportación que corresponde a un archivo según su extensión.
    /// </summary>
    public enum ExportFormat
    {
        /// <summary>La extensión no mapea a ningún exportador soportado.</summary>
        None = 0,

        /// <summary>Archivo de texto delimitado (CSV/TSV/PSV): exportador de texto con separador.</summary>
        Text = 1,

        /// <summary>Libro de Excel (XLSX/XLS 97-2003): exportador de Excel.</summary>
        Excel = 2
    }
}
