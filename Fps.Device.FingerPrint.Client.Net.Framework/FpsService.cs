using FPS.Device.FingerPrint.Model;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;

namespace Fps.Device.FingerPrint.Client.Net.Framework
{
    public interface IFpsService
    {

        Task<bool> InsertTransaction(string url, string LoginName, int SubscriberId, TimeSpan? timeout = null);
    }
    public class FpsService : IFpsService
    {
        public async Task<bool> InsertTransaction(string url, string LoginName, int SubscriberId, TimeSpan? timeout = null)
        {
            if (!string.IsNullOrWhiteSpace(LoginName) && SubscriberId != 0)
            {
                var SessionKey = HttpContext.Current.Request.Form["__fk"].ToString();
                var UserAgent = HttpContext.Current.Request.Headers["User-Agent"].ToString();
                var ClientIp = GetIP();
                TransactionModel transaction = new TransactionModel()
                {
                    LoginName = LoginName,
                    SessionKey = SessionKey,
                    SubscriberId = SubscriberId,
                    UserAgent = UserAgent,
                    ClientIp = ClientIp
                };
                if (transaction != null)
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = timeout ?? TimeSpan.FromSeconds(30);

                        var content = JsonContent.Create(transaction);
                        var response = await client.PostAsync(url, content);

                        return response.IsSuccessStatusCode;
                    }

                }
            }

            return false;
        }

        public static String GetIP()
        {
            String ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
        }
    }
}
