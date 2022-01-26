using Fanex.Data.Repository;
using Device.FingerPrint.Features.Transaction.Criterias;
using Device.FingerPrint.Features.Transaction.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionDataAccess
    {
        void InsertJsonTransaction(string jsonTransaction);

        void DeviceIdentify(int batchSize);

        void MergeTransaction(int batchSize);
    }

    public class TransactionDataAccess : ITransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public TransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public void InsertJsonTransaction(string jsonTransaction)
        {
            var criteria = new InsertJsonTransactionCriteria
            {
                Transaction = jsonTransaction
            };

            repository.Execute(criteria);
        }

        public void DeviceIdentify(int batchSize)
        {
            var criteria = new DeviceIdentifyCriteria
            {
                Size = batchSize
            };

            repository.Execute(criteria);
        }

        public void MergeTransaction(int batchSize)
        {
            var criteria = new MergeTransactionCriteria
            {
               Size  = batchSize
            };

            repository.Execute(criteria);
        }
    }
}
