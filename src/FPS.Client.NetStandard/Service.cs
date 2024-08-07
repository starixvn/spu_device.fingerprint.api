using FPS.Client.NetStandard.Models;
using FPS.Client.NetStandard.Settings;
using FPS.Client.NetStandard.Utils;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FPS.Client.NetStandard
{
    public class Service
    {
        string UrlAPI { get; set; }

        HttpRequest HttpRequest { get; set; }

        public Service InitialRequest(string urlAPI, HttpRequest httpRequest)
        {
            UrlAPI = urlAPI;
            HttpRequest = httpRequest;

            return this;
        }

        private string GetHeaderIP(string param)
        {
            if (HttpRequest.Headers.TryGetValue(param, out var xRemoteAddVal))
            {
                return xRemoteAddVal.ToString();
            }

            return string.Empty;
        }
        private string GetXForWarded()
        {
            var proxyIP = GetHeaderIP("X-Forwarded-For");

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Originating-IP");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Remote-IP");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Remote-Addr");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = HttpRequest.HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            return proxyIP;
        }

        private string GetRealIp()
        {
            var proxyIP = GetHeaderIP("X-Actual-IP");

            if (string.IsNullOrEmpty(proxyIP))
            {
                var clientIps = GetXForWarded();

                if (!string.IsNullOrEmpty(clientIps))
                {
                    var ips = clientIps.Split(new char[] { ',' });

                    if (ips.Length > 1)
                    {
                        return ips[1];
                    }

                    return ips[0];
                }
                else
                {
                    return HttpRequest.HttpContext.Connection.RemoteIpAddress?.ToString();
                }
            }
            else
            {
                return proxyIP.Split(new char[] { ',' })[0];
            }
        }

        private string GetFakeIp()
        {
            var proxyIP = GetXForWarded();

            if (!string.IsNullOrEmpty(proxyIP))
            {
                var ips = proxyIP.Split(new char[] { ',' });

                if (ips[0] != GetRealIp())
                {
                    return ips[0];
                }
            }

            return string.Empty;
        }

        public Service CheckTransaction(string account, string subscriber, string action, DateTime loginTime)
        {
            if (string.IsNullOrEmpty(account))
                throw new ArgumentNullException(nameof(account));
            if (string.IsNullOrEmpty(subscriber))
                throw new ArgumentNullException(nameof(subscriber));
            if (string.IsNullOrEmpty(action))
                throw new ArgumentNullException(nameof(action));
            if (string.IsNullOrEmpty(UrlAPI))
                throw new ArgumentNullException(nameof(UrlAPI));
            
            var realIP = GetRealIp();
            var fakeIP = GetFakeIp();
            var transactionRequest = new TransactionRequest()
            {
                AcceptsTypes = HttpRequest.Headers["Accept"].ToString() ?? string.Empty,
                UserAgent = HttpRequest.Headers["User-Agent"].ToString() ?? string.Empty,
                Ip = realIP,
                FakeIp = fakeIP,
                Url = HttpRequest.Scheme.ToString() + "://" + HttpRequest.Host.ToString(),
                Di = HttpRequest.Form["__di"].ToString() ?? string.Empty,
                Bi = HttpRequest.Form["__bi"].ToString() ?? string.Empty,
                DetecasAnalysis = HttpRequest.Form["detecas-analysis"].ToString() ?? string.Empty,
                Account = account,
                Action = action,
                ActionResult = "Successful",
                Subscriber = subscriber,                
                LoginTime = loginTime,
                CaptchaResult = string.Empty,
                Token = HttpRequest.Form["__tk"].ToString() ?? string.Empty,
                SessionKey = HttpRequest.Form["__fdi"].ToString() ?? string.Empty
            };

            _ = PostBasicAsync(UrlAPI, transactionRequest, new CancellationToken());

            return this;
        }

        public string GetDeviceID()
        {
            try
            {
                var di = HttpRequest.Form["__di"].ToString() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(di))
                {
                    var decryptedText = Base64.Decode(di);

                    var device = JsonConvert.DeserializeObject<DeviceInfo>(decryptedText, JsonSerializerSettingsFactory.Instance);

                    if (device != null)
                        return device.DeviceId;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private class ResultModel
        {
            public string ResultCode { get; set; }

            public string ResultMessage { get; set; }

            public string DeviceCode { get; set; }
        }        

        private async Task PostBasicAsync(string url, object content, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                var json = JsonConvert.SerializeObject(content);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }


    }
}
