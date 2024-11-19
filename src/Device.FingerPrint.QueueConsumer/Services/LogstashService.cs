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
using System.Linq;

namespace Device.FingerPrint.QueueConsumer.Services
{
    public interface ILogstashService
    {
        Task PushLogAsync(IEnumerable<RawTransactionModel> transactions);
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
        
        public async Task PushLogAsync(IEnumerable<RawTransactionModel> transactions)
        {
            transactions = transactions.Where(it => appSettings.LogstashAllowSubscribers.Contains(it.SubscriberName.ToLower()));

            if (transactions.Count() == 0)
                return;

            foreach (var transaction in transactions)
            {
                using var client = new TcpClient(appSettings.LogstashTcpHostname, appSettings.LogstashTcpPort);
                using var stream = client.GetStream();

                var logMessage = TransformLogMessage(transaction);

                byte[] data = Encoding.UTF8.GetBytes(logMessage);
                await stream.WriteAsync(data, 0, data.Length);
            }
        }

        private string TransformLogMessage(RawTransactionModel transaction)
        {
            var fingerprintLogstashModel = GetFingerprintAttributes(transaction.FingerprintAttribute?.FPFingerPrintAttribute);

            transaction.SubscriberName = transaction.SubscriberName.ToLower();

            var logMessage = new TransactionLogstashModel
            {
                Message = transaction,
                Tags = "LoginTransaction",
                FingerprintAttributes = fingerprintLogstashModel?.FingerprintAttributes,
                UnhandledAttributes = fingerprintLogstashModel?.UnhandledAttributes,
            };

            return JsonConvert.SerializeObject(logMessage);
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
