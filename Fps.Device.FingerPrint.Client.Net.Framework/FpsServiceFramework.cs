using FPS.Device.FingerPrint.Model;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Fps.Device.FingerPrint.Client.Net.Framework
{
    public interface IFpsServiceFramework
    {

        Task<bool> InsertTransaction(string urll, Microsoft.AspNetCore.Http.HttpContext context, TimeSpan? timeout = null);
    }
    public class FpsServiceFramework : IFpsServiceFramework
    {
        public async Task<bool> InsertTransaction(string url, Microsoft.AspNetCore.Http.HttpContext context, TimeSpan? timeout = null)
        {
            var LoginName = context.Request.Form["Username"].ToString();
            var SessionKey = context.Request.Form["__fk"].ToString();
            var SubscriberId = context.Items["SubscriberId"].ToString();
            var UserAgent = context.Request.Headers["User-Agent"].ToString();
            var ClientIp = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            TransactionModel transaction = new TransactionModel()
            {
                LoginName = LoginName,
                SessionKey = SessionKey,
                SubscriberId = int.Parse(SubscriberId),
                UserAgent = UserAgent,
                ClientIp = ClientIp
            };
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
