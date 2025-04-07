namespace KUtilitiesCore.Diagnostics.Logger
{
    public interface ILoggerService
    {
        #region Methods

        void Log(LogLevel level, string message, Exception exception = null);

        void LogCritical(string message, Exception exception = null);

        void LogDebug(string message);

        void LogError(string message, Exception exception = null);

        void LogInformation(string message);

        void LogWarning(string message);

        #endregion Methods
    }
}