using System.Data;

namespace KUtilitiesCore.Data.DataExporter
{
    /// <summary>
    /// Preparación compartida del diálogo "Guardar exportación": validación del origen,
    /// nombre por defecto, filtro de archivos y resolución extensión→formato.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Las fachadas de UI (WinForms y WPF) delegan aquí toda decisión previa al diálogo
    /// nativo de cada plataforma, garantizando paridad de comportamiento: mismos orígenes
    /// rechazados, mismo nombre sugerido, mismo filtro y misma resolución de formato.
    /// </para>
    /// <para>
    /// Se ubica en la capa de datos (sin tipos de UI) porque es conocimiento del dominio
    /// de exportación, no de presentación: qué extensiones existen y a cuál exportador
    /// corresponden no depende del framework de interfaz.
    /// </para>
    /// </remarks>
    public static class ExportDialogPreparation
    {
        /// <summary>
        /// Filtro unificado que acepta todos los formatos soportados por los exportadores.
        /// </summary>
        /// <remarks>Constante compartida para que ambas plataformas ofrezcan exactamente las mismas opciones.</remarks>
        public const string FileDialogFilter =
            "Todos los archivos soportados|*.csv;*.tsv;*.psv;*.xlsx;*.xls|" +
            "Archivos de Texto (*.csv, *.tsv, *.psv)|*.csv;*.tsv;*.psv|" +
            "Archivos de Excel (*.xlsx)|*.xlsx|" +
            "Archivos de Excel 97-2003 (*.xls)|*.xls";

        /// <summary>
        /// Determina si el origen tiene contenido exportable.
        /// </summary>
        /// <param name="table">Tabla a exportar; puede ser <see langword="null"/>.</param>
        /// <returns><see langword="true"/> solo si la tabla existe y tiene filas.</returns>
        /// <remarks>
        /// Descartar orígenes vacíos ANTES de abrir el diálogo evita que el usuario
        /// navegue el sistema de archivos para terminar en un error de exportación.
        /// </remarks>
        public static bool ValidateSource(DataTable? table)
            => table is not null && table.Rows.Count > 0;

        /// <summary>
        /// Nombre de archivo sugerido por defecto para la exportación.
        /// </summary>
        /// <returns>Prefijo fijo más un sello de fecha/hora (<c>Exportacion_yyyyMMdd_HHmmss</c>).</returns>
        /// <remarks>
        /// El sello temporal permite al usuario reconocer la exportación por su momento
        /// de creación sin abrir el archivo.
        /// </remarks>
        public static string GetDefaultFileName()
            => $"Exportacion_{DateTime.Now:yyyyMMdd_HHmmss}";

        /// <summary>
        /// Resuelve el formato de exportación a partir de la extensión del archivo.
        /// </summary>
        /// <param name="fileName">Nombre o ruta completa del archivo.</param>
        /// <returns>
        /// <see cref="ExportFormat.Text"/> para .csv/.tsv/.psv,
        /// <see cref="ExportFormat.Excel"/> para .xlsx/.xls y
        /// <see cref="ExportFormat.None"/> para cualquier otra extensión.
        /// </returns>
        /// <remarks>
        /// No lanza excepciones por extensiones desconocidas: la fachada decide cómo
        /// informar al usuario, esta capa solo clasifica.
        /// </remarks>
        public static ExportFormat ResolveFormat(string fileName)
        {
            var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
            return extension switch
            {
                ".csv" or ".tsv" or ".psv" => ExportFormat.Text,
                ".xlsx" or ".xls" => ExportFormat.Excel,
                _ => ExportFormat.None
            };
        }

        /// <summary>
        /// Devuelve el separador de columnas que corresponde a la extensión del archivo de texto.
        /// </summary>
        /// <param name="fileName">Nombre o ruta completa del archivo.</param>
        /// <returns>Separador como <see cref="string"/> (",", "\t", "|").</returns>
        /// <exception cref="ArgumentException">Cuando <paramref name="fileName"/> está vacío.</exception>
        /// <exception cref="NotSupportedException">Cuando la extensión está fuera de csv/tsv/psv.</exception>
        public static string GetSeparator(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("El nombre del archivo no puede estar vacío.", nameof(fileName));
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".csv" => ",",
                ".tsv" => "\t",
                ".psv" => "|",
                _ => throw new NotSupportedException($"La extensión '{extension}' no está soportada.")
            };
        }
    }
}
