using Device.FingerPrint.Features.ServerTransaction.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Fanex.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface ITransactionJobServices
    {
        Task<bool> UpdateTransaction(int batchSize, int numberOfBatch, int retry);

        Task<bool> UpdateTransactionRetry(int batchSize, int numberOfBatch, int retry);

        Task<bool> UpdateTransactionN(int batchSize, int numberOfBatch, int retry);

        Task<bool> MergeTrans(int batchSize, int numberOfBatch, int retry);

        Task<bool> ForceMergeTrans(int batchSize, int numberOfBatch, int retry);
    }

    public class TransactionJobServices : ITransactionJobServices
    {
        private readonly ITransactionService transactionService;
        private readonly IRawTransactionService rawTransactionServices;
        private readonly ILogger _logger;

        public TransactionJobServices(ITransactionService transactionService, IRawTransactionService rawTransactionServices, ILogger logger)
        {
            this.transactionService = transactionService;
            this.rawTransactionServices = rawTransactionServices;
            _logger = logger;
        }

        public Task<bool> UpdateTransaction(int batchSize, int numberOfBatch, int retry)
        {
            var rawTrans = rawTransactionServices.GetServerTransaction(batchSize * numberOfBatch, retry, (int)Offset.UpdateServerTransaction);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransaction(rawTrans, batchSize, retry, (int)Offset.UpdateServerTransaction);
            }

            return Task.FromResult(true);
        }

        public Task<bool> UpdateTransactionRetry(int batchSize, int numberOfBatch, int retry)
        {
            var rawTrans = rawTransactionServices.GetServerTransactionRetry(batchSize * numberOfBatch, retry, (int)Offset.UpdateServerTransactionRetry);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransaction(rawTrans, batchSize, retry, (int)Offset.UpdateServerTransactionRetry);
            }

            return Task.FromResult(true);
        }

        private void ProcessUpdateTransaction(IEnumerable<ServerTransactionModel> rawTrans, int batchSize, int retry, int offsetId)
        {
            if (rawTrans != null && rawTrans.Any())
            {
                var tasks = new List<Task>();
                var chunkRawTrans = rawTrans.Chunk2(batchSize);
                var offsetValue = rawTrans.Min(x => x.Id);

                foreach (var item in chunkRawTrans)
                {
                    var task = Task.Run(() =>
                    {
                        var rawTransJson = JsonConvert.SerializeObject(item);
                        try
                        {
                            rawTransactionServices.UpdateRawTransaction(rawTransJson, retry, offsetId, offsetValue);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.Message + "\n" + rawTransJson);
                        }
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        public Task<bool> UpdateTransactionN(int batchSize, int numberOfBatch, int retry)
        {
            var rawTrans = rawTransactionServices.GetServerTransactionN(batchSize * numberOfBatch, retry, (int)Offset.UpdateServerTransactionN);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransactionN(rawTrans, batchSize, (int)Offset.UpdateServerTransactionN);
            }

            return Task.FromResult(true);
        }

        private void ProcessUpdateTransactionN(IEnumerable<ServerTransactionModel> rawTrans, int batchSize, int offsetId)
        {
            if (rawTrans != null && rawTrans.Any())
            {
                var tasks = new List<Task>();
                var chunkRawTrans = rawTrans.Chunk2(batchSize);

                foreach (var item in chunkRawTrans)
                {
                    var task = Task.Run(() =>
                    {
                        var rawTransJson = JsonConvert.SerializeObject(item);
                        try
                        {
                            var offsetvalue = rawTrans.Min(x => x.Id);
                            rawTransactionServices.UpdateRawTransactionN(rawTransJson, offsetId, offsetvalue);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.Message + "\n" + rawTransJson);
                        }
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
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
                        var mergeTransJson = JsonConvert.SerializeObject(trans);

                        try
                        {
                            transactionService.MergeTransaction(mergeTransJson);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.Message + "\n" + mergeTransJson);
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
                        var mergeTransJson = JsonConvert.SerializeObject(trans);
                        try
                        {
                            transactionService.ForceMergeTransaction(mergeTransJson);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.Message + "\n" + mergeTransJson);
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }

            return Task.FromResult(true);
        }

    }
}
