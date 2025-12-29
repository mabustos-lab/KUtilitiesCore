using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Abstractions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.SQLLog
{
    public static class SqlLoggingExtensions
    {
        /// <summary>
        /// Configura logging automático con Serilog/ILogger
        /// </summary>
        public static IDisposable EnableSerilogSqlLogging<TConnection>(this TConnection connection,
            ILogger logger,
            SqlLoggingOptions options = null)
            where TConnection : DbConnection
        {
            return connection.EnableSqlLogging(entry =>
            {
                var properties = entry.ToDictionary();
                var message = entry.Message;

                switch (entry.LogLevel)
                {
                    case SqlLogLevel.Error:
                        logger.LogError("[SQL] {Message}", message);
                        break;
                    case SqlLogLevel.Warning:
                        logger.LogWarning("[SQL] {Message}", message);
                        break;
                    case SqlLogLevel.Info:
                        logger.LogInformation("[SQL] {Message}", message);
                        break;
                    case SqlLogLevel.Debug:
                        logger.LogDebug("[SQL] {Message}", message);
                        break;
                }
            }, options);
        }

        ///// <summary>
        ///// Configura logging para Application Insights
        ///// </summary>
        //public static IDisposable EnableAppInsightsSqlLogging<TDAO>(this TDAO context,
        //    TelemetryClient telemetryClient,
        //    SqlLoggingOptions options = null)
        //    where TDAO : ISqlExecutorContext
        //{
        //    return context.EnableSqlLogging(entry =>
        //    {
        //        var properties = entry.ToDictionary();

        //        if (entry.LogLevel == SqlLogLevel.Error)
        //        {
        //            telemetryClient.TrackException(
        //                new Exception($"SQL Error {entry.ErrorNumber}: {entry.Message}"),
        //                properties);
        //        }
        //        else
        //        {
        //            telemetryClient.TrackTrace(
        //                $"[SQL {entry.LogLevel}] {entry.Message}",
        //                GetSeverityLevel(entry.LogLevel),
        //                properties);
        //        }
        //    }, options);
        //}

        //private static SeverityLevel GetSeverityLevel(SqlLogLevel logLevel)
        //{
        //    return logLevel switch
        //    {
        //        SqlLogLevel.Error => SeverityLevel.Error,
        //        SqlLogLevel.Warning => SeverityLevel.Warning,
        //        SqlLogLevel.Info => SeverityLevel.Information,
        //        SqlLogLevel.Debug => SeverityLevel.Verbose,
        //        _ => SeverityLevel.Information
        //    };
        //}
    }
}
