using Device.Fingerprint.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Device.Fingerprint.WebAPI.Services
{
    public class BrowserInfo
    {

        public (string, string) GetIPAsync()
        {
            var nodeAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            var nodeAddressV4 = nodeAddresses.FirstOrDefault(_ => _.AddressFamily == AddressFamily.InterNetwork);
            var nodeAddressV6 = nodeAddresses.FirstOrDefault(_ => _.AddressFamily == AddressFamily.InterNetworkV6);

            return (nodeAddressV4 != null ? nodeAddressV4.ToString() : "", nodeAddressV6 != null ? nodeAddressV6.ToString() : "");
        }

        public (BrowserInfoModel, BrowserInfoAttributeModel) GetBrowserInfo(HttpContext httpContext, BrowserInfoJsonModel browserInfoJsonModel, ISentryClient sentryClient)
        {
            var browsers = new List<string> { "Safari", "CocCoc", "Opera", "Microsoft Edge", "Chrome", "Other" };
            var secChUa = httpContext.Request.Headers["sec-ch-ua"].Count > 0 ? httpContext.Request.Headers["sec-ch-ua"].ToString() : "Other";
            var secChUaPlatform = httpContext.Request.Headers["sec-ch-ua-platform"].Count > 0 ? httpContext.Request.Headers["sec-ch-ua-platform"].ToString().Replace("\"", "") : "";
            var secChUaMobile = httpContext.Request.Headers["sec-ch-ua-mobile"].Count > 0 ? httpContext.Request.Headers["sec-ch-ua-mobile"].ToString() : "";

            var browserName = secChUa;
            var osName = secChUaPlatform;

            if (browserName == "Other" || String.IsNullOrEmpty(browserName))
            {
                if (!String.IsNullOrEmpty(browserInfoJsonModel.BrowserInfo))
                {
                    try
                    {
                        var objBrowserInfoJs = Newtonsoft.Json.JsonConvert.DeserializeObject<BrowserInfoJsModel>(browserInfoJsonModel.BrowserInfo);
                        browserName = objBrowserInfoJs.BrowserName;
                        osName = objBrowserInfoJs.OSName;
                    }
                    catch (Exception ex)
                    {
                        sentryClient.CaptureMessage($"GetBrowserInfo error. ({ex.Message})", SentryLevel.Error);
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
                BrowserName = browserName,
                OSName = osName
            };

            var browserInfoJsModel = JsonConvert.DeserializeObject<BrowserInfoJsModel>(browserInfoJsonModel.BrowserInfo);

            var browserInfoAttributeModel = new BrowserInfoAttributeModel
            {
                SecChUa = secChUa,
                SecChUaPlatform = secChUaPlatform,
                SecChUaMobile = secChUaMobile.Replace("?", ""),
                JsBrowserName = browserInfoJsModel.BrowserName,
                JsOSName = browserInfoJsModel.OSName,
            };

            return (browserInfoModel, browserInfoAttributeModel);
        }
    }
}
