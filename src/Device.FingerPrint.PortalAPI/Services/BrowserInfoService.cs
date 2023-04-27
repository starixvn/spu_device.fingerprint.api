using Device.FingerPrint.Core;
using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Device.FingerPrint.PortalAPI.Ultilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Device.FingerPrint.PortalAPI.Services
{
    public interface IBrowserInfoService
    {
        BrowserInfoModel GetBrowserInfo(HeaderModel headerModel, BrowserDetectModel browserDetectModel);

        int GetFlagged(BrowserInfoModel model);

        BrowserInfoAttributeModel GetBrowserInfoAttribute(HeaderModel headerModel, BrowserDetectModel browserDetectModel);
    }

    public class BrowserInfoService : IBrowserInfoService
    {
        private readonly ICrypto crypto;        
        public BrowserInfoService(ICrypto crypto)
        {
            this.crypto = crypto;
        }

        public BrowserInfoModel GetBrowserInfo(HeaderModel headerModel, BrowserDetectModel browserDetectModel)
        {
            var browsers = new List<string> { "Safari", "CocCoc", "Opera", "Microsoft Edge", "Chrome", "Other" };

            var browserName = string.Empty;
            var osName = string.Empty;

            if (headerModel != null)
            {
                osName = headerModel.SecChUaPlatform;
                browserName = browsers.Select(x =>
                {
                    if (headerModel.SecChUa.Contains(x)) return x;
                    return headerModel.SecChUa;
                }).FirstOrDefault();
            }
            else
            {
                if (browserDetectModel != null)
                {
                    browserName = browserDetectModel.BrowserName;
                    osName = browserDetectModel.OSName;
                }
            }           

            var browserInfoModel = new BrowserInfoModel
            {
                BrowserName = browserName,
                OSName = string.IsNullOrEmpty(osName) ? "" : osName.Replace("macOS", "Mac")
            };

            browserInfoModel.BrowserInfoCode = HashBrowserInfoCode(browserInfoModel);

            return browserInfoModel;
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

        public BrowserInfoAttributeModel GetBrowserInfoAttribute(HeaderModel headerModel, BrowserDetectModel browserDetectModel)
        {
            var browserInfoAttributeModel =  new BrowserInfoAttributeModel
            {
                SecChUa = headerModel.SecChUa,
                SecChUaPlatform = headerModel.SecChUaPlatform,
                SecChUaMobile = string.IsNullOrEmpty(headerModel.SecChUaMobile) ? "" : headerModel.SecChUaMobile.Replace("?", ""),
                JsBrowserName = browserDetectModel.BrowserName,
                JsOSName = browserDetectModel.OSName
            };

            browserInfoAttributeModel.BrowserInfoAttributeCode = HashBrowserInfoAttributeCode(browserInfoAttributeModel);


            return browserInfoAttributeModel;
        }

        private string HashBrowserInfoCode(BrowserInfoModel model)
        {
            if (model == null) return string.Empty;

            var json = JsonConvert.SerializeObject(new BrowserInfoProp
            {
                OSName = model.OSName,
                BrowserName = model.BrowserName,
                HeadlessScore = model.HeadlessScore,
                HeadlessDetail = model.HeadlessDetail,
                Flagged = model.Flagged
            });

            return crypto.GetMD5Hash(json);
        }

        private string HashBrowserInfoAttributeCode(BrowserInfoAttributeModel model)
        {
            if (model == null) return string.Empty;

            var json = JsonConvert.SerializeObject(new BrowserInfoAttributeProp
            {
                SecChUa = model.SecChUa,
                SecChUaPlatform = model.SecChUaPlatform,
                SecChUaMobile = model.SecChUaMobile,
                JsBrowserName = model.JsBrowserName,
                JsOSName = model.JsOSName
            });

            return crypto.GetMD5Hash(json);
        }

        
    }
}