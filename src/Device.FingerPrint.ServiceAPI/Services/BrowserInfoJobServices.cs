using Device.FingerPrint.Features.BrowserInfo.Model;
using Device.FingerPrint.Features.BrowserInfo.Services;
using Device.FingerPrint.Features.Offset.Services;
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
        public void DetectBots(int batchSize, int numberOfBatch, int detectionLevel);
    }

    public class BrowserInfoJobServices : IBrowserInfoJobServices
    {
        private readonly IBrowserInfoService browserInfoService;
        private readonly IRawUserAgentJobServices rawUserAgentJobServices;
        private readonly IOffsetService offsetService;

        public BrowserInfoJobServices(IBrowserInfoService browserInfoService, IRawUserAgentJobServices rawUserAgentJobServices, IOffsetService offsetService)
        {
            this.browserInfoService = browserInfoService;
            this.rawUserAgentJobServices = rawUserAgentJobServices;
            this.offsetService = offsetService;
        }

        public void DetectBots(int batchSize, int numberOfBatch, int detectionLevel)
        {
            int offsetId = (int)Offset.UpdateBrowserInfo;
            var browserInfos = browserInfoService.GetBrowserInfo(batchSize * numberOfBatch, offsetId);

            if (browserInfos != null && browserInfos.Any())
            {
                var offsetValue = browserInfos.Min(x => x.Id);
                var browserInfoModel = TransformBrowserInfo(browserInfos);

                var listTask = new List<Task>
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            if (browserInfoModel != null && browserInfoModel.Any())
                            {
                                var browserInfoExecuted = ExecuteRules(browserInfoModel, detectionLevel);

                                UpdateFlagged(browserInfoExecuted, batchSize);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogService.Error(ex.Message, ex);
                        }
                    }),

                    Task.Run(() =>
                    {
                        try
                        {
                            offsetService.UpdateOffset(offsetId, offsetValue);
                        }
                        catch (Exception ex)
                        {
                            LogService.Error(ex.Message, ex);
                        }
                    })
                };

                Task.WaitAll(listTask.ToArray());
            }
        }

        private IEnumerable<BrowserInfoDetectionModel> TransformData(IEnumerable<BrowserInfoDetectionModel> browserInfoObject)
        {
            return browserInfoObject.Select(x => new BrowserInfoDetectionModel
            {
                Id = x.Id,
                UserAgentId = x.UserAgentId,
                UserAgent = x.UserAgent,
                UABrowserName = x.UABrowserName,
                UAOSName = x.UAOSName,
                HeadlessScore = x.HeadlessScore,
                BrowserName = x.BrowserName,
                OSName = x.OSName
            });
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
                        browserInfo.Flagged = FlaggedStatus.bot.ToString();
                        browserInfo.Reason = BrowserInfoReason.HeadlessScore.ToString();
                    }
                    else
                    {
                        if (detectionLevel > 1 && !CompareOSName(browserInfo))
                        {
                            browserInfo.Flagged = FlaggedStatus.bot.ToString();
                            browserInfo.Reason = BrowserInfoReason.OSName.ToString();
                        }
                        else if (detectionLevel > 1 && !CompareBrowserName(browserInfo))
                        {
                            browserInfo.Flagged = FlaggedStatus.bot.ToString();
                            browserInfo.Reason = BrowserInfoReason.BrowserName.ToString();
                        }
                        else
                        {
                            browserInfo.Flagged = FlaggedStatus.human.ToString();
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
            if (browserInfo.OSName == browserInfo.UAOSName)
            {
                return true;
            }
            return false;
        }

        private bool CompareBrowserName(BrowserInfoDetectionModel browserInfo)
        {
            if (browserInfo.UAOSName == "iOS" && browserInfo.UAOSName == browserInfo.OSName) return true;

            string[] arrBrowserNames = { "Chrome", "Firefox", "Microsoft Edge", "Opera", "Safari" };
            var browserName = SpecialCaseBrowserName(browserInfo.BrowserName);
            var uaBrowserName = SpecialCaseBrowserName(browserInfo.UABrowserName);

            if (!arrBrowserNames.Contains(uaBrowserName) && !arrBrowserNames.Contains(browserName))
            {
                return true;
            }

            if (browserName == uaBrowserName)
            {
                return true;
            }

            return false;
        }

        private string SpecialCaseBrowserName(string browserName)
        {
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
                        Flagged = browserInfo.Flagged,
                        Reason = browserInfo.Reason
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
                            LogService.Error(ex.Message + "\n" + JsonConvert.SerializeObject(item), ex);
                            throw;
                        }
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

    }
}
