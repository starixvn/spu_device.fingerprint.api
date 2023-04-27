namespace Device.FingerPrint.ServiceAPI.Services
{
    using Device.FingerPrint.Features.FPTransactions.Services;
    using Fanex.Logging;
    using Newtonsoft.Json;
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
        private readonly ILogger _logger;

        public FPTransactionJobService(IFPTransactionServices transactionService, ILogger logger)
        {
            this.transactionService = transactionService;
            _logger = logger;
        }

        public Task<bool> Execute(int numberOfTrans, int numberOfBatch)
        {
            if (numberOfTrans > 0 && numberOfBatch > 0)
            {
                JobRun(numberOfTrans, numberOfBatch);
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
                        var transIdJson = JsonConvert.SerializeObject(trans);
                        _logger.Error(ex.Message + "\n" + transIdJson);
                    }
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
