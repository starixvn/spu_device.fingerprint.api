using FPS.Device.FingerPrint.Model.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;

namespace Fps.Device.FingerPrint.Client.Net.Framework
{
    public interface IFpsService
    {

        bool InsertTransaction(string LoginName, string sessionKey = null, TimeSpan? timeout = null);

        string GenerateSessionKey();

        string GetScriptUrl();
    }
    public class FpsService : IFpsService
    {
        private readonly int subscriberId;
        private readonly string apiUrl;

        public FpsService(int subscriberId, string apiUrl)
        {
            this.subscriberId = subscriberId;
            this.apiUrl = apiUrl;
        }

        public bool InsertTransaction(string LoginName, string sessionKey = null, TimeSpan? timeout = null)
        {
            if (!string.IsNullOrWhiteSpace(LoginName) && subscriberId != 0)
            {
                var url = new Uri(apiUrl).Combine("api/fp/Transaction");
                var SessionKey = sessionKey ?? HttpContext.Current.Request.Form["__fk"].ToString();
                var UserAgent = HttpContext.Current.Request.Headers["User-Agent"].ToString();
                var ClientIp = GetIP();
                var transaction = new TransactionModel
                {
                    LoginName = LoginName,
                    SessionKey = SessionKey,
                    SubscriberId = subscriberId,
                    UserAgent = UserAgent,
                    ClientIp = ClientIp,
                    TransTime = DateTime.Now
                };

                using (var client = new HttpClient())
                {
                    client.Timeout = timeout ?? TimeSpan.FromSeconds(30);
                    var content = JsonContent.Create(transaction);
                    var response = client.PostAsync(url, content).Result;

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

        public string GetScriptUrl()
        {
            var uri = new Uri(apiUrl).Combine("api/fp/scr", subscriberId.ToString());

            return uri.ToString();

        }
    }
}
