using FPS.Device.FingerPrint.Net.Framework.Client.Model;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Web;

namespace Fps.Device.FingerPrint.Net.Framework.Client
{
    public interface IFpsService
    {

        bool InsertTransaction(string action, string loginName, long custId = 0, long subAccId = 0, string transKey = null, TimeSpan? timeout = null);               

        string GetScriptUrl();
    }

    public class FpsService : IFpsService
    {
        private readonly int subscriberId;
        private readonly string apiUrl;
        private readonly string sessionKey;

        public FpsService(int subscriberId, string apiUrl)
        {
            this.subscriberId = subscriberId;
            this.apiUrl = apiUrl;
            this.sessionKey = Guid.NewGuid().ToString("N");
        }

        public bool InsertTransaction(string action, string loginName, long custId = 0, long subAccId = 0, string transKey = null, TimeSpan? timeout = null)
        {            
            if (!string.IsNullOrWhiteSpace(loginName) && subscriberId != 0)
            {                
                var url = "http:" + GetUrl(apiUrl, "api/fp/Transaction");
                                
                var UserAgent = HttpContext.Current.Request.Headers["User-Agent"].ToString();
                var ClientIp = GetIP();
                var transaction = new TransactionModel
                {
                    LoginName = loginName,
                    SessionKey = sessionKey,
                    SubscriberId = subscriberId,
                    UserAgent = UserAgent,
                    ClientIp = ClientIp,
                    TransTime = DateTime.Now,
                    Url = HttpContext.Current.Request.Url.AbsoluteUri,
                    Action = action,
                    TransKey = transKey,
                    CustId = custId,
                    SubAccId = subAccId
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
        private string GetUrl(string baseUrl, string relativeUrl)
        {
            var url = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";

            return url + relativeUrl;
        }

        private string GetIP()
        {
            var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
        }

        public string GetScriptUrl()
        {
            return GetUrl(apiUrl, $"api/fp/scr/{sessionKey}/{subscriberId}");
        }
    }
}
