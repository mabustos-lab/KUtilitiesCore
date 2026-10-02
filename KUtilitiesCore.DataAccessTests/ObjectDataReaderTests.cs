using KUtilitiesCore.Dal.BulkInsert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace KUtilitiesCore.Dal.Tests
{
    /// <summary>
    /// Pruebas de las guardas de flujo nulo de <see cref="ObjectDataReader{T}"/>:
    /// lectura tras liberar el lector y lectores de datos anidados no soportados.
    /// </summary>
    [TestClass()]
    public class ObjectDataReaderTests
    {
        private sealed class Row
        {
            public int Id { get; set; }
        }

        [TestMethod()]
        public void GetData_Throws_NotSupported_AfterRead()
        {
            using var reader = new ObjectDataReader<Row>(new[] { new Row { Id = 1 } });

            Assert.IsTrue(reader.Read());
            Assert.ThrowsExactly<NotSupportedException>(() => reader.GetData(0));
        }

        [TestMethod()]
        public void Read_AfterDispose_Throws_ObjectDisposed()
        {
            var reader = new ObjectDataReader<Row>(new[] { new Row { Id = 1 } });
            reader.Dispose();

            Assert.ThrowsExactly<ObjectDisposedException>(() => reader.Read());
        }
    }
}
