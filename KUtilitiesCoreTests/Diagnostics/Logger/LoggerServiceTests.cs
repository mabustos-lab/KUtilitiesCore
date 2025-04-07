using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.Diagnostics.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Diagnostics.Logger.Tests
{
    [TestClass()]
    public class LoggerServiceTests
    {
        [TestMethod()]
        public void AddFileLogServiceTest()
        {
            LogFactory.Service.RegisterLogger(FileLoggerService.Create(""));
            Assert.IsTrue(LogFactory.Service.Contains<FileLoggerService>());
            
        }

        [TestMethod()]
        public void LogCriticalTest()
        {
            if (!LogFactory.Service.Contains<FileLoggerService>())
                LogFactory.Service.RegisterLogger(FileLoggerService.Create(""));
            LogFactory.Service.LogCritical("Prueba falla de Log", new Exception("Prueba"));
            //Assert.Fail();
        }

        [TestMethod()]
        public void LogDebugTest()
        {
            if (!LogFactory.Service.Contains<FileLoggerService>())
                LogFactory.Service.RegisterLogger(FileLoggerService.Create(""));
            LogFactory.Service.LogDebug("Mensaje de prueba Debug.");
        }

        [TestMethod()]
        public void LogErrorTest()
        {
            if (!LogFactory.Service.Contains<FileLoggerService>())
                LogFactory.Service.RegisterLogger(FileLoggerService.Create(""));
            try
            {
                int zero = 0;
                int result = 1 / zero;
            }
            catch (DivideByZeroException ex)
            {
                LogFactory.Service.LogError("Ocurrio un problema en la operación",ex);
            }
            
        }


    }
}