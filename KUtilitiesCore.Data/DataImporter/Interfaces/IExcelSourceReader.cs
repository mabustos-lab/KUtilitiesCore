using System;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter.Interfaces
{
    /// <summary>
    /// Interfaz extendida para lectores de Excel que requieren configuración de hoja.
    /// </summary>
    public interface IExcelSourceReader : IDataSourceReader
    {
        /// <summary>
        /// Indica si la primera fila contiene los encabezados.
        /// </summary>
        bool HasHeader { get; set; }

        /// <summary>
        /// Ruta del archivo Excel seleccionado.
        /// Se utiliza para pre-cargar la metadata (como las hojas) antes de la importación completa.
        /// </summary>
        string FilePath { get; set; }
        /// <summary>
        /// Nombre de la hoja seleccionada que se va a procesar.
        /// </summary>
        string SheetName { get; set; }
        /// <summary>
        /// Obtiene la lista de nombres de todas las hojas disponibles en el archivo asignado a FilePath.
        /// </summary>
        /// <returns>Lista de nombres de hojas.</returns>
        List<string> GetSheets();
    }
}
