namespace Device.FingerPrint.ServiceAPI.Services
{
    using Device.FingerPrint.Features.FPTransactions.Services;
    using Fanex.Logging;
    using Newtonsoft.Json;
    using Sentry;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFPTransactionJobService
    {
        Task<bool> Execute(int numberOfTrans, int numberOfBatch);
    }

    public class FPTransactionJobService : IFPTransactionJobService
    {
        private readonly IFPTransactionServices transactionService;
        private readonly IList<string> ignoreMessages;
        private readonly ISentryClient sentryClient;

        public FPTransactionJobService(IFPTransactionServices transactionService, ISentryClient sentryClient)
        {
            this.transactionService = transactionService;
            this.sentryClient = sentryClient;

            ignoreMessages = new List<string> {
                "Deadlock found when trying to get lock; try restarting transaction"
            };
        }

        public Task<bool> Execute(int numberOfTrans, int numberOfBatch)
        {
            if (numberOfTrans > 0 && numberOfBatch > 0)
            {
                try
                {
                    JobRun(numberOfTrans, numberOfBatch);
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);

                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }

        private void JobRun(int numberOfTrans, int numberOfBatch)
        {
            var rawTrans = transactionService.GetFPRawTransaction(0, numberOfTrans, numberOfBatch);

            var tasks = new List<Task>();

            foreach (var trans in rawTrans)
            {
                var task = Task.Run(() =>
                {
                    try
                    {
                        transactionService
                           .UnhandleFPSubscriberActionProcessing(trans.MinTransId, trans.MaxTransId)
                           .UserAgentProcessing(trans.MinTransId, trans.MaxTransId)
                           .TransactionProcessing(trans.MinTransId, trans.MaxTransId);
                    }
                    catch (Exception ex)
                    {
                        if (!ignoreMessages.Contains(ex.Message))
                        {
                            var transIdJson = JsonConvert.SerializeObject(trans);
                            LogService.Error(ex.Message + "\n" + transIdJson, ex);
                        }
                    }
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
