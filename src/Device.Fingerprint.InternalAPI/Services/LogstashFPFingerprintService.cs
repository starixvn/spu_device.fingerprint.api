using Device.Fingerprint.InternalAPI.Model;
using Device.FingerPrint.Core;
using Device.FingerPrint.Features.FingerPrint.Entities;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace Device.Fingerprint.InternalAPI.Services
{
    public interface ILogstashFPFingerprintService
    {
        ulong PushLogAsync(IEnumerable<FPFingerPrintTransform> fingerprintTransforms);
    }
    public class LogstashFPFingerprintService : ILogstashFPFingerprintService
    {
        private readonly IAppSettings appSettings;

        public LogstashFPFingerprintService(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public ulong PushLogAsync(IEnumerable<FPFingerPrintTransform> fingerprintTransforms)
        {
            ulong processedFingerprintID = 0;
            using var client = new TcpClient(appSettings.LogstashTcpHostname, appSettings.LogstashTcpPort);
            using var stream = client.GetStream();

            foreach (var fingerprint in fingerprintTransforms)
            {
                var logMessage = TransformLogMessage(fingerprint);

                string jsonLog = JsonConvert.SerializeObject(logMessage);

                byte[] data = Encoding.UTF8.GetBytes(jsonLog + "\n");

                stream.WriteAsync(data);

                processedFingerprintID = Math.Max(processedFingerprintID, fingerprint.ID);
            }

            return processedFingerprintID;
        }

        private FPFingerprintLogstashModel TransformLogMessage(FPFingerPrintTransform fingerprint)
        {
            var logMessage = new FPFingerprintLogstashModel
            {
                Message = fingerprint,
                Tags = "FingerprintAttribute",
            };

            return logMessage;
        }

    }
}
