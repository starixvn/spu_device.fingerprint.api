using Device.FingerPrint.Features.Transaction.Services;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IDeviceIdentifyServices
    {
        public void DeviceIdentify(int batchSize);

        public void MergeTransaction(int batchSize);

        public void UpdateTransaction(int batchSize);

        public void UpdateTransaction24();

        public void UpdateTransaction48();
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

        public void UpdateTransaction(int batchSize)
        {
            transactionService.UpdateTransaction(batchSize);
        }

        public void UpdateTransaction24()
        {
            transactionService.UpdateTransaction24();
        }

        public void UpdateTransaction48()
        {
            transactionService.UpdateTransaction48();
        }
    }
}
