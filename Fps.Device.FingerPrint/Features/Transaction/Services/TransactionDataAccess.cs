using Fanex.Data.Repository;
using Fps.Device.FingerPrint.Features.Transaction.Criterias;
using Fps.Device.FingerPrint.Features.Transaction.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fps.Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionDataAccess
    {
        void InsertTransaction(TransactionEntity transaction);
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
                RoutingKey = transaction.RoutingKey,
                ClientIp = transaction.ClientIp,
                TransTime = transaction.TransTime,
                UserAgent = transaction.UserAgent
            };

            repository.Execute(criteria);
        }
    }
}
