using Device.FingerPrint.Features.RawUserAgent.Services;
using Device.FingerPrint.Model;
using DeviceDetectorNET;
using DeviceDetectorNET.Cache;
using DeviceDetectorNET.Parser;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IRawUserAgentJobServices
    {
        void UpdateRawUserAgent(int batchSize);
    }

    public class RawUserAgentJobServices : IRawUserAgentJobServices
    {
        private readonly IRawUserAgentService rawUserAgentService;       

        public RawUserAgentJobServices(IRawUserAgentService rawUserAgentService)
        {
            this.rawUserAgentService = rawUserAgentService;            
        }

        public void UpdateRawUserAgent(int batchSize)
        {
            var offsetId = (int)Offset.DetectRawUserAgent;
            var rawUserAgents = rawUserAgentService.GetRawUserAgents(batchSize, offsetId);

            if (rawUserAgents != null && rawUserAgents.Any())
            {
                var userAgentInfoList = DeviceDetection(rawUserAgents);

                ProcessUpdateUserAgent(userAgentInfoList, offsetId);
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

        private UserAgentInfoModel GetUserAgentInfo(RawUserAgentModel item)
        {
            var deviceDetector = new DeviceDetector(item.UserAgent);

            deviceDetector.SetCache(new DictionaryCache());

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

        private void ProcessUpdateUserAgent(IEnumerable<UserAgentInfoModel> rawUserAgents, int offsetId)
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
