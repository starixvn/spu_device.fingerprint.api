using Device.FingerPrint.Core;
using Device.FingerPrint.Model;
using Device.FingerPrint.QueueConsumer.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Device.FingerPrint.QueueConsumer.Services
{
    public interface ILogstashService
    {
        Task PushLog(RawTransactionModel transaction);
    }

    public class LogstashService : ILogstashService
    {
        private readonly IAppSettings appSettings;

        public LogstashService(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public async Task PushLog(RawTransactionModel transaction)
        {
            PushLogUsingTCP(transaction);
            await PushLogUsingHttp(transaction);
        }

        public void PushLogUsingTCP(RawTransactionModel transaction)
        {
            if (string.IsNullOrEmpty(appSettings.LogstashTcpHostname))
                return;

            using (TcpClient client = new TcpClient(appSettings.LogstashTcpHostname, appSettings.LogstashTcpPort))
            {
                var logMessage = new TransactionLogstashModel
                {
                    Message = transaction,
                    Tags = "LoginTransaction",
                    FingerprintAttributes = GetHandledFingerprintAttributes(transaction.FingerprintAttribute.FPFingerPrintAttribute),
                    UnhandledAttributes = GetUnhandleAttributes(transaction.FingerprintAttribute.FPFingerPrintAttribute),
                };

                var jsonLogMessage = JsonConvert.SerializeObject(logMessage);

                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(jsonLogMessage);

                stream.Write(data, 0, data.Length);
            }

        }

        public async Task PushLogUsingHttp(RawTransactionModel transaction)
        {
            if(string.IsNullOrEmpty(appSettings.LogstashHttpUrl))
                return;

            using (var httpClient = new HttpClient())
            {
                var logMessage = new TransactionLogstashModel
                {
                    Message = transaction,
                    Tags = "LoginTransaction",
                    FingerprintAttributes = GetHandledFingerprintAttributes(transaction.FingerprintAttribute.FPFingerPrintAttribute),
                    UnhandledAttributes = GetUnhandleAttributes(transaction.FingerprintAttribute.FPFingerPrintAttribute),
                };

                var jsonLogMessage = JsonConvert.SerializeObject(logMessage);

                var httpContent = new StringContent(jsonLogMessage, Encoding.UTF8, "application/json");

                _ = await httpClient.PostAsync(appSettings.LogstashHttpUrl, httpContent);
            }
        }
        public List<string> GetUnhandleAttributes<T>(T fingerprintAttribute)
        {
            var unhandleAttributes = new List<string>();

            if (fingerprintAttribute == null)
                return unhandleAttributes;

            var unhandleValues = appSettings.UnhandledFingerprintAttributeValues;

            if(fingerprintAttribute != null)
            {
                foreach (var property in fingerprintAttribute.GetType().GetProperties())
                {
                    var propertyValue = property.GetValue(fingerprintAttribute);
                    if (propertyValue == null)
                    {
                        unhandleAttributes.Add(property.Name);
                        continue;
                    }

                    if (propertyValue.GetType() == typeof(string))
                    {
                        var stringValue = propertyValue.ToString();
                        if (!string.IsNullOrEmpty(stringValue)
                            && !unhandleValues.Contains(stringValue.Trim('"').ToLower()))
                            continue;

                        unhandleAttributes.Add(property.Name);
                    }
                }
            }

            return unhandleAttributes;
        }

        public List<string> GetHandledFingerprintAttributes<T>(T fingerprintAttribute)
        {
            var attributes = new List<string>();

            if (fingerprintAttribute == null)
                return attributes;

            var unhandleValues = appSettings.UnhandledFingerprintAttributeValues;

            if (fingerprintAttribute != null)
            {
                foreach (var property in fingerprintAttribute.GetType().GetProperties())
                {
                    var propertyValue = property.GetValue(fingerprintAttribute);
                    if (propertyValue == null)
                        continue;

                    if (propertyValue.GetType() == typeof(string))
                    {
                        var stringValue = propertyValue.ToString();
                        if (string.IsNullOrEmpty(stringValue))
                            continue;

                        if (unhandleValues.Contains(stringValue.Trim('"').ToLower()))
                            continue;
                    }

                    attributes.Add(property.Name);
                }
            }

            return attributes;
        }
    }
}
