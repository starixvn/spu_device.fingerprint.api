using Device.Fingerprint.WebAPI.Models;
using Fanex.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Device.Fingerprint.WebAPI.Services
{
    public class BrowserInfo
    {

        public string GetIP()
        {
            var hostName = Dns.GetHostName();
            var nodeAddresses = Dns.GetHostAddresses(hostName);
            var nodeAddressV4 = nodeAddresses.FirstOrDefault(_ => _.AddressFamily == AddressFamily.InterNetwork);

            return nodeAddressV4 != null ? nodeAddressV4.ToString() : "";
        }

        public (string, string) GetIPAsync()
        {
            var nodeAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            var nodeAddressV4 = nodeAddresses.FirstOrDefault(_ => _.AddressFamily == AddressFamily.InterNetwork);
            var nodeAddressV6 = nodeAddresses.FirstOrDefault(_ => _.AddressFamily == AddressFamily.InterNetworkV6);

            return (nodeAddressV4 != null ? nodeAddressV4.ToString() : "", nodeAddressV6 != null ? nodeAddressV6.ToString() : "");
        }

        public BrowserInfoModel GetBrowserInfo(HttpContext httpContext, string browserInfoJs)
        {
            var browsers = new List<string> { "CocCoc", "Opera", "Chrome", "Microsoft Edge", "Other" };
            var secChUa = httpContext.Request.Headers["sec-ch-ua"].Count > 0 ? httpContext.Request.Headers["sec-ch-ua"].ToString() : "Other";
            var browserName = secChUa;
            var osName = httpContext.Request.Headers["sec-ch-ua-platform"].ToString().Replace("\"", "");

            if (browserName == "Other" || String.IsNullOrEmpty(browserName))
            {
                if (!String.IsNullOrEmpty(browserInfoJs))
                {
                    try
                    {
                        var objBrowserInfoJs = Newtonsoft.Json.JsonConvert.DeserializeObject<BrowserInfoJs>(browserInfoJs);
                        browserName = objBrowserInfoJs.BrowserName;
                        osName = objBrowserInfoJs.OSName;
                    }
                    catch (Exception ex)
                    {
                        LogService.Error($"GetBrowserInfo error. ({ex.Message})", ex);
                    }
                }
            }
            else
            {
                foreach (var browser in browsers)
                {
                    if (secChUa.Contains(browser))
                    {
                        browserName = browser;
                        break;
                    }
                }
            }

            return new BrowserInfoModel
            {
                ClientIp = GetIP(),
                BrowserName = browserName,
                OSName = osName
            };
        }

        private class BrowserInfoJs
        {
            public string ClientIp { get; set; }
            public string BrowserName { get; set; }
            public string OSName { get; set; }
        }
    }
}
