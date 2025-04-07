using KUtilitiesCore.Diagnostics.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KUtilitiesCore.Diagnostics.Tests
{

    [TestClass()]
    public class ExceptionInfoTests
    {

        [TestMethod()]
        public void Constructor_WithException_ShouldLoadException()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act
            var exceptionInfo = new ExceptionInfo(exception);

            var str = exceptionInfo.GetReport();
            // Assert
            Assert.IsTrue(str.Contains("Test exception"));
        }

        [TestMethod()]
        public void LoadAditionalInfo_ShouldAddDataToAditionalInfo()
        {
            // Arrange
            var exception = new Exception();
            exception.Data["Key1"] = "Value1";
            exception.Data["Key2"] = "Value2";
            var exceptionInfo = new ExceptionInfo(exception);


            // Assert
            Assert.AreEqual(2, exceptionInfo.AditionaInfo.Count);
            //Assert.Contains(exceptionInfo.AditionaInfo, kvp => kvp.Key == "Key1" && kvp.Value == "Value1");
            //Assert.Contains(exceptionInfo.AditionaInfo, kvp => kvp.Key == "Key2" && kvp.Value == "Value2");
        }

        [TestMethod()]
        public void GetStackTraceBlock_ShouldReturnStackTrace()
        {
            // Arrange
            Exception exception = null;
            try
            {
                int zero = 0;
                int result = 1 / zero;
            }
            catch (DivideByZeroException ex)
            {
                exception = ex;
            }
            var exceptionInfo = new ExceptionInfo(exception);

            // Act
            var stackTraceBlock = exceptionInfo.GetReport();

            // Assert
            Assert.IsTrue(stackTraceBlock.Contains("DivideByZeroException"));
        }

        [TestMethod()]
        public void GetHelpLinkBlock_AggregateExceptionTest()
        {
            // Arrange
            AggregateException aggregateException = null;
            try
            {
                Task task1 = Task.Run(() => throw new InvalidOperationException("Invalid operation"));
                Task task2 = Task.Run(() => throw new ArgumentNullException("Argument null"));
                Task.WaitAll(task1, task2);
            }
            catch (AggregateException ex)
            {
                aggregateException = ex;
            }
            var exceptionInfo = new ExceptionInfo(aggregateException);

            // Act
            var helpLinkBlock = exceptionInfo.GetReport();

            // Assert
            Assert.IsTrue(helpLinkBlock.Contains("InvalidOperationException"));
            Assert.IsTrue(helpLinkBlock.Contains("ArgumentNullException"));
        }

    }
}