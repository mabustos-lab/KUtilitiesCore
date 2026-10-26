using KUtilitiesCore.Dal;
using KUtilitiesCore.Dal.ConnectionBuilder;
using KUtilitiesCore.Dal.Exceptions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.Tests
{
    /// <summary>
    /// Tests del contrato del hook <c>OnConnectionOpened</c> de <see cref="DaoContext"/>:
    /// resolucion de la conexion viva sin re-entrada, disparo unico por conexion logica,
    /// exclusion de sondas y semantica fail-closed con envenenamiento del contexto.
    /// </summary>
    [TestClass]
    public class DaoContextOnConnectionOpenedTests
    {
        private SecureConnectionBuilder? builder;
        private DaoContext? context;

        [TestInitialize]
        public void Initialize()
        {
#if NET8_0_OR_GREATER
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
#endif
            builder = new SecureConnectionBuilder
            {
                InitialCatalog = "master",
                ServerName = "localhost",
                Encrypt = true,
                IntegratedSecurity = true,
                TrustServerCertificate = true
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            context?.Dispose();
            context = null;
        }

        private DaoContext CreateContext()
        {
            // builder nunca es null tras Initialize; el operador evita CS8604 en el ctor.
            context = new DaoContext(builder!);
            return context;
        }

        private static void SkipIfNotLocalSqlEnvironment()
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

        [TestMethod]
        public void OnConnectionOpened_HandlerSetsSessionContext_CompletesWithoutRecursion()
        {
            SkipIfNotLocalSqlEnvironment();
            int hookCalls = 0;
            DaoContext daoContext = CreateContext();
            daoContext.OnConnectionOpened = ctx =>
            {
                Interlocked.Increment(ref hookCalls);
                ctx.SetSessionContextAsync(
                    new KeyValuePair<string, object>("Usuario", "TestUser"))
                    .GetAwaiter().GetResult();
            };

            DbConnection connection = daoContext.Connection;

            Assert.AreEqual(1, hookCalls,
                "El hook debe dispararse exactamente una vez por conexion logica.");
            Assert.AreSame(connection, daoContext.Connection,
                "Las operaciones posteriores del contexto deben ver la misma conexion que uso el handler.");
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }

        [TestMethod]
        public void DatabaseExists_WithConfiguredHook_DoesNotInvokeHookNorMaterializeConnection()
        {
            SkipIfNotLocalSqlEnvironment();
            int hookCalls = 0;
            DaoContext daoContext = CreateContext();
            daoContext.OnConnectionOpened = _ => Interlocked.Increment(ref hookCalls);

            bool exists = daoContext.DatabaseExists();

            Assert.IsTrue(exists);
            Assert.AreEqual(0, hookCalls,
                "La conexion de sonda no debe disparar el hook.");
            // La conexion logica no queda materializada por la sonda: su primer acceso
            // dispara el hook exactamente una vez.
            _ = daoContext.Connection;
            Assert.AreEqual(1, hookCalls,
                "El primer acceso a la conexion logica debe disparar el hook exactamente una vez.");
        }

        [TestMethod]
        public async Task OnConnectionOpenedAsync_Configured_InvokedOnce_BlockingMaterialization()
        {
            SkipIfNotLocalSqlEnvironment();
            int asyncHookCalls = 0;
            Task? observedTask = null;
            DaoContext daoContext = CreateContext();
            daoContext.OnConnectionOpenedAsync = async ctx =>
            {
                Interlocked.Increment(ref asyncHookCalls);
                observedTask = ctx.SetSessionContextAsync(
                    new KeyValuePair<string, object>("Usuario", "AsyncUser"));
                await observedTask;
            };

            DbConnection connection = daoContext.Connection;

            Assert.AreEqual(1, asyncHookCalls,
                "El hook asincrono debe invocarse exactamente una vez por conexion logica.");
            Assert.IsNotNull(observedTask);
            Assert.IsTrue(observedTask.IsCompletedSuccessfully,
                "La materializacion debe bloquear hasta que el hook asincrono complete.");
            Assert.AreSame(connection, daoContext.Connection);
        }

        [TestMethod]
        public void BothHooks_SyncBeforeAsync_DeterministicOrder()
        {
            SkipIfNotLocalSqlEnvironment();
            var invocationOrder = new System.Collections.Generic.List<string>();
            DaoContext daoContext = CreateContext();
            daoContext.OnConnectionOpened = _ => invocationOrder.Add("sync");
            daoContext.OnConnectionOpenedAsync = async _ =>
            {
                invocationOrder.Add("async");
                await Task.CompletedTask;
            };

            _ = daoContext.Connection;

            CollectionAssert.AreEqual(new[] { "sync", "async" }, invocationOrder,
                "Con ambos hooks configurados el orden debe ser sincrono primero y asincrono despues.");
        }

        [TestMethod]
        public void OnConnectionOpened_ConcurrentAccess_FiresHookOnce_AllThreadsShareConnection()
        {
            SkipIfNotLocalSqlEnvironment();
            int hookCalls = 0;
            using var hookGate = new ManualResetEventSlim(false);
            DaoContext daoContext = CreateContext();
            daoContext.OnConnectionOpened = _ =>
            {
                Interlocked.Increment(ref hookCalls);
                // Mantiene la materializacion en curso para exponer la ventana de concurrencia.
                hookGate.Wait(TimeSpan.FromSeconds(10));
            };

            const int threadCount = 4;
            using var startBarrier = new Barrier(threadCount);
            var connections = new DbConnection?[threadCount];
            var threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                int index = i;
                threads[index] = new Thread(() =>
                {
                    startBarrier.SignalAndWait(TimeSpan.FromSeconds(10));
                    connections[index] = daoContext.Connection;
                });
                threads[index].Start();
            }

            Assert.IsTrue(
                SpinWait.SpinUntil(() => Volatile.Read(ref hookCalls) == 1, 10_000),
                "El hook debe dispararse una sola vez aunque varios hilos esperen la materializacion.");
            hookGate.Set();
            foreach (Thread thread in threads)
            {
                thread.Join(TimeSpan.FromSeconds(10));
            }

            Assert.AreEqual(1, hookCalls,
                "Bajo concurrencia el hook se invoca exactamente una vez por conexion logica.");
            DbConnection? first = connections[0];
            Assert.IsNotNull(first, "Todos los hilos deben completar la materializacion.");
            foreach (DbConnection? connection in connections)
            {
                Assert.IsNotNull(connection, "Todos los hilos deben completar la materializacion.");
                Assert.AreSame(first, connection,
                    "Todos los hilos concurrentes deben recibir la misma instancia de conexion.");
            }
        }

        [TestMethod]
        public void OnConnectionOpened_SessionContextE2E_SubsequentOperationReadsValue()
        {
            SkipIfNotLocalSqlEnvironment();
            DaoContext daoContext = CreateContext();
            daoContext.OnConnectionOpened = ctx =>
                ctx.SetSessionContextAsync(
                    new KeyValuePair<string, object>("Usuario", "E2EUser"))
                    .GetAwaiter().GetResult();

            string? sessionUser = daoContext.Scalar<string>("SELECT SESSION_CONTEXT(N'Usuario')");

            Assert.AreEqual("E2EUser", sessionUser,
                "La operacion posterior debe leer el contexto de sesion sobre la misma conexion logica.");
        }

        [TestMethod]
        public void OnConnectionOpened_HandlerThrowsE2E_PoisonedContextFailsWithoutDatabaseOperations()
        {
            SkipIfNotLocalSqlEnvironment();
            var handlerFailure = new InvalidOperationException("Fallo simulado E2E del handler.");
            DbConnection? capturedConnection = null;
            DaoContext daoContext = CreateContext();
            daoContext.OnConnectionOpened = ctx =>
            {
                capturedConnection = ctx.Connection;
                throw handlerFailure;
            };

            DataAccessException first =
                Assert.ThrowsExactly<DataAccessException>(() => _ = daoContext.Connection);
            Assert.IsNotNull(capturedConnection,
                "El handler debe poder resolver la conexion viva desde el propio contexto.");
            Assert.AreEqual(ConnectionState.Closed, capturedConnection.State,
                "La conexion abierta debe quedar cerrada y dispuesta tras el fallo del handler.");

            DataAccessException second =
                Assert.ThrowsExactly<DataAccessException>(() => _ = daoContext.Connection);
            Assert.AreSame(first.InnerException, second.InnerException,
                "El contexto envenenado re-lanza la excepcion cacheada sin ejecutar operaciones de base de datos.");
            Assert.ThrowsExactly<DataAccessException>(() => daoContext.ExecuteNonQuery("SELECT 1"),
                "Toda operacion del contexto envenenado debe fallar de forma determinista.");
        }

        [TestMethod]
        public void OnConnectionOpened_HandlerThrows_DisposesConnection_AndPoisonsContext()
        {
            SkipIfNotLocalSqlEnvironment();
            DbConnection? capturedConnection = null;
            DaoContext daoContext = CreateContext();
            daoContext.OnConnectionOpened = ctx =>
            {
                capturedConnection = ctx.Connection;
                throw new InvalidOperationException("Fallo simulado del handler de conexion abierta.");
            };

            Assert.ThrowsExactly<DataAccessException>(() => _ = daoContext.Connection);
            Assert.IsNotNull(capturedConnection,
                "El handler debe poder resolver la conexion viva desde el propio contexto.");
            Assert.AreEqual(ConnectionState.Closed, capturedConnection.State,
                "La conexion abierta debe disponerse sin fugas cuando el handler falla.");
            Assert.ThrowsExactly<DataAccessException>(() => _ = daoContext.Connection,
                "Todo acceso posterior debe re-lanzar la excepcion cacheada (envenenamiento).");
        }
    }
}
