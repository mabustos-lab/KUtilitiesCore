using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.OrderedInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Linq;

namespace KUtilitiesCore.OrderedInfo.Tests
{
    [TestClass()]
    public class OrderedCollectionInfoTests
    {
        private List<orderTest> orderTestList;
        [TestInitialize]
        public void setup()
        {
            orderTestList = CreateList();
        }
        [TestMethod()]
        public void OrderedCollectionInfoTest()
        {
            Assert.IsTrue(orderTestList.First().Name == "a");
            OrderedCollectionInfo info=new OrderedCollectionInfo();
            info.AddProperty(nameof(orderTest.Name), SortDirection.Descending);
            var res = info.Apply( orderTestList);
            Assert.IsTrue(res.First().Name == "c");
        }

        private List<orderTest> CreateList()
        {
            return new List<orderTest>
            {
                new orderTest { Id = 1, Name = "a", Description ="desc1"  },
                new orderTest { Id = 2, Name = "b", Description ="desc2"  },
                new orderTest { Id = 3, Name = "c", Description ="desc3"  },
                new orderTest { Id = 4, Name = "c", Description ="desc3"  }
            };
        }
        public class orderTest
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}