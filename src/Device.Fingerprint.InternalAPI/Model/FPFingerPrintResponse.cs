using Device.FingerPrint.Features.FingerPrint.Entities;
using System.Net;

namespace Device.Fingerprint.InternalAPI.Model
{
    public class FPFingerPrintResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string? ErrorMessage { get; set; }

        public IEnumerable<FPFingerPrintTransform>? FPFingerPrint { get; set; }

        public ulong? MaxFingerprintID { get; set; }
    }
}
