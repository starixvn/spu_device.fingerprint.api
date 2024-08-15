using Device.FingerPrint.Core;
using Device.FingerPrint.Model;
using Device.FingerPrint.QueueConsumer.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
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
            using (var httpClient = new HttpClient())
            {
                var logMessage = new TransactionLogstashModel
                {
                    Message = transaction,
                    Tags = "LoginTransaction",
                };

                var unhandleFingerprintAttributes = new List<string>();

                if (transaction.FingerprintAttribute != null)
                {
                    unhandleFingerprintAttributes.AddRange(GetUnhandleAttributes(transaction.FingerprintAttribute.FPFingerPrintAttribute));
                    unhandleFingerprintAttributes.AddRange(GetUnhandleAttributes(transaction.FingerprintAttribute.FPFingerPrintExtraAttribute));
                }

                logMessage.UnhandleAttributes = unhandleFingerprintAttributes;

                var jsonLogMessage = JsonConvert.SerializeObject(logMessage);

                var httpContent = new StringContent(jsonLogMessage, Encoding.UTF8, "application/json");

                _ = await httpClient.PostAsync(appSettings.LogstashUrl, httpContent);
            }
        }

        public List<string> GetUnhandleAttributes<T>(T fingerprintAttribute)
        {
            var unhandleAttributes = new List<string>();

            if (fingerprintAttribute == null)
                return unhandleAttributes;

            var unhandleValues = appSettings.UnhandleFingerprintAttributeValues;

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
    }
}
