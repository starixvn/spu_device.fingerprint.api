using Device.Fingerprint.WebAPI.Models;
using Device.Fingerprint.WebAPI.Ultilities;

namespace Device.Fingerprint.WebAPI.Services
{
    public class TransactionAttributeService
    {

        public string GenerateFPAttributesCode(string fingerPrintCode, FingerPrintAttributeModel model, Crypto crypto)
        {
            var concatString = fingerPrintCode + "_doNotTrack=" + model.DoNotTrack + "_glRenderer=" + model.GLRenderer + "_accept=" + model.HeaderAccept + "_encoding=" + model.HeaderAcceptEncoding + "_headers=" + model.Headers;
            var fpAttributeCode = crypto.GetMD5Hash(concatString);
            return fpAttributeCode;
        }
    }
}
