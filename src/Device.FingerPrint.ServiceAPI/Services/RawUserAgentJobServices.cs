using Device.FingerPrint.Features.RawUserAgent.Services;
using Device.FingerPrint.Model;
using DeviceDetectorNET;
using DeviceDetectorNET.Parser;
using Fanex.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IRawUserAgentJobServices
    {
        void UpdateRawUserAgent(int batchSize);

        IEnumerable<BrowserInfoDetectionModel> ParseUserAgentFromBrowserInfo(IEnumerable<BrowserInfoDetectionModel> browserInfos);
    }

    public class RawUserAgentJobServices : IRawUserAgentJobServices
    {
        private readonly IRawUserAgentService rawUserAgentService;
        private readonly int offsetId = (int)Offset.DetectRawUserAgent;

        public RawUserAgentJobServices(IRawUserAgentService rawUserAgentService)
        {
            this.rawUserAgentService = rawUserAgentService;
        }

        public void UpdateRawUserAgent(int batchSize)
        {
            var rawUserAgents = rawUserAgentService.GetRawUserAgents(batchSize, offsetId);

            if (rawUserAgents != null && rawUserAgents.Any())
            {
                var userAgentInfoList = DeviceDetection(rawUserAgents);

                ProcessUpdateUserAgent(userAgentInfoList);
            }
        }

        private IEnumerable<UserAgentInfoModel> DeviceDetection(IEnumerable<RawUserAgentModel> rawUserAgents)
        {
            var userAgentInfoList = new List<UserAgentInfoModel>();

            DeviceDetector.SetVersionTruncation(VersionTruncation.VERSION_TRUNCATION_NONE);

            foreach (var rawUserAgent in rawUserAgents)
            {
                var userAgentInfo = GetUserAgentInfo(rawUserAgent);

                userAgentInfoList.Add(userAgentInfo);
            }

            return userAgentInfoList;
        }

        private void ProcessUpdateUserAgent(IEnumerable<UserAgentInfoModel> rawUserAgents)
        {
            if (rawUserAgents != null && rawUserAgents.Any())
            {
                var rawUserAgentJson = JsonConvert.SerializeObject(rawUserAgents);

                var offsetValue = rawUserAgents.Min(x => x.Id);

                rawUserAgentService.UpdateRawUserAgent(rawUserAgentJson, offsetId, offsetValue);
            }
        }

        public IEnumerable<BrowserInfoDetectionModel> ParseUserAgentFromBrowserInfo(IEnumerable<BrowserInfoDetectionModel> browserInfos)
        {
            if (browserInfos != null && browserInfos.Any())
            {
                // Get userAgentInfoList & Update browserInfos
                var (userAgentInfoList, browserInfoParsed) = DeviceDetectionFromBrowserInfo(browserInfos);

                if (userAgentInfoList != null && userAgentInfoList.Any())
                {
                    _ = ProcessUpdateUserAgentAsync(userAgentInfoList);
                }

                return browserInfoParsed;
            }

            return new List<BrowserInfoDetectionModel>();
        }

        private (IEnumerable<UserAgentInfoModel>, IEnumerable<BrowserInfoDetectionModel>) DeviceDetectionFromBrowserInfo(IEnumerable<BrowserInfoDetectionModel> browserInfos)
        {
            if (browserInfos != null && browserInfos.Any())
            {
                var userAgentInfoList = new List<UserAgentInfoModel>();
                var browserInfosList = browserInfos.ToList();

                DeviceDetector.SetVersionTruncation(VersionTruncation.VERSION_TRUNCATION_NONE);

                foreach (var browserInfo in browserInfosList)
                {
                    var rawUserAgent = new RawUserAgentModel
                    {
                        Id = browserInfo.UserAgentId,
                        UserAgent = browserInfo.UserAgent
                    };
                    var userAgentInfo = GetUserAgentInfo(rawUserAgent);

                    browserInfo.UABrowserName = userAgentInfo.ClientName;
                    browserInfo.UAOSName = userAgentInfo.OSName;

                    userAgentInfoList.Add(userAgentInfo);
                }
                
                return (userAgentInfoList.Distinct().ToList(), browserInfosList);
            }

            return (new List<UserAgentInfoModel>(), new List<BrowserInfoDetectionModel>());
        }

        private async Task ProcessUpdateUserAgentAsync(IEnumerable<UserAgentInfoModel> rawUserAgents)
        {
            try
            {
                if (rawUserAgents != null && rawUserAgents.Any())
                {
                    var rawUserAgentJson = JsonConvert.SerializeObject(rawUserAgents);

                    var offsetValue = rawUserAgents.Max(x => x.Id);

                    rawUserAgentService.UpdateRawUserAgent(rawUserAgentJson, offsetId, offsetValue);
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);
            }
        }

        private UserAgentInfoModel GetUserAgentInfo(RawUserAgentModel item)
        {
            var deviceDetector = new DeviceDetector(item.UserAgent);

            deviceDetector.SkipBotDetection();
            deviceDetector.Parse();

            var userAgentInfo = new UserAgentInfoModel
            {
                Id = item.Id
            };

            if (deviceDetector.IsBot())
            {
                var botInfo = deviceDetector.GetBot();
                if (botInfo != null && botInfo.Success)
                {
                    userAgentInfo.BotName = botInfo.Match.Name;
                    userAgentInfo.BotCategory = botInfo.Match.Category;
                }
                else
                {
                    userAgentInfo.BotName = "Not detected";
                }
            }

            var clientInfo = deviceDetector.GetClient();

            if (clientInfo != null && clientInfo.Success)
            {
                userAgentInfo.ClientType = clientInfo.Match.Type;
                userAgentInfo.ClientName = clientInfo.Match.Name;
                userAgentInfo.ClientVersion = clientInfo.Match.Version;
            }

            var osInfo = deviceDetector.GetOs();

            if (osInfo != null && osInfo.Success)
            {
                userAgentInfo.OSName = osInfo.Match.Name;
                userAgentInfo.OSShortName = osInfo.Match.ShortName;
                userAgentInfo.OSVersion = osInfo.Match.Version;
                userAgentInfo.OSPlatform = osInfo.Match.Platform;
            }

            userAgentInfo.Device = deviceDetector.GetDeviceName();
            userAgentInfo.Brand = deviceDetector.GetBrandName();
            userAgentInfo.Model = deviceDetector.GetModel();

            return userAgentInfo;
        }

    }
}
