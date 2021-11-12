using FPS.Device.FingerPrint.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Fps.Device.FingerPrint.Client.Net.Core
{
    public interface IFpsService
    {

        Task<bool> InsertTransaction(HttpContext context, string url, string LoginName, int SubscriberId, TimeSpan? timeout = null);
    }
    public class FpsService : IFpsService
    {
        public async Task<bool> InsertTransaction(HttpContext context, string url, string LoginName, int SubscriberId, TimeSpan? timeout = null)
        {
            if(!string.IsNullOrWhiteSpace(LoginName) && SubscriberId != 0)
            {
                var SessionKey = context.Request.Form["__fk"].ToString();
                var UserAgent = context.Request.Headers["User-Agent"].ToString();
                var ClientIp = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                TransactionModel transaction = new()
                {
                    LoginName = LoginName,
                    SessionKey = SessionKey,
                    SubscriberId = SubscriberId,
                    UserAgent = UserAgent,
                    ClientIp = ClientIp
                };
                if (transaction != null)
                {
                    using var client = new HttpClient();
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
