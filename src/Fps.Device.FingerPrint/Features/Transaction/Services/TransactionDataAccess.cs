using Fanex.Data.Repository;
using Fps.Device.FingerPrint.Features.Transaction.Criterias;
using Fps.Device.FingerPrint.Features.Transaction.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fps.Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionDataAccess
    {
        void InsertTransaction(TransactionEntity transaction);
        void InsertJsonTransaction(string jsonTransaction);

        void RecoverDevice(int batchSize);
    }

    public class TransactionDataAccess : ITransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public TransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public void InsertTransaction(TransactionEntity transaction)
        {
            var criteria = new InsertTransactionCriteria
            {
                LoginName = transaction.LoginName,
                SubscriberId = transaction.SubscriberId,
                SessionKey = transaction.SessionKey,
                ClientIp = transaction.ClientIp,
                TransTime = transaction.TransTime,
                UserAgent = transaction.UserAgent,
                Url = transaction.Url
            };

            repository.Execute(criteria);
        }
        public void InsertJsonTransaction(string jsonTransaction)
        {
            var criteria = new InsertJsonTransactionCriteria
            {
                Transaction = jsonTransaction
            };

            repository.Execute(criteria);
        }

        public void RecoverDevice(int batchSize)
        {
            var criteria = new RecoverDeviceCriteria
            {
                Batch = batchSize
            };

            repository.Execute(criteria);
        }
    }
}
