using KUtilitiesCore.Dal.ConnectionBuilder;
using KUtilitiesCore.Dal.UOW;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KUtilitiesCore.Dal.Tests
{
    [TestClass()]
    public class DaoUOWTests
    {
        private SecureConnectionBuilder? builder;
        private DaoUnitOfWork? _uow;
        private static string queryStr = "SELECT name [Name], database_id [DataBaseID], create_date [CreateAt] FROM master.sys.databases;";

        internal class DatabasesModel
        {
            public string Name { get; set; }
            public int DataBaseID { get; set; }
            public DateTime CreateAt { get; set; }
        }

        public class RawRepo : RawRepositorybase
        {
            public RawRepo(IDaoUowContext context) : base(context)
            {
            }
            public DataTable GetDataBases()
            {
                try
                {
                    var result= Context.ExecuteReader(DaoUOWTests.queryStr, null,commandType: CommandType.Text);
                    return result.GetDataTable();
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                    throw;
                }
            }
        }

        [TestInitialize]
        public void Initilize()
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
            DaoContext context=new DaoContext(builder);
            _uow = new DaoUnitOfWork(context);
            _uow.RegisterCustomRepository<RawRepo>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _uow.Dispose();
            _uow = null;
        }
        [TestMethod]
        public void LoadRawRepoTest()
        {
            // Excluir la prueba si se ejecuta en GitHub Actions (variable de entorno 'GITHUB_ACTIONS' == 'true')
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                Assert.Inconclusive("Esta prueba se omite en GitHub Actions.");
            }
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("Esta prueba solo se ejecuta en Windows.");
            }
            var repo= _uow.RawRepository<RawRepo>();
            Assert.IsNotNull(repo);
            DataTable dt= repo.GetDataBases();
            Assert.IsNotNull(dt);
            Assert.IsGreaterThan(0, dt.Rows.Count);
        }
        
    }
}