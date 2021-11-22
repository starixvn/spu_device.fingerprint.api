using Fps.Device.FingerPrint.Features.FingerPrint.Model;
using FPS.Device.FingerPrint.Model;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Services
{
    public interface IFingerPrintService
    {
        void InsertFingerPrint(FingerPrintEntity fingerPrint);
    }

    public class FingerPrintService : IFingerPrintService
    {
        private readonly IFingerPrintDataAccess fingerPrintDataAccess;

        public FingerPrintService(IFingerPrintDataAccess fingerPrintDataAccess)
        {
            this.fingerPrintDataAccess = fingerPrintDataAccess;
        }

        public void InsertFingerPrint(FingerPrintEntity fingerPrint)
        {
            fingerPrintDataAccess.InsertFingerPrint(fingerPrint);
        }
    }
}
