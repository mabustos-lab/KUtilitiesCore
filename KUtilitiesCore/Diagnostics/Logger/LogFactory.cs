﻿namespace KUtilitiesCore.Diagnostics.Logger
{
    public class LogFactory : ILogFactory, IDisposable
    {
        #region Fields

        private static Lazy<LogFactory> _instanse = new Lazy<LogFactory>(() => new LogFactory());
        private bool _disposed;
        private Dictionary<Type, ILoggerService> logs = new Dictionary<Type, ILoggerService>();

        #endregion Fields

        #region Constructors

        private LogFactory()
        {
        }

        #endregion Constructors

        #region Properties

        public static ILogFactory Service => _instanse.Value;

        #endregion Properties

        #region Methods

        public bool Contains<Tlog>()
               where Tlog : ILoggerService
        {
            return logs.ContainsKey(typeof(Tlog));
        }
        public Tlog GetLogService<Tlog>()
             where Tlog : ILoggerService
        {
            if(logs.TryGetValue(typeof(Tlog), out var service))
                return (Tlog)Service;
            return default;
        }
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                foreach (var log in logs.Values)
                {
                    if (log is IDisposable)
                        ((IDisposable)log).Dispose();
                }
                logs.Clear();
                if (logs != null)
                    logs = null;
            }
            finally
            {
                _disposed = true;
                _instanse = new Lazy<LogFactory>(() => new LogFactory());
                GC.SuppressFinalize(this);
            }
        }

        public void Log(LogLevel level, string message, Exception exception = null)
        {
            if (!logs.Any())
                throw new InvalidOperationException("No se ha registrado ningún servicio de log.");
            foreach (var log in logs.Values)
                log.Log(level, message, exception);
        }

        public void LogCritical(string message, Exception exception = null)
        {
            Log(LogLevel.Critical, message, exception);
        }

        public void LogDebug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public void LogError(string message, Exception exception = null)
        {
            Log(LogLevel.Error, message, exception);
        }

        public void LogInformation(string message)
        {
            Log(LogLevel.Information, message);
        }

        public void LogWarning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public void RegisterLogger<Tlog>(Tlog logger)
            where Tlog : ILoggerService
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            Type loggerType = logger.GetType();
            if (logs.ContainsKey(loggerType))
            {
                throw new InvalidOperationException($"El logger de tipo {loggerType.FullName} ya está registrado.");
            }
            logs.Add(loggerType, logger);
        }

        public bool UnRegisterLogger<Tlog>()
            where Tlog : ILoggerService
        {
            Type loggerType = typeof(Tlog);
            if (!logs.TryGetValue(loggerType, out var logger))
                return false;
            if (logger is IDisposable disposableLogger)
                disposableLogger.Dispose();
            return logs.Remove(loggerType);
        }

        #endregion Methods
    }
}