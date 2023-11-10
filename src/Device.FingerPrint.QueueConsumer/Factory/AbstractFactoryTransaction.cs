using Device.FingerPrint.Model;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using System;
using System.Collections.Generic;

namespace Device.FingerPrint.QueueConsumer.Factory
{
    public abstract class AbstractFactoryTransaction
    {
        public void HandleTransaction(List<RawTransactionModel> transactions)
        {
            try
            {
                if (transactions.Count >= GetBatchThreshold())
                {
                    InsertTransaction(transactions);
                }
                else
                {
                    DelayTask(transactions);
                }
            }
            catch (Exception ex)
            {
                var logMessage = new LogMessage(ex.Message)
                    .AddSection("Data Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                   .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex)).ToString();

                LogService.Error(logMessage);
            }
        }

        protected abstract void InsertTransaction(List<RawTransactionModel> transactions);

        protected abstract void DelayTask(List<RawTransactionModel> transactions);

        protected abstract int GetBatchThreshold();
    }
}
