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

        public (BrowserInfoModel, BrowserInfoAttributeModel) GetBrowserInfo(HttpContext httpContext, BrowserInfoJsonModel browserInfoJsonModel)
        {
            var browsers = new List<string> { "CocCoc", "Opera", "Google Chrome", "Microsoft Edge", "Chrome", "Other" };
            var secChUa = httpContext.Request.Headers["sec-ch-ua"].Count > 0 ? httpContext.Request.Headers["sec-ch-ua"].ToString() : "Other";
            var browserName = secChUa;
            var osName = httpContext.Request.Headers["sec-ch-ua-platform"].ToString().Replace("\"", "");

            if (browserName == "Other" || String.IsNullOrEmpty(browserName))
            {
                if (!String.IsNullOrEmpty(browserInfoJsonModel.BrowserInfo))
                {
                    try
                    {
                        var objBrowserInfoJs = Newtonsoft.Json.JsonConvert.DeserializeObject<BrowserInfoJs>(browserInfoJsonModel.BrowserInfo);
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

            var browserInfoModel = new BrowserInfoModel
            {
                ClientIp = GetIP(),
                BrowserName = browserName,
                OSName = osName
            };

            var browserInfoAttributeModel = new BrowserInfoAttributeModel
            {
                SecChUa = secChUa,
                BrowserInfoJs = browserInfoJsonModel.BrowserInfo,
                UserAgentData = browserInfoJsonModel.UserAgentData
            };

            return (browserInfoModel, browserInfoAttributeModel);
        }

        private class BrowserInfoJs
        {
            public string ClientIp { get; set; }
            public string BrowserName { get; set; }
            public string OSName { get; set; }
        }
    }
}
