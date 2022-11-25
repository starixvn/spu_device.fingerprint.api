using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using System;
using System.Data.SqlClient;

namespace Device.FingerPrint.Core
{
    public class LogService
    {
        private readonly ILogger _logger;

        public LogService(ILogger logger)
        {
            _logger = logger;
        }


        public void WriteError(string message)
        {
            _logger.Error(message);
        }

        public void WriteError(Exception ex)
        {
            var logMessage = new LogMessage(ex.Message)
               .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
               .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
               .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
               .ToString();

            _logger.Error(logMessage);
        }

        public void WriteError(string message, Exception ex)
        {
            message += "\r\n" + ex.Message;
            var logMessage = new LogMessage(message)
               .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
               .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
               .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
               .ToString();

            _logger.Error(logMessage);
        }


        public void WriteInfo(string message)
        {
            _logger.Info(message);
        }

        public void WriteInfo(Exception ex)
        {
            var logMessage = new LogMessage(ex.Message)
               .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
               .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
               .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
               .ToString();

            _logger.Info(logMessage);
        }

        public void WriteInfo(string message, Exception ex)
        {
            message += "\r\n" + ex.Message;
            var logMessage = new LogMessage(message)
               .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
               .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
               .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
               .ToString();

            _logger.Info(logMessage);
        }


        public void WriteWarning(string message)
        {
            _logger.Warn(message);
        }

        public void WriteWarning(Exception ex)
        {
            var logMessage = new LogMessage(ex.Message)
               .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
               .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
               .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
               .ToString();

            _logger.Warn(logMessage);
        }

        public void WriteWarning(string message, Exception ex)
        {
            message += "\r\n" + ex.Message;
            var logMessage = new LogMessage(message)
               .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
               .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
               .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
               .ToString();

            _logger.Warn(logMessage);
        }
    }
}