using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Xml.Linq;

namespace KUtilitiesCore.Extensions.Tests
{
    [TestClass()]
    public class DataTableExtTests
    {
        private List<TestConvert> SourceTest;
        private DataTable SourceDataTableTest;

        [TestInitialize]
        public void Initialize()
        {
            SourceTest = new List<TestConvert>
            {
                new TestConvert { Name = "Juan Pérez", BirthDate = new DateTime(1990, 5, 15), Age = 33 },
                new TestConvert { Name = "María López", BirthDate = new DateTime(1985, 8, 20), Age = 38 },
                new TestConvert { Name = "Carlos García", BirthDate = new DateTime(2000, 12, 1), Age = 22 },
                new TestConvert { Name = "Ana Fernández", BirthDate = new DateTime(1995, 3, 10), Age = 28 },
                new TestConvert { Name = "Luis Martínez", BirthDate = new DateTime(1988, 7, 25), Age = 35 }
            };

            SourceDataTableTest = new DataTable();
            SourceDataTableTest.Columns.Add("Name", typeof(string));
            SourceDataTableTest.Columns.Add("BirthDate", typeof(DateTime));
            SourceDataTableTest.Columns.Add("Age", typeof(int));

            foreach (var item in SourceTest)
            {
                SourceDataTableTest.Rows.Add(item.Name, item.BirthDate, item.Age);
            }
        }

        [TestMethod()]
        public void DataColumnAsEnumerableTest()
        {
            var dc= SourceDataTableTest.GetColumns();
            Assert.IsTrue(dc!=null && dc.Count()==3);
        }

        [TestMethod()]
        public void DataTableToTextTest()
        {
            string result= SourceDataTableTest.ToText();
            Assert.IsTrue(!string.IsNullOrEmpty(result));
        }

        [TestMethod()]
        public void ToXmlTest()
        {
            XDocument result = SourceDataTableTest.ToXml();
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void ConvertToTest()
        {
            var result = SourceDataTableTest.MapTo<TestConvert>();
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void ConvertToDataTableTest()
        {
            DataTable result = SourceDataTableTest.ToDataTable();
            Assert.IsNotNull(result);
        }
        private class TestConvert
        {
            public string Name { get; set; }
            public DateTime BirthDate { get; set; }
            public int Age { get; set; }
        }
    }
}