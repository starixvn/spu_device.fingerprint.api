namespace Device.FingerPrint.ServiceAPI.Services
{
    using Device.FingerPrint.Features.Transaction.Services;
    using Fanex.Logging;
    using Fanex.Logging.Extensions.Database;
    using Fanex.Logging.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Text.Json;

    public interface ITransactionJobServices
    {
        Task<bool> MergeTrans(int batchSize, int numberOfBatch, int retry);

        Task<bool> ForceMergeTrans(int batchSize, int numberOfBatch, int retry);
    }

    public class TransactionJobServices : ITransactionJobServices
    {
        private readonly ITransactionService transactionService;
        private readonly ILogger _logger;

        public TransactionJobServices(ITransactionService transactionService, ILogger logger)
        {
            this.transactionService = transactionService;
            _logger = logger;
        }

        public Task<bool> MergeTrans(int batchSize, int numberOfBatch, int retry)
        {
            var mergeTrans = transactionService.GetMergeTransaction(batchSize * numberOfBatch, retry);

            if (mergeTrans != null && mergeTrans.Any())
            {
                var mergeTransGroup = mergeTrans.Chunk2(batchSize);
                var tasks = new List<Task>();

                foreach (var trans in mergeTransGroup)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var mergeTransJson = JsonSerializer.Serialize(trans);

                        try
                        {
                            transactionService.MergeTransaction(mergeTransJson);
                        }
                        catch (Exception ex)
                        {
                            WriteLogException(ex, mergeTransJson);
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }

            return Task.FromResult(true);
        }

        public Task<bool> ForceMergeTrans(int batchSize, int numberOfBatch, int retry)
        {
            var mergeTrans = transactionService.GetForceMergeTransaction(batchSize * numberOfBatch, retry);

            if (mergeTrans != null && mergeTrans.Any())
            {
                var mergeTransGroup = mergeTrans.Chunk2(batchSize);
                var tasks = new List<Task>();

                foreach (var trans in mergeTransGroup)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var mergeTransJson = JsonSerializer.Serialize(trans);
                        try
                        {
                            transactionService.ForceMergeTransaction(mergeTransJson);
                        }
                        catch (Exception ex)
                        {
                            WriteLogException(ex, mergeTransJson);
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }

            return Task.FromResult(true);
        }

        private void WriteLogException(Exception ex, string message)
        {
            var logMessage = new LogMessage(ex.Message)
                   .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                   .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                   .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                   .AddMessage(message)
                   .ToString();

            _logger.ErrorAsync(logMessage);
        }

    }
}
