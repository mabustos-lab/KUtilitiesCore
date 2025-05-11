using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.DAL
{
    /// <summary>
    /// Define una interfaz para interactuar con el contexto de acceso a datos.
    /// </summary>
    public interface IDALContext:IDisposable
    {
        /// <summary>
        /// Inicia una nueva transacción con el nivel de aislamiento especificado.
        /// </summary>
        /// <param name="isolationLevel">El nivel de aislamiento para la transacción.</param>
        /// <returns>Una instancia de <see cref="ITransaction"/> que representa la transacción iniciada.</returns>
        ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Snapshot);

        /// <summary>
        /// Verifica si la base de datos asociada al contexto existe.
        /// </summary>
        /// <returns><c>true</c> si la base de datos existe; de lo contrario, <c>false</c>.</returns>
        bool DatabaseExists();

    }
}
