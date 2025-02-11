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
using System.Transactions;
using EasyNetQ;
using System.Collections.Concurrent;
using Fanex.Logging.Extensions.AspNetCore.UserAgent;
using FileHelpers.Converters;
using System.Security.Policy;

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
            using var client = new TcpClient(appSettings.LogstashTcpHostname, appSettings.LogstashTcpPort);
            using var stream = client.GetStream();

            foreach (var transaction in transactions)
            {
                if (appSettings.LogstashIgnoreSubscribers.Count != 0 && appSettings.LogstashIgnoreSubscribers.Contains(transaction.SubscriberName.ToLower()))
                    continue;

                var logMessage = TransformLogMessage(transaction);

                string jsonLog = JsonConvert.SerializeObject(logMessage);

                byte[] data = Encoding.UTF8.GetBytes(jsonLog + "\n");
                await stream.WriteAsync(data);
            }
        }

        private TransactionLogstashModel TransformLogMessage(RawTransactionModel transaction)
        {
            var fingerprintLogstashModel = GetFingerprintAttributes(transaction.FingerprintAttribute?.FPFingerPrintAttribute);

            var transactionLogsTash = TransformRawTransactionLogstash(transaction);

            transactionLogsTash.SubscriberName = transactionLogsTash.SubscriberName.ToLower();

            var logMessage = new TransactionLogstashModel
            {
                Message = transactionLogsTash,
                Tags = "LoginTransaction",
                FingerprintAttributes = fingerprintLogstashModel?.FingerprintAttributes,
                UnhandledAttributes = fingerprintLogstashModel?.UnhandledAttributes,
            };

            return logMessage;
        }

        public RawTransactionLogstashModel TransformRawTransactionLogstash(RawTransactionModel transaction)
        {
            try
            {
                var rawTransModel = new RawTransactionLogstashModel();

                foreach (var property in typeof(RawTransactionModel).GetProperties())
                {
                    var targetProperty = typeof(RawTransactionLogstashModel).GetProperty(property.Name);
                    if (targetProperty != null)
                    {
                        targetProperty.SetValue(rawTransModel, property.GetValue(transaction));
                    }
                }

                if (transaction.FingerprintAttribute != null)
                {
                    rawTransModel.FPFingerPrintExtraAttribute = transaction.FingerprintAttribute.FPFingerPrintExtraAttribute;
                    rawTransModel.FPFingerPrintAttribute = transaction.FingerprintAttribute.FPFingerPrintAttribute;
                }

                if (transaction.UserAgentInfo != null)
                {
                    rawTransModel.ClientName = transaction.UserAgentInfo.ClientName;
                    rawTransModel.ClientVersion = transaction.UserAgentInfo.ClientVersion;
                    rawTransModel.OSName = transaction.UserAgentInfo.OSName;
                    rawTransModel.OSShortName = transaction.UserAgentInfo.OSShortName;
                    rawTransModel.OSVersion = transaction.UserAgentInfo.OSVersion;
                    rawTransModel.OSPlatform = transaction.UserAgentInfo.OSPlatform;
                    rawTransModel.Device = transaction.UserAgentInfo.Device;
                }

                if (transaction.RawJsChallengeInfo != null)
                {
                    rawTransModel.ChallengeIds = transaction.RawJsChallengeInfo.ChallengeIds;
                    rawTransModel.Parameters = transaction.RawJsChallengeInfo.Parameters;
                    rawTransModel.Expect = transaction.RawJsChallengeInfo.Expect;
                    rawTransModel.Expected = transaction.RawJsChallengeInfo.Expected;
                    rawTransModel.Actual = transaction.RawJsChallengeInfo.Actual;
                    rawTransModel.ExeServerTimeInSecond = transaction.RawJsChallengeInfo.ExeServerTimeInSecond;
                    rawTransModel.ExeClientTimeInSecond = transaction.RawJsChallengeInfo.ExeClientTimeInSecond;
                    rawTransModel.Result = transaction.RawJsChallengeInfo.Result;
                    rawTransModel.ErrMsg = transaction.RawJsChallengeInfo.ErrMsg;
                }

                return rawTransModel;
            }
            catch (Exception ex)
            {
                _ = LogService.ErrorAsync($"{appSettings.Environment} - TransformRawTransactionLogstash has error:\n {ex.Message}");
                return null;
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
