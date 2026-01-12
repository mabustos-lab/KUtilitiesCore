using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter
{
    /// <summary>
    /// Configuración para el parseo de archivos planos de texto
    /// </summary>
    public class TextFileParsingOptions: IParsingOptions
    {
        /// <summary>
        /// Separador de columnas (default: ",")
        /// </summary>
        public string Separator { get; set; } = ",";

        /// <summary>
        /// Indica si el CSV tiene encabezado (default: true)
        /// </summary>
        public bool HasHeader { get; set; } = true;

        /// <summary>
        /// Indica si se deben trimear los valores (default: true)
        /// </summary>
        public bool TrimValues { get; set; } = true;

        /// <summary>
        /// Encoding del archivo (default: UTF8)
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Comparador para nombres de columna (default: OrdinalIgnoreCase)
        /// </summary>
        public StringComparer ColumnNameComparer { get; set; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Carácter de escape para valores (default: null)
        /// </summary>
        public char? EscapeCharacter { get; set; }

        /// <summary>
        /// Indica si se deben ignorar filas vacías (default: true)
        /// </summary>
        public bool IgnoreEmptyLines { get; set; } = true;

        /// <summary>
        /// Crea una copia de las opciones actuales
        /// </summary>
        /// <returns>Nueva instancia con los mismos valores</returns>
        public TextFileParsingOptions Clone()
        {
            return new TextFileParsingOptions
            {
                Separator = Separator,
                HasHeader = HasHeader,
                TrimValues = TrimValues,
                Encoding = Encoding,
                ColumnNameComparer = ColumnNameComparer,
                EscapeCharacter = EscapeCharacter,
                IgnoreEmptyLines = IgnoreEmptyLines
            };
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
