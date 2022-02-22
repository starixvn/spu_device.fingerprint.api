using Device.FingerPrint.Features.Transaction.Services;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IDeviceIdentifyServices
    {
        public void DeviceIdentify(int batchSize);

        public void MergeTransaction(int batchSize);
    }

    public class DeviceIdentifyServices: IDeviceIdentifyServices
    {
        private readonly ITransactionService transactionService;

        public DeviceIdentifyServices(ITransactionService transactionService)
        {
            this.transactionService = transactionService;
        }

        public void DeviceIdentify(int batchSize)
        {
            transactionService.DeviceIdentify(batchSize);
        }

        public void MergeTransaction(int batchSize)
        {
            transactionService.MergeTransaction(batchSize);
        }
    }
}
