using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataTests.DataImporter
{
    [TestClass()]
    public class CsvSourceReaderTests
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }
        [TestInitialize]
        public void InitializeFiles()
        {
            CsvSourceReaderDatagenerator.RootPath = TestContext.TestRunDirectory;
            CsvSourceReaderDatagenerator.CreateDataTest();
        }
        [TestCleanup]
        public void Cleanup()
        {
            CsvSourceReaderDatagenerator.ClearFilesTest();
        }
        [TestMethod()]
        public void CsvSourceReaderBasicDataTest()
        {
            var reader = CsvSourceReaderFactory.Create(CsvSourceReaderDatagenerator.BasicDataPath);
            try
            {
                DataTable dt = reader.ReadData();
                dt.PrintPretty(true);
                Assert.IsTrue(dt != null && dt.Rows.Count > 0);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [TestMethod()]
        public void CsvSourceReadeDuplicatedDataTest()
        {
            var reader = CsvSourceReaderFactory.Create(CsvSourceReaderDatagenerator.DuplicatedDataPath);
            try
            {
                DataTable dt = reader.ReadData();
                dt.PrintPretty(true);
                Assert.IsTrue(dt != null && dt.Rows.Count > 0);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [TestMethod()]
        public void CsvSourceReadeEmpyDataTest()
        {
            var reader = CsvSourceReaderFactory.Create(CsvSourceReaderDatagenerator.EmptyDataPath);
            try
            {
                DataTable dt = reader.ReadData();
                dt.PrintPretty(true);
                Assert.IsTrue(dt != null && dt.Rows.Count > 0);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [TestMethod()]
        public void CsvSourceReadeTabSplitDataTest()
        {
            var reader = CsvSourceReaderFactory.CreateTsvReader(CsvSourceReaderDatagenerator.TabSplitDataPath);
            try
            {
                DataTable dt = reader.ReadData();
                dt.PrintPretty(true);
                Assert.IsTrue(dt != null && dt.Rows.Count > 0);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
