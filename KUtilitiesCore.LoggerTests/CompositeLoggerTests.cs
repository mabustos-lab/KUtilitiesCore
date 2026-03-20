using KUtilitiesCore.Logger;
using KUtilitiesCore.Logger.Options;
using KUtilitiesCore.Logger.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KUtilitiesCore.LoggerTests
{
    [TestClass]
    public class CompositeLoggerTests
    {
        private LoggerServiceFactory _factory = null!;

        [TestInitialize]
        public void Setup()
        {
            _factory = new LoggerServiceFactory();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _factory.Dispose();
        }

        private class MockProvider : ILoggerServiceProvider
        {
            public string Name { get; }
            public MockProvider(string name) => Name = name;
            public virtual ILoggerService<TCategoryName> CreateLogger<TCategoryName>() => NullLoggerService<TCategoryName>.Instance;
        }

        private class EnabledMockProvider : ILoggerServiceProvider
        {
            public string Name { get; }
            private readonly LogLevel _minLevel;
            public EnabledMockProvider(string name, LogLevel minLevel) { Name = name; _minLevel = minLevel; }
            public ILoggerService<TCategoryName> CreateLogger<TCategoryName>() => new EnabledMockLogger<TCategoryName>(_minLevel);
        }

        private class EnabledMockLogger<TCategoryName> : ILoggerService<TCategoryName>
        {
            private readonly LogLevel _minLevel;
            public EnabledMockLogger(LogLevel minLevel) => _minLevel = minLevel;
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;
            public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
            public void LogTrace(string message, EventId? eventId, params object[] args) { }
            public void LogTrace(Exception exception, string message, EventId? eventId, params object[] args) { }
            public void LogDebug(string message, EventId? eventId, params object[] args) { }
            public void LogDebug(Exception exception, string message, EventId? eventId, params object[] args) { }
            public void LogInformation(string message, EventId? eventId, params object[] args) { }
            public void LogInformation(Exception exception, string message, EventId? eventId, params object[] args) { }
            public void LogWarning(string message, EventId? eventId, params object[] args) { }
            public void LogWarning(Exception exception, string message, EventId? eventId, params object[] args) { }
            public void LogError(string message, EventId? eventId, params object[] args) { }
            public void LogError(Exception exception, string message, EventId? eventId, params object[] args) { }
            public void LogCritical(string message, EventId? eventId, params object[] args) { }
            public void LogCritical(Exception exception, string message, EventId? eventId, params object[] args) { }
        }

        [TestMethod]
        public void GetLogger_WithCompositeTrue_ReturnsCompositeLogger()
        {
            // Arrange
            _factory.AddProvider(new MockProvider("Debug1"));
            _factory.AddProvider(new MockProvider("Debug2"));

            // Act
            var logger = _factory.GetLogger<CompositeLoggerTests>(composite: true);

            // Assert
            Assert.IsInstanceOfType(logger, typeof(CompositeLogger<CompositeLoggerTests>));
        }

        [TestMethod]
        public void GetLogger_WithCompositeFalse_ReturnsFirstProviderLogger()
        {
            // Arrange
            _factory.AddProvider(new MockProvider("Debug1"));
            _factory.AddProvider(new MockProvider("Debug2"));

            // Act
            var logger = _factory.GetLogger<CompositeLoggerTests>(composite: false);

            // Assert
            Assert.IsNotInstanceOfType(logger, typeof(CompositeLogger<CompositeLoggerTests>));
        }

        [TestMethod]
        public void CompositeLogger_IsEnabled_ReturnsTrueIfAnyLoggerIsEnabled()
        {
            // Arrange
            _factory.AddProvider(new EnabledMockProvider("D1", LogLevel.Error));
            _factory.AddProvider(new EnabledMockProvider("D2", LogLevel.Information));

            var logger = _factory.GetLogger<CompositeLoggerTests>(composite: true);

            // Act & Assert
            Assert.IsTrue(logger.IsEnabled(LogLevel.Information), "Should be enabled because second logger is enabled for Information.");
            Assert.IsFalse(logger.IsEnabled(LogLevel.Debug), "Should be disabled because none are enabled for Debug.");
        }

        [TestMethod]
        public void GetLogger_Composite_IsCached()
        {
            // Arrange
            _factory.AddProvider(new MockProvider("Debug1"));

            // Act
            var logger1 = _factory.GetLogger<CompositeLoggerTests>(composite: true);
            var logger2 = _factory.GetLogger<CompositeLoggerTests>(composite: true);

            // Assert
            Assert.AreSame(logger1, logger2, "Composite loggers for the same category should be cached.");
        }
    }
}
