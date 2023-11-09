using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Device.FingerPrint.Model;
using Device.FingerPrint.Core;
using System;
using Fanex.Logging;
using System.Linq;

namespace Device.FingerPrint.QueueConsumer.Factory
{
    public class ExtraSubsTransactionFactory : AbstractFactoryTransaction
    {
        private Task taskExtraSub = null;
        private readonly CancellationTokenSource tokenSourceExtraSub = new CancellationTokenSource();
        private readonly IAppSettings appSettings;

        public ExtraSubsTransactionFactory(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        protected override void InsertTransaction(List<RawTransactionModel> transactions)
        {
            TransformTransactionByIps(transactions);
            //_capturingDataService.InsertTransaction(transactions, true);
            transactions.Clear();
        }

        protected override void DelayTask(List<RawTransactionModel> transactions)
        {
            if (taskExtraSub == null)
            {
                taskExtraSub = Task.Delay(appSettings.Interval).ContinueWith((task) =>
                {
                    try
                    {
                        ProcessDelayedTask(transactions);
                    }
                    catch (OperationCanceledException)
                    {
                        LogService.Info("Task of add transaction extra sub list was canceled.");
                    }
                }, tokenSourceExtraSub.Token);
            }
        }

        private void ProcessDelayedTask(List<RawTransactionModel> transactions)
        {
            if (transactions.Any())
            {
                InsertTransaction(transactions);
            }
            taskExtraSub = null;
        }

        protected override int GetBatchThreshold() => appSettings.TransOfExtraSubsBatch;
    }
}
