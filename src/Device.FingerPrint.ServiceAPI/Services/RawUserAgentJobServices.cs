using Device.FingerPrint.Features.RawUserAgent.Services;
using Device.FingerPrint.Model;
using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using DeviceDetectorNET.Parser;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IRawUserAgentJobServices
    {
        void UpdateRawUserAgent(int batchSize);
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

        public void UpdateRawUserAgent(int batchSize)
        {
            var offsetId = (int)Offset.DetectRawUserAgent;
            var rawUserAgents = rawUserAgentService.GetRawUserAgents(batchSize, offsetId);

            if (rawUserAgents != null && rawUserAgents.Any())
            {
                var deviceDetectorModels = GetRawUserAgentInfo(rawUserAgents);
                ProcessUpdateRawUserAgent(deviceDetectorModels, offsetId);
            }
        }

        private IEnumerable<DeviceDetectorModel> GetRawUserAgentInfo(IEnumerable<RawUserAgentModel> rawUserAgents)
        {
            var deviceDetectorModels = new List<DeviceDetectorModel>();
            DeviceDetector.SetVersionTruncation(VersionTruncation.VERSION_TRUNCATION_NONE);
            foreach (var item in rawUserAgents)
            {
                var deviceDetector = new DeviceDetector(item.UserAgent);
                deviceDetector.SetCache(new DictionaryCache());
                // deviceDetector.DiscardBotInformation();
                // deviceDetector.SkipBotDetection();
                deviceDetector.Parse();

                var deviceDetectorModel = TransformData(item, deviceDetector);
                deviceDetectorModels.Add(deviceDetectorModel);
            }
            return deviceDetectorModels;
        }

        private DeviceDetectorModel TransformData(RawUserAgentModel item, DeviceDetector deviceDetector)
        {
            var deviceDetectorModel = new DeviceDetectorModel
            {
                Id = item.Id
            };

            if (deviceDetector != null)
            {
                if (deviceDetector.IsBot())
                {
                    // handle bots,spiders,crawlers,...
                    var botInfo = deviceDetector.GetBot();
                    if (botInfo != null && botInfo.Success)
                    {
                        deviceDetectorModel.BotName = botInfo.Match.Name;
                        deviceDetectorModel.BotCategory = botInfo.Match.Category;
                    }
                    else
                    {
                        deviceDetectorModel.BotName = "Not detected";
                    }
                }
                else
                {
                    var clientInfo = deviceDetector.GetClient(); // holds information about browser, feed reader, media player, ...
                    var osInfo = deviceDetector.GetOs();
                    if (clientInfo != null && clientInfo.Success)
                    {
                        deviceDetectorModel.ClientType = clientInfo.Match.Type;
                        deviceDetectorModel.ClientName = clientInfo.Match.Name;
                        deviceDetectorModel.ClientVersion = clientInfo.Match.Version;
                    }
                    if (osInfo != null && osInfo.Success)
                    {
                        deviceDetectorModel.OSName = osInfo.Match.Name;
                        deviceDetectorModel.OSShortName = osInfo.Match.ShortName;
                        deviceDetectorModel.OSVersion = osInfo.Match.Version;
                        deviceDetectorModel.OSPlatform = osInfo.Match.Platform;
                    }
                    deviceDetectorModel.Device = deviceDetector.GetDeviceName();
                    deviceDetectorModel.Brand = deviceDetector.GetBrandName();
                    deviceDetectorModel.Model = deviceDetector.GetModel();
                }
            }
            return deviceDetectorModel;
        }

        private void ProcessUpdateRawUserAgent(IEnumerable<DeviceDetectorModel> rawUserAgents, int offsetId)
        {
            if (rawUserAgents != null && rawUserAgents.Any())
            {
                var rawUserAgentJson = JsonConvert.SerializeObject(rawUserAgents);
                var offsetValue = rawUserAgents.Max(x => x.Id);

                rawUserAgentService.UpdateRawUserAgent(rawUserAgentJson, offsetId, offsetValue);
            }
        }
    }
}
