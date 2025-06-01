using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.BulkInsert
{
    /// <summary>
    /// Interfaz genérica para realizar operaciones masivas de datos.
    /// </summary>
    public interface IBulkOperationsService
    {
        /// <summary>
        /// Inserta un DataTable en la base de datos utilizando la estrategia óptima según el proveedor.
        /// </summary>
        void BulkCopy(DataTable dataTable);
    }
}
