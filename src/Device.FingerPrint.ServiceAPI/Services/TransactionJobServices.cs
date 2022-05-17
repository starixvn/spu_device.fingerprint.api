using Device.FingerPrint.Features.ServerTransaction.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface ITransactionJobServices
    {
        public void UpdateTransaction(int batchSize, int numberOfBatch, int retry);

        public void UpdateTransactionN(int batchSize, int numberOfBatch);

        public void MergeTrans(int batchSize, int numberOfBatch, int retry);

        public void ForceMergeTrans(int batchSize, int numberOfBatch, int retry);
    }

    public class TransactionJobServices : ITransactionJobServices
    {
        private readonly ITransactionService transactionService;
        private readonly IRawTransactionService rawTransactionServices;
        private readonly ISentryClient sentryClient;

        public TransactionJobServices(ITransactionService transactionService, IRawTransactionService rawTransactionServices, ISentryClient sentryClient)
        {
            this.transactionService = transactionService;
            this.rawTransactionServices = rawTransactionServices;
            this.sentryClient = sentryClient;
        }

        public void UpdateTransaction(int batchSize, int numberOfBatch, int retry)
        {
            var rawTrans = rawTransactionServices.GetServerTransaction(batchSize * numberOfBatch, retry, (int)Offset.UpdateServerTransaction);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransaction(rawTrans, batchSize, retry, (int)Offset.UpdateServerTransaction);
            }
        }

        private void ProcessUpdateTransaction(IEnumerable<ServerTransactionModel> rawTrans, int batchSize, int retry, int offsetId)
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
                            var offsetValue = rawTrans.Min(x => x.Id);

                            rawTransactionServices.UpdateRawTransaction(rawTransJson, retry, offsetId, offsetValue);
                        }
                        catch (Exception ex)
                        {
                            sentryClient.CaptureMessage(ex.Message + "\n" + rawTransJson, SentryLevel.Error);
                            throw;
                        }
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        public void UpdateTransactionN(int batchSize, int numberOfBatch)
        {
            var rawTrans = rawTransactionServices.GetServerTransactionN(batchSize * numberOfBatch, (int)Offset.UpdateServerTransactionN);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransactionN(rawTrans, batchSize, (int)Offset.UpdateServerTransactionN);
            }
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
                            sentryClient.CaptureMessage(ex.Message + "\n" + rawTransJson, SentryLevel.Error);
                            throw;
                        }
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }


        public void MergeTrans(int batchSize, int numberOfBatch, int retry)
        {
            var mergeTrans = transactionService.GetMergeTransaction(batchSize * numberOfBatch, retry);

            if (mergeTrans != null && mergeTrans.Any())
            {
                var mergeTransGroup = mergeTrans.Chunk2(numberOfBatch);
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
                            sentryClient.CaptureMessage(ex.Message + "\n" + mergeTransJson, SentryLevel.Error);
                            throw;
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        public void ForceMergeTrans(int batchSize, int numberOfBatch, int retry)
        {
            var mergeTrans = transactionService.GetForceMergeTransaction(batchSize * numberOfBatch, retry);

            if (mergeTrans != null && mergeTrans.Any())
            {
                var mergeTransGroup = mergeTrans.Chunk2(numberOfBatch);
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
                            sentryClient.CaptureMessage(ex.Message + "\n" + mergeTransJson, SentryLevel.Error);
                            throw;
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}
