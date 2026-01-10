using System;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter
{
    /// <summary>
    /// Opciones de configuración para lectura de Excel
    /// </summary>
    public class ExcelParsingOptions: ICloneable
    {
        /// <summary>
        /// Indica si la primera fila contiene encabezados (default: true)
        /// </summary>
        public bool HasHeader { get; set; } = true;

        /// <summary>
        /// Nombre de la hoja a procesar (default: primera hoja)
        /// </summary>
        public string SheetName { get; set; }

        /// <summary>
        /// Indica si trimear los valores de texto (default: true)
        /// </summary>
        public bool TrimValues { get; set; } = true;

        /// <summary>
        /// Tratar valores vacíos como null (default: true)
        /// </summary>
        public bool TreatEmptyAsNull { get; set; } = true;

        /// <summary>
        /// Formato de fecha para conversión (default: null = usar formato de celda)
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Fila inicial para leer (1-based, default: 1)
        /// </summary>
        public int StartRow { get; set; } = 1;

        /// <summary>
        /// Fila final para leer (default: todas)
        /// </summary>
        public int? EndRow { get; set; }

        /// <summary>
        /// Indica si ignorar filas completamente vacías (default: true)
        /// </summary>
        public bool IgnoreEmptyRows { get; set; } = true;

        /// <summary>
        /// Indica si lanzar excepción cuando no se encuentra la hoja (default: true)
        /// </summary>
        public bool ThrowOnMissingSheet { get; set; } = true;

        /// <summary>
        /// Crea una copia superficial de las opciones actuales
        /// </summary>
        /// <returns>Nueva instancia con los mismos valores</returns>
        public ExcelParsingOptions Clone()
        {
            return new ExcelParsingOptions
            {
                HasHeader = HasHeader,
                SheetName = SheetName,
                TrimValues = TrimValues,
                TreatEmptyAsNull = TreatEmptyAsNull,
                DateFormat = DateFormat,
                StartRow = StartRow,
                EndRow = EndRow,
                IgnoreEmptyRows = IgnoreEmptyRows,
                ThrowOnMissingSheet = ThrowOnMissingSheet
            };
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
