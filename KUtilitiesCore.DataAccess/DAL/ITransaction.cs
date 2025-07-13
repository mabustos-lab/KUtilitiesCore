using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.DAL
{
    /// <summary>
    /// Define una interfaz para manejar transacciones, proporcionando métodos para confirmar o revertir cambios,
    /// así como para liberar recursos asociados a la transacción.
    /// </summary>
    /// <remarks>Si no se ha llamado al Commit antes de hacer Dispose, automáticamente se hará un RollBack</remarks>
    public interface ITransaction : IDisposable
    {
        #region Methods

        /// <summary>
        /// Confirma todos los cambios realizados durante la transacción actual, marcando su finalización exitosa.
        /// </summary>
        /// <remarks>
        /// Este método debe ser llamado únicamente cuando todos los pasos de la transacción se hayan completado correctamente.
        /// </remarks>
        void Commit();

        /// <summary>
        /// Revierte todos los cambios realizados durante la transacción actual, devolviendo el estado al momento
        /// en que la transacción fue iniciada o a un punto de retorno definido dentro de la misma.
        /// </summary>
        /// <remarks>
        /// Este método debe ser utilizado para manejar errores o inconsistencias detectadas durante la transacción.
        /// </remarks>
        void Rollback();

        #endregion Methods
    }
}
