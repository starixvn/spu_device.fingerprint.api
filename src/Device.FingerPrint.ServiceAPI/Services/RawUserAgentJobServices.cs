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
        void UpdateRawUserAgent(int batchSize, int numberOfBatch);
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

        public void UpdateRawUserAgent(int batchSize, int numberOfBatch)
        {
            var offsetId = (int)Offset.DetectRawUserAgent;
            var rawUserAgents = rawUserAgentService.GetRawUserAgents(batchSize * numberOfBatch, offsetId);

            if (rawUserAgents != null && rawUserAgents.Any())
            {
                var deviceDetectorModels = GetDeviceDetector(rawUserAgents);
                ProcessUpdateRawUserAgent(deviceDetectorModels, batchSize, offsetId);
            }
        }

        private IEnumerable<DeviceDetectorModel> GetDeviceDetector(IEnumerable<RawUserAgentModel> rawUserAgents)
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
                deviceDetectorModels.Add(deviceDetectorModel);
            }
            return deviceDetectorModels;
        }

        private void ProcessUpdateRawUserAgent(IEnumerable<DeviceDetectorModel> rawUserAgents, int batchSize, int offsetId)
        {
            if (rawUserAgents != null && rawUserAgents.Any())
            {
                var tasks = new List<Task>();
                var chunkRawUserAgents = rawUserAgents.Chunk2(batchSize);

                foreach (var item in chunkRawUserAgents)
                {
                    var task = Task.Run(() =>
                    {
                        var rawUserAgentJson = JsonConvert.SerializeObject(item);
                        try
                        {
                            var offsetValue = rawUserAgents.Max(x => x.Id);

                            rawUserAgentService.UpdateRawUserAgent(rawUserAgentJson, offsetId, offsetValue);
                        }
                        catch (Exception ex)
                        {
                            sentryClient.CaptureMessage(ex.Message + "\n" + rawUserAgentJson, SentryLevel.Error);
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
