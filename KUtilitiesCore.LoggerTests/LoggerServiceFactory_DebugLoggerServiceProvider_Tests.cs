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
            var options = new LoggerOptions(); // Ajusta según tus opciones reales
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
                Assert.Fail($"LogDebug lanzó una excepción: {ex}");
            }
        }

        [TestMethod]
        public void LoggerService_LogError_WithException_DoesNotThrow()
        {
            var logger = _factory.GetLogger<LoggerServiceFactory_DebugLoggerServiceProvider_Tests>(ProviderName);
            Assert.IsNotNull(logger, "El logger no debe ser nulo.");
            var ex = new InvalidOperationException("Excepción de prueba");
            try
            {
                logger.LogError(ex, "Error con excepción {0}", null, 456);
            }
            catch (Exception e)
            {
                Assert.Fail($"LogError (con excepción) lanzó una excepción: {e}");
            }
        }

        [TestMethod]
        public void LoggerService_LogInformation_DoesNotThrow()
        {
            var logger = _factory.GetLogger<LoggerServiceFactory_DebugLoggerServiceProvider_Tests>(ProviderName);
            Assert.IsNotNull(logger, "El logger no debe ser nulo.");
            try
            {
                logger.LogInformation("Información de prueba", null);
            }
            catch (Exception ex)
            {
                Assert.Fail($"LogInformation lanzó una excepción: {ex}");
            }
        }

        [TestMethod]
        public void LoggerService_LogCritical_DoesNotThrow()
        {
            var logger = _factory.GetLogger<LoggerServiceFactory_DebugLoggerServiceProvider_Tests>(ProviderName);
            Assert.IsNotNull(logger, "El logger no debe ser nulo.");
            try
            {
                logger.LogCritical("Crítico de prueba", null);
            }
            catch (Exception ex)
            {
                Assert.Fail($"LogCritical lanzó una excepción: {ex}");
            }
        }
    }
}