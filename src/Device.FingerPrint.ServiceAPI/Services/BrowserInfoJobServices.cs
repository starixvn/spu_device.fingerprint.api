using Device.FingerPrint.Features.BrowserInfo.Model;
using Device.FingerPrint.Features.BrowserInfo.Services;
using Device.FingerPrint.Model;
using Fanex.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{

    public interface IBrowserInfoJobServices
    {
        Task<bool> DetectBots(int batchSize, int detectionLevel);
    }

    public class BrowserInfoJobServices : IBrowserInfoJobServices
    {
        private readonly IBrowserInfoService browserInfoService;
        private readonly ILogger _logger;

        public BrowserInfoJobServices(IBrowserInfoService browserInfoService, ILogger logger)
        {
            this.browserInfoService = browserInfoService;
            _logger = logger;
        }

        public Task<bool> DetectBots(int batchSize, int detectionLevel)
        {
            var browserInfos = browserInfoService.GetBrowserInfo(batchSize);

            if (browserInfos != null && browserInfos.Any())
            {
                var browserInfoModel = TransformBrowserInfo(browserInfos);

                if (browserInfoModel != null && browserInfoModel.Any())
                {
                    var browserInfoExecuted = ExecuteRules(browserInfoModel, detectionLevel);

                    UpdateFlagged(browserInfoExecuted, batchSize);
                }
            }

            return Task.FromResult(true);
        }

        private IEnumerable<BrowserInfoDetectionModel> TransformBrowserInfo(IEnumerable<BrowserInfoDetectionEntity> browserInfos)
        {
            var listEntity = new List<BrowserInfoDetectionModel>();

            foreach (var browserInfo in browserInfos)
            {
                listEntity.Add(new BrowserInfoDetectionModel
                {
                    Id = browserInfo.Id,
                    UserAgentId = browserInfo.UserAgentId,
                    UserAgent = browserInfo.UserAgent,
                    HeadlessScore = browserInfo.HeadlessScore,
                    BrowserName = browserInfo.BrowserName,
                    OSName = browserInfo.OSName,
                    UABrowserName = browserInfo.UABrowserName,
                    UAOSName = browserInfo.UAOSName
                });
            }

            return listEntity;
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

        private void UpdateFlagged(IEnumerable<BrowserInfoDetectionModel> browserInfos, int batchSize)
        {
            if (browserInfos != null && browserInfos.Any())
            {
                var browserInfoFlaggedList = new List<BrowserInfoFlaggedEntity>();
                foreach (var browserInfo in browserInfos)
                {
                    var model = new BrowserInfoFlaggedEntity
                    {
                        Id = browserInfo.Id,
                        Flagged = browserInfo.Flagged
                    };
                    browserInfoFlaggedList.Add(model);
                }

                var tasks = new List<Task>();
                var chunkData = browserInfoFlaggedList.Chunk2(batchSize);

                foreach (var item in chunkData)
                {
                    var task = Task.Run(() =>
                    {
                        try
                        {
                            browserInfoService.UpdateBrowserInfo(item);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.Message + "\n" + JsonConvert.SerializeObject(item));
                        }
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}
