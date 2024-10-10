using Device.FingerPrint.Core;
using Device.FingerPrint.Model;
using Device.FingerPrint.QueueConsumer.Models;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Fanex.Logging;

namespace Device.FingerPrint.QueueConsumer.Services
{
    public interface ILogstashService
    {
        Task PushLog(RawTransactionModel transaction);
    }

    public class LogstashService : ILogstashService
    {
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;

        public LogstashService(IAppSettings appSettings, ILogger logger)
        {
            this.appSettings = appSettings;
            this.logger = logger;
        }

        public async Task PushLog(RawTransactionModel transaction)
        {
            var logMessage = TransformLogMessage(transaction);

            PushLogUsingTCP(logMessage);
            await PushLogUsingHttp(logMessage);
        }

        private string TransformLogMessage(RawTransactionModel transaction)
        {
            var fingerprintLogstashModel = GetFingerprintAttributes(transaction.FingerprintAttribute?.FPFingerPrintAttribute);

            var logMessage = new TransactionLogstashModel
            {
                Message = transaction,
                Tags = "LoginTransaction",
                FingerprintAttributes = fingerprintLogstashModel?.FingerprintAttributes,
                UnhandledAttributes = fingerprintLogstashModel?.UnhandledAttributes,
            };

            return JsonConvert.SerializeObject(logMessage);
        }

        private void PushLogUsingTCP(string logMessage)
        {
            if (string.IsNullOrEmpty(appSettings.LogstashTcpHostname))
                return;

            try
            {
                using (TcpClient client = new TcpClient(appSettings.LogstashTcpHostname, appSettings.LogstashTcpPort))
                {
                    NetworkStream stream = client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(logMessage);

                    stream.Write(data, 0, data.Length);
                }
            } 
            catch(Exception ex)
            {
                LogException(ex);
            }
            
        }

        private async Task PushLogUsingHttp(string logMessage)
        {
            if(string.IsNullOrEmpty(appSettings.LogstashHttpUrl))
                return;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var httpContent = new StringContent(logMessage, Encoding.UTF8, "application/json");

                    _ = await httpClient.PostAsync(appSettings.LogstashHttpUrl, httpContent);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private FingerprintLogstashModel GetFingerprintAttributes<T>(T fingerprintAttribute)
        {
            var attributes = new List<string>();
            var unhandledAttributes = new List<string>();

            if (fingerprintAttribute == null)
                return null;

            var unhandledValues = appSettings.UnhandledFingerprintAttributeValues;

            foreach (var property in fingerprintAttribute.GetType().GetProperties())
            {
                var propertyValue = property.GetValue(fingerprintAttribute);

                if (propertyValue == null)
                {
                    unhandledAttributes.Add(property.Name);
                    continue;
                }

                if (propertyValue.GetType() == typeof(string))
                {
                    var stringValue = propertyValue.ToString();
                    if (!string.IsNullOrEmpty(stringValue)
                        && !unhandledValues.Contains(stringValue.Trim('"').ToLower()))
                    {
                        attributes.Add(property.Name);
                        continue;
                    }

                    unhandledAttributes.Add(property.Name);
                }
                else
                {
                    attributes.Add(property.Name);
                }
            }

            return new FingerprintLogstashModel
            {
                FingerprintAttributes = attributes,
                UnhandledAttributes = unhandledAttributes
            };
        }

        private void LogException(Exception ex)
        {
            var logMessage = new LogMessage(ex.Message)
                   .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                   .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                   .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                   .ToString();

            logger.Error(logMessage);
        }
    }
}
