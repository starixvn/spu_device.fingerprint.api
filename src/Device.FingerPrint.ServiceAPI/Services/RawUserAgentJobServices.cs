using Device.FingerPrint.Features.RawUserAgent.Services;
using Device.FingerPrint.Model;
using DeviceDetectorNET;
using DeviceDetectorNET.Parser;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IRawUserAgentJobServices
    {
        Task<bool> UpdateRawUserAgent(int batchSize);
    }

    public class RawUserAgentJobServices : IRawUserAgentJobServices
    {
        private readonly IRawUserAgentService rawUserAgentService;
        private readonly ISentryClient sentryClient;

        public RawUserAgentJobServices(IRawUserAgentService rawUserAgentService, ISentryClient sentryClient)
        {
            this.rawUserAgentService = rawUserAgentService;
            this.sentryClient = sentryClient;
        }

        public Task<bool> UpdateRawUserAgent(int batchSize)
        {
            try
            {
                var rawUserAgents = rawUserAgentService.GetRawUserAgents(batchSize);

                if (rawUserAgents != null && rawUserAgents.Any())
                {
                    var userAgentInfoList = DeviceDetection(rawUserAgents);

                    ProcessUpdateUserAgent(userAgentInfoList);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
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

                rawUserAgentService.UpdateRawUserAgent(rawUserAgentJson);
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
