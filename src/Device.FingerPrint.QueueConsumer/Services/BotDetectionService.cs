using Device.FingerPrint.Core;
using Device.FingerPrint.Model;
using Fanex.DeviceDetector.Models;
using Fanex.Logging;
using Fanex.Oneyed.Infrastructure.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.QueueConsumer.Services
{
    public interface IBotDetectionService
    {
        string GetFlaggedCode(MessageQueue message, string deviceNo, string userAgent);

        BotDModel TransformBotdModel(string encodedBotD);

        string GetBotDetectionTrue(BotDModel botDModel);

        string GetBotComponent(BotDModel botDModel, string flaggedCode);
    }

    public class BotDetectionService : IBotDetectionService
    {
        private readonly IAppSettings appSettings;

        public BotDetectionService(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public string GetFlaggedCode(MessageQueue message, string deviceNo, string userAgent)
        {
            var flaggedCode = string.Empty;
            if (!string.IsNullOrWhiteSpace(message?.BotD))
            {
                var botD = TransformBotD(message.BotD);
                flaggedCode = botD?.Result?.Bot == true ? botD.Result.BotKind : string.Empty;
            }

            if (string.IsNullOrEmpty(flaggedCode))
            {
                var browserless = Browserless(userAgent, string.IsNullOrWhiteSpace(deviceNo));
                flaggedCode = browserless.ToString();
            }

            return flaggedCode;
        }

        public BotDModel TransformBotdModel(string encodedBotD)
        {
            if (!string.IsNullOrEmpty(encodedBotD))
            {
                var botD = Fanex.DI.Core.Encoders.Base64.Decode(encodedBotD);

                if (!string.IsNullOrEmpty(botD))
                {
                    return JsonConvert.DeserializeObject<BotDModel>(botD);
                }
            }

            return new BotDModel();
        }

        public string GetBotDetectionTrue(BotDModel botDModel)
        {
            try
            {
                var detection = botDModel?.Detection;
                if (!string.IsNullOrEmpty(detection))
                {
                    JObject jObject = JObject.Parse(detection);
                    IEnumerable<string> filteredKeys = jObject.Properties()
                        .Where(p => p.Value.Value<bool>("bot"))
                        .Select(p => p.Name);

                    return string.Join(",", filteredKeys);
                }
            }
            catch (Exception ex)
            {
                LogService.Error("Consumer GetBotDetectionTrue: " + ex.Message + "\n" + JsonConvert.SerializeObject(botDModel));
            }

            return string.Empty;
        }

        public string GetBotComponent(BotDModel botDModel, string flaggedCode)
        {
            if (!appSettings.GetValue<bool>("Consumer:TurnOnBotComponentForHuman"))
            {
                if (flaggedCode != Flagged.Human.ToString())
                {
                    return botDModel?.Component ?? string.Empty;
                }
            }
            else
            {
                return botDModel?.Component ?? string.Empty;
            }

            return string.Empty;
        }

        private BotDModel TransformBotD(string encodedBotD)
        {
            var botD = Fanex.DI.Core.Encoders.Base64.Decode(encodedBotD);

            if (!string.IsNullOrEmpty(botD))
            {
                return JsonConvert.DeserializeObject<BotDModel>(botD);
            }

            return new BotDModel();
        }

        private Flagged Browserless(string userAgent, bool emptyFP)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(userAgent) && IsHeadless(userAgent))
                {
                    return Flagged.Headless;
                }

                if (emptyFP)
                {
                    return Flagged.EmptyDevice;
                }

                if (string.IsNullOrWhiteSpace(userAgent))
                {
                    return Flagged.EmptyUserAgent;
                }
            }
            catch (Exception ex)
            {
                LogService.Error("Consumer CheckBrowserless: " + ex.Message + "\n" + JsonConvert.SerializeObject(userAgent));
            }

            return Flagged.Human;
        }

        private bool IsHeadless(string userAgent)
        {
            if (userAgent.Contains("Headless")) return true;

            return false;
        }
    }
}
