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
            string secChUa, secChUaPlatform, secChUaMobile, jsBrowserName, jsOSName, browserName, osName;
            secChUa = secChUaPlatform = secChUaMobile = jsBrowserName = jsOSName = browserName = osName = string.Empty;

            if (headerModel != null && browserDetectModel != null)
            {
                secChUa = headerModel.secChUa;
                secChUaPlatform = headerModel.secChUaPlatform;
                secChUaMobile = headerModel.secChUaMobile;

                jsBrowserName = browserDetectModel.BrowserName;
                jsOSName = browserDetectModel.OSName;

                browserName = secChUa;
                osName = secChUaPlatform;

                if (String.IsNullOrEmpty(secChUa))
                {
                    browserName = jsBrowserName;
                    osName = jsOSName;
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
            }
            else if (headerModel != null)
            {
                browserName = headerModel.secChUa;
                osName = headerModel.secChUaPlatform;

                if (!String.IsNullOrEmpty(secChUa))
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
            }
            else if (browserDetectModel != null)
            {
                browserName = jsBrowserName;
                osName = jsOSName;
            }

            var browserInfoModel = new BrowserInfoModel
            {
                BrowserName = browserName,
                OSName = string.IsNullOrEmpty(osName) ? "" : osName.Replace("macOS", "Mac")
            };

            var browserInfoAttributeModel = new BrowserInfoAttributeModel
            {
                SecChUa = secChUa,
                SecChUaPlatform = secChUaPlatform,
                SecChUaMobile = string.IsNullOrEmpty(secChUaMobile) ? "" : secChUaMobile.Replace("?", ""),
                JsBrowserName = jsBrowserName,
                JsOSName = jsOSName
            };

            return (browserInfoModel, browserInfoAttributeModel);
        }
    }
}