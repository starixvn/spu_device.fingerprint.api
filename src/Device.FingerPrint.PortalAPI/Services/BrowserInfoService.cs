using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using System;
using System.Collections.Generic;

namespace Device.FingerPrint.PortalAPI.Services
{
    public interface IBrowserInfoService
    {
        (BrowserInfoModel, BrowserInfoAttributeModel) GetBrowserInfo(HeaderModel headerModel, BrowserDetectModel browserDetectModel);
    }

    public class BrowserInfoService : IBrowserInfoService
    {
        public BrowserInfoService()
        {
        }

        public (BrowserInfoModel, BrowserInfoAttributeModel) GetBrowserInfo(HeaderModel headerModel, BrowserDetectModel browserDetectModel)
        {
            var browsers = new List<string> { "Safari", "CocCoc", "Opera", "Microsoft Edge", "Chrome", "Other" };
            var secChUa = headerModel.secChUa;
            var secChUaPlatform = headerModel.secChUaPlatform;
            var secChUaMobile = headerModel.secChUaMobile;

            var browserName = secChUa;
            var osName = secChUaPlatform;

            if (browserName == "Other" || String.IsNullOrEmpty(browserName))
            {
                if (browserDetectModel != null)
                {
                    browserName = browserDetectModel.BrowserName;
                    osName = browserDetectModel.OSName;
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
                OSName = ReplaceOSName(osName)
            };

            var browserInfoAttributeModel = new BrowserInfoAttributeModel
            {
                SecChUa = secChUa,
                SecChUaPlatform = secChUaPlatform,
                SecChUaMobile = secChUaMobile.Replace("?", ""),
                JsBrowserName = browserDetectModel.BrowserName,
                JsOSName = browserDetectModel.OSName,
            };

            return (browserInfoModel, browserInfoAttributeModel);
        }

        private string ReplaceOSName(string osName)
        {
            return string.IsNullOrEmpty(osName) ? "" : osName.Replace("macOS", "Mac");
        }
    }
}