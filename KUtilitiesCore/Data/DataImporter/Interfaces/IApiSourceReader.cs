using System;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter.Interfaces
{
    /// <summary>
    /// Interfaz para lectores que consumen datos directamente de una cadena JSON o respuesta de API.
    /// </summary>
    public interface IApiSourceReader : IDataSourceReader
    {
        /// <summary>
        /// Contenido JSON crudo o cuerpo de la respuesta que se va a procesar.
        /// </summary>
        string JsonContent { get; set; }

        // Opcional: Si la fuente requiere una URL base para validaciones futuras
        // string BaseUrl { get; set; }
    }
}
