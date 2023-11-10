using Device.FingerPrint.Core;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Fanex.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Device.FingerPrint.QueueConsumer.Factory
{
    public class DCSSubsTransactionFactory : AbstractFactoryTransaction
    {
        private Task taskDcsSub = null;
        private readonly CancellationTokenSource tokenSourceDcsSub = new CancellationTokenSource();
        private readonly IAppSettings appSettings;
        private readonly ITransactionService transactionService;

        public DCSSubsTransactionFactory(IAppSettings appSettings, ITransactionService transactionService)
        {
            this.appSettings = appSettings;
            this.transactionService = transactionService;
        }

        protected override void InsertTransaction(List<RawTransactionModel> transactions)
        {
            transactionService.InsertRawTransaction(transactions, false);
        }

        protected override void DelayTask(List<RawTransactionModel> transactions)
        {
            if (taskDcsSub == null)
            {
                taskDcsSub = Task.Delay(appSettings.Interval).ContinueWith((task) =>
                {
                    try
                    {
                        ProcessDelayedTask(transactions);
                    }
                    catch (OperationCanceledException)
                    {
                        LogService.Info("Task of add transaction dcs sub list was canceled.");
                    }
                }, tokenSourceDcsSub.Token);
            }
        }

        private void ProcessDelayedTask(List<RawTransactionModel> transactions)
        {
            if (transactions.Any())
            {
                InsertTransaction(transactions);
            }
            taskDcsSub = null;
            transactions.Clear();
        }

        protected override int GetBatchThreshold() => appSettings.TransBatchSize;
    }
}
