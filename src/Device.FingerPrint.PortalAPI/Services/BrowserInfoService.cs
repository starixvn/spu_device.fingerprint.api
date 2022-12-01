using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.PortalAPI.Services
{
    public interface IBrowserInfoService
    {
        (BrowserInfoModel, BrowserInfoAttributeModel) GetBrowserInfo(HeaderModel headerModel, BrowserDetectModel browserDetectModel);

        int GetFlagged(BrowserInfoModel model);
    }

    public class BrowserInfoService : IBrowserInfoService
    {
        public (BrowserInfoModel, BrowserInfoAttributeModel) GetBrowserInfo(HeaderModel headerModel, BrowserDetectModel browserDetectModel)
        {
            var browsers = new List<string> { "Safari", "CocCoc", "Opera", "Microsoft Edge", "Chrome", "Other" };
            string secChUa, secChUaPlatform, secChUaMobile, jsBrowserName, jsOSName, browserName, osName;
            secChUa = secChUaPlatform = secChUaMobile = jsBrowserName = jsOSName = browserName = osName = string.Empty;

            if (headerModel != null && browserDetectModel != null)
            {
                secChUa = headerModel.SecChUa;
                secChUaPlatform = headerModel.SecChUaPlatform;
                secChUaMobile = headerModel.SecChUaMobile;

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
                browserName = headerModel.SecChUa;
                osName = headerModel.SecChUaPlatform;

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
            else
            {
                return (new BrowserInfoModel(), new BrowserInfoAttributeModel());
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
        
        public int GetFlagged(BrowserInfoModel browserInfo)
        {
            if (browserInfo.HeadlessScore > (Decimal)0.5)
            {
                return (int)FlaggedStatus.bot_headless;
            }
            return (int)FlaggedStatus.human;
        }

        private IEnumerable<BrowserInfoDetectionModel> ExecuteRules(IEnumerable<BrowserInfoDetectionModel> browserInfos, int detectionLevel)
        {
            if (browserInfos != null && browserInfos.Any())
            {
                var browserInfosList = browserInfos.ToList();

                foreach (var browserInfo in browserInfosList)
                {
                    var headlessScore = TransformRobotDetectionDetail(browserInfo.HeadlessScore);
                    if (headlessScore.Score > 0.5)
                    {
                        browserInfo.Flagged = (int)FlaggedStatus.bot_headless;
                    }
                    else
                    {
                        if (detectionLevel > 1 && !CompareOSName(browserInfo))
                        {
                            browserInfo.Flagged = (int)FlaggedStatus.bot_fake_os;
                        }
                        else if (detectionLevel > 1 && !CompareBrowserName(browserInfo))
                        {
                            browserInfo.Flagged = (int)FlaggedStatus.bot_fake_browser;
                        }
                        else
                        {
                            browserInfo.Flagged = (int)FlaggedStatus.human;
                        }
                    }
                }

                return browserInfosList;
            }

            return new List<BrowserInfoDetectionModel>();
        }

        private RobotDetectionDetail TransformRobotDetectionDetail(string robotDetectDetail)
        {
            return JsonConvert.DeserializeObject<RobotDetectionDetail>(robotDetectDetail);
        }


        private bool CompareOSName(BrowserInfoDetectionModel browserInfo)
        {
            string[] arrOSName = { "Mac", "Windows", "iOS", "Android" };

            var osName = SpecialCaseOSName(browserInfo.OSName);
            var uaosName = SpecialCaseOSName(browserInfo.UAOSName);

            if (!arrOSName.Contains(uaosName)
                || (osName == uaosName))
            {
                return true;
            }

            return false;
        }

        private string SpecialCaseOSName(string osName)
        {
            if (string.IsNullOrEmpty(osName))
                return string.Empty;

            if (osName == "GNU/Linux")
            {
                return "Linux";
            }

            return osName;
        }

        private bool CompareBrowserName(BrowserInfoDetectionModel browserInfo)
        {
            if (browserInfo.UAOSName == "iOS" && browserInfo.UAOSName == browserInfo.OSName) return true;

            string[] arrBrowserNames = { "Chrome", "Firefox", "Microsoft Edge", "Opera", "Safari" };

            var maxLengthBrowserName = 14;
            var browserName = SpecialCaseBrowserName(browserInfo.BrowserName);
            var uaBrowserName = SpecialCaseBrowserName(browserInfo.UABrowserName);

            if ((browserName.Length > maxLengthBrowserName)
                || (!arrBrowserNames.Contains(uaBrowserName))
                || (browserName == uaBrowserName))
            {
                return true;
            }

            return false;
        }

        private string SpecialCaseBrowserName(string browserName)
        {
            if (string.IsNullOrEmpty(browserName))
                return string.Empty;

            if (browserName == "CocCoc")
            {
                return "Chrome";
            }

            return browserName.Replace(" Mobile", "");
        }

    }
}