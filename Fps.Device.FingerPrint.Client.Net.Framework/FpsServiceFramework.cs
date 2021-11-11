using FPS.Device.FingerPrint.Model;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Fps.Device.FingerPrint.Client.Net.Framework
{
    public interface IFpsServiceFramework
    {

        Task<bool> InsertTransaction(string url, TransactionModel transaction, TimeSpan? timeout = null);
    }
    public class FpsServiceFramework : IFpsServiceFramework
    {
        public async Task<bool> InsertTransaction(string url, TransactionModel transaction, TimeSpan? timeout = null)
        {
            if (transaction != null && !string.IsNullOrWhiteSpace(transaction.LoginName) && transaction.SubscriberId != 0)
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = timeout ?? TimeSpan.FromSeconds(30);

                    var content = JsonContent.Create(transaction);
                    var response = await client.PostAsync(url, content);

                    return response.IsSuccessStatusCode;
                }

            }

            return false;
        }
    }
}
