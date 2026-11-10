namespace KUtilitiesCore.Data.WPF.Importer
{
    /// <summary>
    /// Opción de separador de columnas para archivos de texto delimitados: combina una
    /// etiqueta legible (para mostrar al usuario) con el valor real que usa el lector al
    /// parsear el archivo. Se mantiene como clase inmutable (no record) para que el combo
    /// de la interfaz compare por referencia las instancias estáticas ofrecidas por defecto.
    /// </summary>
    public sealed class DelimiterOption
    {
        /// <summary>
        /// Inicializa una opción de separador con su etiqueta legible y su valor real.
        /// </summary>
        /// <param name="label">Etiqueta legible para mostrar al usuario.</param>
        /// <param name="value">Valor del separador usado por el lector de texto.</param>
        public DelimiterOption(string label, string value)
        {
            Label = label;
            Value = value;
        }

        /// <summary>
        /// Etiqueta legible del separador para mostrar en la interfaz.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Valor real del separador usado al parsear el archivo.
        /// </summary>
        public string Value { get; }
    }
}
