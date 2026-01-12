using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter.Interfaces
{
    /// <summary>
    /// Interfaz base para obtener datos de cualquier origen de datos
    /// </summary>
    public interface IDataSourceReader
    {
        /// <summary>
        /// Indica si la fuente cumple los requisitos mínimos para ser leída
        /// </summary>
        /// <value>True si puede leer, False en caso contrario</value>
        bool CanRead { get; }

        /// <summary>
        /// Lee la fuente de datos de forma síncrona
        /// </summary>
        /// <returns>DataTable con los datos procesados</returns>
        /// <exception cref="InvalidOperationException">Cuando no se cumplen las precondiciones</exception>
        /// <exception cref="FileNotFoundException">Cuando la fuente no existe</exception>
        DataTable ReadData();

        /// <summary>
        /// Lee la fuente de datos de forma asíncrona
        /// </summary>
        /// <returns>Task que al completarse devuelve un DataTable con los datos procesados</returns>
        /// <exception cref="InvalidOperationException">Cuando no se cumplen las precondiciones</exception>
        /// <exception cref="FileNotFoundException">Cuando la fuente no existe</exception>
        Task<DataTable> ReadDataAsync();
    }
}
