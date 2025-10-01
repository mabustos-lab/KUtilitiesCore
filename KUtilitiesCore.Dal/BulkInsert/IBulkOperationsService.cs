using System.Data;

namespace KUtilitiesCore.Dal.BulkInsert
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