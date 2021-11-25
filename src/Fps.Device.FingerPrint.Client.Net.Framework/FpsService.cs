using FPS.Device.FingerPrint.Model.Framework;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;

namespace Fps.Device.FingerPrint.Client.Net.Framework
{
    public interface IFpsService
    {

        Task<bool> InsertTransaction(string url, string LoginName, int SubscriberId, string sessionKey = null, TimeSpan? timeout = null);

        string GenerateSessionKey();
    }
    public class FpsService : IFpsService
    {
        public async Task<bool> InsertTransaction(string url, string LoginName, int SubscriberId, string sessionKey = null, TimeSpan? timeout = null)
        {
            if (!string.IsNullOrWhiteSpace(LoginName) && SubscriberId != 0)
            {
                var apiUrl = $"{url}/api/fp/Transaction";
                var SessionKey = sessionKey ?? HttpContext.Current.Request.Form["__fk"].ToString();
                var UserAgent = HttpContext.Current.Request.Headers["User-Agent"].ToString();
                var ClientIp = GetIP();
                var transaction = new TransactionModel
                {
                    LoginName = LoginName,
                    SessionKey = SessionKey,
                    SubscriberId = SubscriberId,
                    UserAgent = UserAgent,
                    ClientIp = ClientIp,
                    TransTime = DateTime.Now
                };

                using (var client = new HttpClient())
                {
                    client.Timeout = timeout ?? TimeSpan.FromSeconds(30);
                    var content = JsonContent.Create(transaction);
                    var response = await client.PostAsync(apiUrl, content);

                    return response.IsSuccessStatusCode;
                }
            }

            return false;
        }

        private string GetIP()
        {
            String ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
        }

        public string GenerateSessionKey()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
