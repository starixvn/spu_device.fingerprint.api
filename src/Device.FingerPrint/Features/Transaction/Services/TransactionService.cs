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

        IEnumerable<TransactionEvolutionEntity> GetTransactionEvolution(int batchSize);

        IEnumerable<TransactionEvolutionAttributesEntity> GetTransactionEvolutionAttributes(string fingerPrintIds, string BrowserInstanceCode);

        void UpdateTransactionEvolution(string transJson, int type);
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

        public IEnumerable<TransactionEvolutionAttributesEntity> GetTransactionEvolutionAttributes(string fingerPrintIds, string BrowserInstanceCode)
        {
            return transactionDataAccess.GetTransactionEvolutionAttributes(fingerPrintIds, BrowserInstanceCode);
        }

        public IEnumerable<TransactionEvolutionEntity> GetTransactionEvolution(int batchSize)
        {
            return transactionDataAccess.GetTransactionEvolution(batchSize);
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

        public void UpdateTransactionEvolution(string transJson, int type)
        {
            transactionDataAccess.UpdateTransactionEvolution(transJson, type);
        }
    }
}
