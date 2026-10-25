using KUtilitiesCore.Dal;
using KUtilitiesCore.Dal.ConnectionBuilder;
using KUtilitiesCore.Dal.Exceptions;
using KUtilitiesCore.Dal.UOW;
using KUtilitiesCore.DataAccess.UOW.Interfaces;
using Microsoft.Data.SqlClient;
using Moq;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace KUtilitiesCore.Dal.Tests
{
    /// <summary>
    /// Pruebas del contrato transaccional del UoW de Dal (spec dal-transactions):
    /// pasividad del getter (jamás abre transacciones implícitas), contrato 0/no-op
    /// de SaveChanges/Rollback sin transacción registrada y semántica transaccional explícita.
    /// </summary>
    [TestClass()]
    public class DaoUowTransactionTests
    {
        #region Test Types

        internal class TestEntity
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        internal class ExposingDaoRepository : DaoRepository<TestEntity>
        {
            public ExposingDaoRepository(IDaoUowContext context) : base(context)
            {
            }

            public ITransaction? ExposedTransaction => Transaction;

            public override void AddEntity(TestEntity entity) => throw new NotImplementedException();
            public override void DeleteEntity(TestEntity entity) => throw new NotImplementedException();
            public override void UpdateEntity(TestEntity entity) => throw new NotImplementedException();
            public override int Count(ISpecification<TestEntity> spec) => throw new NotImplementedException();
            public override Task<int> CountAsync(ISpecification<TestEntity> spec) => throw new NotImplementedException();
            public override IEnumerable<TestEntity> GetEntities(ISpecification<TestEntity> spec) => throw new NotImplementedException();
            public override Task<IEnumerable<TestEntity>> GetEntitiesAsync(ISpecification<TestEntity> spec) => throw new NotImplementedException();
            public override TestEntity GetFirstOrDefault(ISpecification<TestEntity> spec) => throw new NotImplementedException();
            public override Task<TestEntity> GetFirstOrDefaultAsync(ISpecification<TestEntity> spec) => throw new NotImplementedException();
        }

        internal class ExposingRawRepository : RawRepositorybase
        {
            public ExposingRawRepository(IDaoUowContext context) : base(context)
            {
            }

            public ITransaction? ExposedTransaction => Transaction;
        }

        internal class TestableDaoUnitOfWork : DaoUnitOfWork
        {
            public TestableDaoUnitOfWork(IDaoContext context) : base(context)
            {
            }

            public IDaoUowContext ExposedUowContext => UowContext;
        }

        #endregion Test Types

        #region 1. Congelacion de la pasividad actual (tests de caracterizacion)

        [TestMethod]
        public void TransactionGetter_RepeatedAccess_ReturnsNullAndNeverOpensTransaction()
        {
            var contextMock = new Mock<IDaoContext>();
            using var uow = new TestableDaoUnitOfWork(contextMock.Object);

            for (int i = 0; i < 5; i++)
            {
                Assert.IsNull(uow.ExposedUowContext.Transaction,
                    "El getter debe devolver null sin transaccion registrada y jamas abrir una implicita.");
            }

            contextMock.Verify(c => c.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Never);
        }

        [TestMethod]
        public void RepositoryTransactionAccess_ReturnsNullWithoutImplicitOpen()
        {
            var contextMock = new Mock<IDaoContext>();
            using var uow = new TestableDaoUnitOfWork(contextMock.Object);
            uow.RegisterCustomRepository<TestEntity, ExposingDaoRepository>();
            uow.RegisterCustomRepository<ExposingRawRepository>();

            var daoRepo = (ExposingDaoRepository)uow.Repository<TestEntity>();
            var rawRepo = uow.RawRepository<ExposingRawRepository>();

            for (int i = 0; i < 5; i++)
            {
                Assert.IsNull(daoRepo.ExposedTransaction);
                Assert.IsNull(rawRepo.ExposedTransaction);
            }

            contextMock.Verify(c => c.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Never);
        }

        [TestMethod]
        public void SaveChanges_WithoutRegisteredTransaction_ReturnsZeroAndOpensNothing()
        {
            var contextMock = new Mock<IDaoContext>();
            using var uow = new TestableDaoUnitOfWork(contextMock.Object);

            Assert.AreEqual(0, uow.SaveChanges());
            Assert.AreEqual(0, uow.SaveChanges());

            contextMock.Verify(c => c.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Never);
        }

        [TestMethod]
        public void Rollback_WithoutRegisteredTransaction_IsSilentNoOp()
        {
            var contextMock = new Mock<IDaoContext>();
            using var uow = new TestableDaoUnitOfWork(contextMock.Object);

            uow.Rollback();

            contextMock.Verify(c => c.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Never);
        }

        #endregion 1. Congelacion de la pasividad actual (tests de caracterizacion)

        #region 2. API explicita uow.BeginTransaction

        [TestMethod]
        public void BeginTransaction_Default_RegistersReadCommittedTransactionInUow()
        {
            var contextMock = new Mock<IDaoContext>();
            var txMock = new Mock<ITransaction>();
            using var uow = new TestableDaoUnitOfWork(contextMock.Object);
            contextMock.Setup(c => c.BeginTransaction(IsolationLevel.ReadCommitted))
                .Returns(txMock.Object);

            var tx = uow.BeginTransaction();

            Assert.AreSame(txMock.Object, tx,
                "uow.BeginTransaction() debe devolver la transaccion creada por el contexto.");
            Assert.AreSame(txMock.Object, uow.ExposedUowContext.Transaction,
                "La transaccion debe quedar registrada en el UoW.");
            contextMock.Verify(c => c.BeginTransaction(IsolationLevel.ReadCommitted), Times.Once);
        }

        [TestMethod]
        public void BeginTransaction_ExplicitIsolationLevel_IsForwardedToContext()
        {
            var contextMock = new Mock<IDaoContext>();
            var txMock = new Mock<ITransaction>();
            using var uow = new TestableDaoUnitOfWork(contextMock.Object);
            contextMock.Setup(c => c.BeginTransaction(IsolationLevel.Serializable))
                .Returns(txMock.Object);

            var tx = uow.BeginTransaction(IsolationLevel.Serializable);

            Assert.AreSame(txMock.Object, tx);
            Assert.AreSame(txMock.Object, uow.ExposedUowContext.Transaction);
            contextMock.Verify(c => c.BeginTransaction(IsolationLevel.Serializable), Times.Once);
        }

        [TestMethod]
        public void BeginTransaction_OnContextDirectly_DoesNotRegisterInUow()
        {
            var contextMock = new Mock<IDaoContext>();
            var txMock = new Mock<ITransaction>();
            using var uow = new TestableDaoUnitOfWork(contextMock.Object);
            contextMock.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>()))
                .Returns(txMock.Object);

            uow.ExposedUowContext.Context.BeginTransaction();

            Assert.IsNull(uow.ExposedUowContext.Transaction,
                "Abrir una transaccion directamente en el contexto no debe registrarla en el UoW.");
        }

        [TestMethod]
        public void SaveChanges_WithRegisteredTransaction_CommitsAndClearsUow()
        {
            var contextMock = new Mock<IDaoContext>();
            var txMock = new Mock<ITransaction>();
            using var uow = new TestableDaoUnitOfWork(contextMock.Object);
            contextMock.Setup(c => c.BeginTransaction(IsolationLevel.ReadCommitted))
                .Returns(txMock.Object);

            uow.BeginTransaction();
            var result = uow.SaveChanges();

            Assert.AreEqual(1, result,
                "SaveChanges debe confirmar la transaccion registrada y devolver 1.");
            txMock.Verify(t => t.Commit(), Times.Once);
            Assert.IsNull(uow.ExposedUowContext.Transaction,
                "Tras SaveChanges el UoW debe quedar sin transaccion registrada.");
        }

        [TestMethod]
        public void Rollback_WithRegisteredTransaction_RevertsAndClearsUow()
        {
            var contextMock = new Mock<IDaoContext>();
            var txMock = new Mock<ITransaction>();
            using var uow = new TestableDaoUnitOfWork(contextMock.Object);
            contextMock.Setup(c => c.BeginTransaction(IsolationLevel.ReadCommitted))
                .Returns(txMock.Object);

            uow.BeginTransaction();
            uow.Rollback();

            txMock.Verify(t => t.Rollback(), Times.Once);
            Assert.IsNull(uow.ExposedUowContext.Transaction,
                "Tras Rollback el UoW debe quedar sin transaccion registrada.");
        }

        #endregion 2. API explicita uow.BeginTransaction

        #region 2.3 Verificacion de vitalidad (design D4)

        [TestMethod]
        public void TransactionCommittedOutsideUow_GetterReturnsNullAndSaveChangesIsZero()
        {
            var contextMock = new Mock<IDaoContext>();
            var dbTxMock = new Mock<DbTransaction>();
            using var uow = new TestableDaoUnitOfWork(contextMock.Object);
            contextMock.Setup(c => c.BeginTransaction(IsolationLevel.ReadCommitted))
                .Returns(new TransactionBase(dbTxMock.Object));

            var tx = uow.BeginTransaction();

            Assert.IsNotNull(uow.ExposedUowContext.Transaction,
                "Mientras la transaccion esta viva, el getter debe devolverla.");
            tx.Commit();

            Assert.IsNull(uow.ExposedUowContext.Transaction,
                "Finalizada la transaccion fuera del UoW, el getter debe devolver null (referencia obsoleta).");
            Assert.AreEqual(0, uow.SaveChanges(),
                "Sin transaccion viva registrada, SaveChanges debe devolver 0.");
        }

        [TestMethod]
        public void TransactionDisposedOutsideUow_GetterReturnsNullAndSaveChangesIsZero()
        {
            var contextMock = new Mock<IDaoContext>();
            var dbTxMock = new Mock<DbTransaction>();
            using var uow = new TestableDaoUnitOfWork(contextMock.Object);
            contextMock.Setup(c => c.BeginTransaction(IsolationLevel.ReadCommitted))
                .Returns(new TransactionBase(dbTxMock.Object));

            var tx = uow.BeginTransaction();

            Assert.IsNotNull(uow.ExposedUowContext.Transaction);
            tx.Dispose();

            Assert.IsNull(uow.ExposedUowContext.Transaction,
                "Dispuesta la transaccion fuera del UoW, el getter debe devolver null (referencia obsoleta).");
            Assert.AreEqual(0, uow.SaveChanges());
        }

        [TestMethod]
        public void RolledBackOutsideUow_GetterReturnsNullAndSaveChangesIsZero()
        {
            var contextMock = new Mock<IDaoContext>();
            var dbTxMock = new Mock<DbTransaction>();
            using var uow = new TestableDaoUnitOfWork(contextMock.Object);
            contextMock.Setup(c => c.BeginTransaction(IsolationLevel.ReadCommitted))
                .Returns(new TransactionBase(dbTxMock.Object));

            var tx = uow.BeginTransaction();

            Assert.IsNotNull(uow.ExposedUowContext.Transaction);
            tx.Rollback();

            Assert.IsNull(uow.ExposedUowContext.Transaction,
                "Revertida la transaccion fuera del UoW, el getter debe devolver null (referencia obsoleta).");
            Assert.AreEqual(0, uow.SaveChanges());
        }

        #endregion 2.3 Verificacion de vitalidad (design D4)

        #region 3. Integracion contra SQL Server local

        private const string CreateTempTableSql = "CREATE TABLE #TempTx (Id INT, Value NVARCHAR(100));";
        private const string InsertSql = "INSERT INTO #TempTx (Id, Value) VALUES (1, N'valor');";
        private const string CountSql = "SELECT COUNT(*) FROM #TempTx;";

        private static bool _sqlFactoryRegistered;

        // Registra el factory de SqlClient una sola vez por proceso; un registro duplicado
        // (otra clase de pruebas ya lo registro) se ignora silenciosamente.
        private static void EnsureSqlClientFactoryRegistered()
        {
            if (_sqlFactoryRegistered)
            {
                return;
            }
#if NET8_0_OR_GREATER
            try
            {
                DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
            }
            catch (ArgumentException)
            {
                // Ya registrado por otra clase de pruebas en el mismo proceso.
            }
#endif
            _sqlFactoryRegistered = true;
        }

        // Omite la prueba cuando no hay SQL Server local disponible (GitHub Actions o
        // sistemas no Windows).
        private static void AssertLocalSqlEnvironment()
        {
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                Assert.Inconclusive("Esta prueba se omite en GitHub Actions.");
            }
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("Esta prueba solo se ejecuta en Windows.");
            }
        }

        private static SecureConnectionBuilder CreateConnectionBuilder() => new()
        {
            InitialCatalog = "master",
            ServerName = "localhost",
            Encrypt = true,
            IntegratedSecurity = true,
            TrustServerCertificate = true
        };

        // Crea un UoW real contra SQL Server local; el Dispose del UoW tambien libera el contexto.
        private static TestableDaoUnitOfWork CreateRealUow()
            => new(new DaoContext(CreateConnectionBuilder()));

        [TestMethod]
        public void BeginTransaction_Insert_SaveChanges_PersistsRowAndClearsUow()
        {
            AssertLocalSqlEnvironment();
            EnsureSqlClientFactoryRegistered();

            using var uow = CreateRealUow();
            var context = uow.ExposedUowContext.Context;
            context.ExecuteNonQuery(CreateTempTableSql);

            var tx = uow.BeginTransaction();
            Assert.IsNotNull(uow.ExposedUowContext.Transaction,
                "Mientras la transaccion esta viva, el getter debe devolverla.");
            context.ExecuteNonQuery(InsertSql, transaction: tx);

            Assert.AreEqual(1, uow.SaveChanges(),
                "SaveChanges debe confirmar la transaccion registrada y devolver 1.");
            Assert.IsNull(uow.ExposedUowContext.Transaction,
                "Tras SaveChanges el UoW debe quedar sin transaccion registrada.");
            Assert.AreEqual(1, context.Scalar<int>(CountSql),
                "La fila debe persistir tras el commit de SaveChanges.");
            Assert.AreEqual(0, uow.SaveChanges(),
                "Un segundo SaveChanges sin transaccion registrada debe devolver 0.");
        }

        [TestMethod]
        public void BeginTransaction_Insert_Rollback_RowDoesNotPersist()
        {
            AssertLocalSqlEnvironment();
            EnsureSqlClientFactoryRegistered();

            using var uow = CreateRealUow();
            var context = uow.ExposedUowContext.Context;
            context.ExecuteNonQuery(CreateTempTableSql);

            var tx = uow.BeginTransaction();
            context.ExecuteNonQuery(InsertSql, transaction: tx);
            uow.Rollback();

            Assert.IsNull(uow.ExposedUowContext.Transaction,
                "Tras Rollback el UoW debe quedar sin transaccion registrada.");
            Assert.AreEqual(0, context.Scalar<int>(CountSql),
                "La fila no debe persistir tras el rollback del UoW.");
        }

        [TestMethod]
        public void UsingBlockWithoutCommit_RevertsAutomatically()
        {
            AssertLocalSqlEnvironment();
            EnsureSqlClientFactoryRegistered();

            using var uow = CreateRealUow();
            var context = uow.ExposedUowContext.Context;
            context.ExecuteNonQuery(CreateTempTableSql);

            using (var tx = uow.BeginTransaction())
            {
                context.ExecuteNonQuery(InsertSql, transaction: tx);
            } // Dispose sin Commit revierte la transaccion (semantica de ITransaction).

            Assert.IsNull(uow.ExposedUowContext.Transaction,
                "La referencia obsoleta no debe exponerse tras finalizar la transaccion fuera del UoW.");
            Assert.AreEqual(0, context.Scalar<int>(CountSql),
                "Salir del using sin Commit debe revertir la insercion.");
        }

        [TestMethod]
        public void ExplicitCommit_PersistsRow()
        {
            AssertLocalSqlEnvironment();
            EnsureSqlClientFactoryRegistered();

            using var uow = CreateRealUow();
            var context = uow.ExposedUowContext.Context;
            context.ExecuteNonQuery(CreateTempTableSql);

            using (var tx = uow.BeginTransaction())
            {
                context.ExecuteNonQuery(InsertSql, transaction: tx);
                tx.Commit();
            }

            Assert.IsNull(uow.ExposedUowContext.Transaction,
                "Finalizada la transaccion fuera del UoW, el getter debe devolver null.");
            Assert.AreEqual(1, context.Scalar<int>(CountSql),
                "La fila debe persistir tras el commit explicito.");
        }

        [TestMethod]
        public void RepeatedCommitRollbackDispose_DoNotThrow_AndFinalStateIsCoherent()
        {
            AssertLocalSqlEnvironment();
            EnsureSqlClientFactoryRegistered();

            using var uow = CreateRealUow();
            var context = uow.ExposedUowContext.Context;
            context.ExecuteNonQuery(CreateTempTableSql);

            var tx = uow.BeginTransaction();
            context.ExecuteNonQuery(InsertSql, transaction: tx);
            Assert.AreEqual(1, uow.SaveChanges());

            // Finalizada la transaccion, las llamadas repetidas deben ser no-op y no lanzar.
            tx.Commit();
            tx.Rollback();
            tx.Dispose();

            Assert.AreEqual(0, uow.SaveChanges(),
                "Sin transaccion viva registrada, SaveChanges debe devolver 0.");
            Assert.AreEqual(1, context.Scalar<int>(CountSql),
                "El estado confirmado debe permanecer intacto.");

            uow.Rollback(); // no-op silencioso sin transaccion registrada.

            Assert.AreEqual(1, context.Scalar<int>(CountSql),
                "El Rollback no-op no debe alterar el estado confirmado.");
        }

        [TestMethod]
        public void UowDispose_DisposesRegisteredTransactionOnce_AndRepeatedDisposeIsSafe()
        {
            var contextMock = new Mock<IDaoContext>();
            var txMock = new Mock<ITransaction>();
            var uow = new TestableDaoUnitOfWork(contextMock.Object); // Dispose manual: no using.
            contextMock.Setup(c => c.BeginTransaction(IsolationLevel.ReadCommitted))
                .Returns(txMock.Object);

            uow.BeginTransaction();
            uow.Dispose();
            uow.Dispose();

            txMock.Verify(t => t.Dispose(), Times.Once,
                "El Dispose del UoW debe liberar la transaccion registrada exactamente una vez.");
        }

        [TestMethod]
        public void ForeignConnectionTransaction_ThrowsDataAccessException_AndDoesNotExecute()
        {
            AssertLocalSqlEnvironment();
            EnsureSqlClientFactoryRegistered();

            using var uow = CreateRealUow();
            var context = uow.ExposedUowContext.Context;
            context.ExecuteNonQuery(CreateTempTableSql);

            var foreignContext = new DaoContext(CreateConnectionBuilder());
            try
            {
                var foreignTx = foreignContext.BeginTransaction(IsolationLevel.ReadCommitted);

                var ex = Assert.ThrowsExactly<DataAccessException>(() =>
                    context.ExecuteNonQuery(InsertSql, transaction: foreignTx));

                Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidOperationException));
                StringAssert.Contains(ex.InnerException!.Message,
                    "La transacción pertenece a una conexión diferente");
                Assert.AreEqual(0, context.Scalar<int>(CountSql),
                    "El comando rechazado no debe ejecutarse.");
            }
            finally
            {
                foreignContext.Dispose();
            }
        }

        #endregion 3. Integracion contra SQL Server local
    }
}
