using KUtilitiesCore.Logger;
using KUtilitiesCore.Logger.Options;
using KUtilitiesCore.Logger.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace KUtilitiesCore.LoggerTests
{
    [TestClass]
    public class LoggerServiceFactory_DebugLoggerServiceProvider_Tests
    {
        private LoggerServiceFactory _factory = null!;
        private const string ProviderName = "DebugLoggerServiceProvider";

        [TestInitialize]
        public void Setup()
        {
            var options = new LoggerOptions(); // Ajusta seg�n tus opciones reales
            var debugProvider = new DebugLoggerServiceProvider(options);
            _factory = new LoggerServiceFactory();
            _factory.AddProvider(debugProvider);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _factory.Dispose();
        }

        [TestMethod]
        public void GetLogger_Returns_LoggerService_Instance()
        {
            var logger = _factory.GetLogger<LoggerServiceFactory_DebugLoggerServiceProvider_Tests>(ProviderName);
            Assert.IsNotNull(logger, "El logger no debe ser nulo.");
            Assert.IsInstanceOfType(logger, typeof(ILoggerService<LoggerServiceFactory_DebugLoggerServiceProvider_Tests>), "El logger debe ser del tipo ILoggerService<T>.");
        }

        [TestMethod]
        public void LoggerService_LogDebug_DoesNotThrow()
        {
            var logger = _factory.GetLogger<LoggerServiceFactory_DebugLoggerServiceProvider_Tests>(ProviderName);
            Assert.IsNotNull(logger, "El logger no debe ser nulo.");
            try
            {
                logger.LogDebug("Mensaje de prueba {0}", null, 123);
            }
            catch (Exception ex)
            {
                Assert.Fail($"LogDebug lanz� una excepci�n: {ex}");
            }
        }

        [TestMethod]
        public void LoggerService_LogError_WithException_DoesNotThrow()
        {
            var logger = _factory.GetLogger<LoggerServiceFactory_DebugLoggerServiceProvider_Tests>(ProviderName);
            Assert.IsNotNull(logger, "El logger no debe ser nulo.");
            var ex = new InvalidOperationException("Excepci�n de prueba");
            try
            {
                logger.LogError(ex, "Error con excepci�n {0}", null, 456);
            }
            catch (Exception e)
            {
                Assert.Fail($"LogError (con excepci�n) lanz� una excepci�n: {e}");
            }
        }

        [TestMethod]
        public void LoggerService_LogInformation_DoesNotThrow()
        {
            var logger = _factory.GetLogger<LoggerServiceFactory_DebugLoggerServiceProvider_Tests>(ProviderName);
            Assert.IsNotNull(logger, "El logger no debe ser nulo.");
            try
            {
                logger.LogInformation("Informaci�n de prueba", null);
            }
            catch (Exception ex)
            {
                Assert.Fail($"LogInformation lanz� una excepci�n: {ex}");
            }
        }

        [TestMethod]
        public void LoggerService_LogCritical_DoesNotThrow()
        {
            var logger = _factory.GetLogger<LoggerServiceFactory_DebugLoggerServiceProvider_Tests>(ProviderName);
            Assert.IsNotNull(logger, "El logger no debe ser nulo.");
            try
            {
                logger.LogCritical("Cr�tico de prueba", null);
            }
            catch (Exception ex)
            {
                Assert.Fail($"LogCritical lanz� una excepci�n: {ex}");
            }
        }
    }
}