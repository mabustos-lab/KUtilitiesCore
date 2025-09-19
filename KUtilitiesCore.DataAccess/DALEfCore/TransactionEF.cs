using KUtilitiesCore.DataAccess.DAL;

#if NETCOREAPP
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
#endif

namespace KUtilitiesCore.DataAccess.DALEfCore
{
    public class TransactionEF : TransactionBase
    {
#if NETCOREAPP
        /// <summary>
        /// Constructor para EF Core (Microsoft.EntityFrameworkCore)
        /// </summary>
        public TransactionEF(IDbContextTransaction transaction)
            : base(transaction.GetDbTransaction()) // Convertir a DbTransaction
        {
        }
#elif NETFRAMEWORK

        /// <summary>
        /// Constructor para EF6 (System.Data.Entity)
        /// </summary>
        public TransactionEF(System.Data.Entity.DbContextTransaction transaction)
            : base(transaction.UnderlyingTransaction) // Convertir a DbTransaction
        {
        }

#endif
    }
}