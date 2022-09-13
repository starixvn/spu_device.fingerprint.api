using Device.FingerPrint.Features.Transaction.Model;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionService
    {
       
        void DeviceIdentify(string transJson, int groupType);

        void DeviceIdentifyNoFingerprint(string rawTransIds);

        (IEnumerable<DeviceIdentifyEntity>, IEnumerable<DeviceIdentifyEntity> ,IEnumerable<ulong>) GetDeviceIdentify(int batchSize, int numberOfBatch);

        void MergeTransaction(string transJson);

        IEnumerable<MergeTransEntity> GetMergeTransaction(int batchSize, int retry);

        IEnumerable<MergeTransEntity> GetForceMergeTransaction(int batchSize, int retry);

        void ForceMergeTransaction(string transJson);

        IEnumerable<FingerPrintEvolutionEntity> GetFingerPrintEvolution(int batchSize);

        void UpdateFingerPrintEvolution(string transJson, int type);
    }

    public class TransactionService : ITransactionService
    {
        private readonly ITransactionDataAccess transactionDataAccess;

        public TransactionService(ITransactionDataAccess transactionDataAccess)
        {
            this.transactionDataAccess = transactionDataAccess;
        }

        public void DeviceIdentify(string transJson, int groupType)
        {
            transactionDataAccess.DeviceIdentify(transJson, groupType);
        }

        public void DeviceIdentifyNoFingerprint(string rawTransIds)
        {
            transactionDataAccess.DeviceIdentifyNoFingerprint(rawTransIds);
        }

        public void ForceMergeTransaction(string transJson)
        {
            transactionDataAccess.UpdateForceMergeTransaction(transJson);
        }

        public (IEnumerable<DeviceIdentifyEntity>, IEnumerable<DeviceIdentifyEntity> ,IEnumerable<ulong>) GetDeviceIdentify(int batchSize, int numberOfBatch)
        {
            return transactionDataAccess.GetDeviceIdentify(batchSize, numberOfBatch);            
        }

        public IEnumerable<FingerPrintEvolutionEntity> GetFingerPrintEvolution(int batchSize)
        {
            return transactionDataAccess.GetFingerPrintEvolution(batchSize);
        }

        public IEnumerable<MergeTransEntity> GetForceMergeTransaction(int batchSize, int retry)
        {
            return transactionDataAccess.GetForceMergeTransaction(batchSize, retry);
        }

        public IEnumerable<MergeTransEntity> GetMergeTransaction(int batchSize, int retry)
        {
            return transactionDataAccess.GetMergeTransaction(batchSize, retry);
        }

        public void MergeTransaction(string transJson)
        {
            transactionDataAccess.UpdateMergeTransaction(transJson);
        }

        public void UpdateFingerPrintEvolution(string transJson, int type)
        {
            transactionDataAccess.UpdateFingerPrintEvolution(transJson, type);
        }
    }
}
