namespace Fps.Device.FingerPrint.Features.FingerPrint.Services
{
    public interface IFingerPrintService
    {
        void InsertFingerPrint(string deviceIdentify, long attributeId);
    }

    public class FingerPrintService : IFingerPrintService
    {
        private readonly IFingerPrintDataAccess fingerPrintDataAccess;

        public FingerPrintService(IFingerPrintDataAccess fingerPrintDataAccess)
        {
            this.fingerPrintDataAccess = fingerPrintDataAccess;
        }

        public void InsertFingerPrint(string deviceIdentify, long attributeId)
        {
            fingerPrintDataAccess.InsertFingerPrint(deviceIdentify, attributeId);
        }
    }
}
