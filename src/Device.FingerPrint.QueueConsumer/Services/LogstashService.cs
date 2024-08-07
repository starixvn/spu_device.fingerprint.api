using Device.FingerPrint.Core;
using Device.FingerPrint.Model;
using Device.FingerPrint.QueueConsumer.Models;
using Newtonsoft.Json;
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
                    Tags = "LoginTransaction"
                };

                var jsonLogMessage = JsonConvert.SerializeObject(logMessage);

                var httpContent = new StringContent(jsonLogMessage, Encoding.UTF8, "application/json");

                _ = await httpClient.PostAsync(appSettings.LogstashUrl, httpContent);
            }
        }
    }
}
