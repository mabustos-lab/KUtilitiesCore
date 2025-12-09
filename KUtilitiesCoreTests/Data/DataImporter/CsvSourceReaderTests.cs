using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter.Tests
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
            var reader = new CsvSourceReader(CsvSourceReaderDatagenerator.BasicDataPath);
            try
            {
                DataTable dt = reader.ReadData();
                dt.PrintPretty(true);
                Assert.IsTrue(dt!=null && dt.Rows.Count>0);
            }
            catch (Exception )
            {
                throw;
            }
        }
        [TestMethod()]
        public void CsvSourceReadeDuplicatedDataTest()
        {
            var reader = new CsvSourceReader(CsvSourceReaderDatagenerator.DuplicatedDataPath);
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
            var reader = new CsvSourceReader(CsvSourceReaderDatagenerator.EmptyDataPath);
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
            var reader = new CsvSourceReader(CsvSourceReaderDatagenerator.TabSplitDataPath);
            reader.SpliterChar = "\t";
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