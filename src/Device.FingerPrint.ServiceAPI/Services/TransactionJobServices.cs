using Device.FingerPrint.Features.ServerTransaction.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface ITransactionJobServices
    {
        public void UpdateTransaction(int batchSize, int numberOfBatch, int retry);

        public void UpdateTransactionN(int batchSize, int numberOfBatch);

        public void MergeTrans(int batchSize, int numberOfBatch);

        public void ForceMergeTrans(int batchSize, int numberOfBatch);
    }

    public class TransactionJobServices : ITransactionJobServices
    {
        private readonly ITransactionService transactionService;
        private readonly IRawTransactionService rawTransactionServices;

        public TransactionJobServices(ITransactionService transactionService, IRawTransactionService rawTransactionServices)
        {
            this.transactionService = transactionService;
            this.rawTransactionServices = rawTransactionServices;
        }

        public void UpdateTransaction(int batchSize, int numberOfBatch, int retry)
        {
            var rawTrans = rawTransactionServices.GetServerTransaction(batchSize * numberOfBatch, retry, (int)Offset.RawTransaction);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransaction(rawTrans, batchSize, retry, (int)Offset.RawTransaction);
            }
        }

        public void UpdateTransactionN(int batchSize, int numberOfBatch)
        {
            var rawTrans = rawTransactionServices.GetServerTransactionN(batchSize * numberOfBatch, (int)Offset.RawTransactionN);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransactionN(rawTrans, batchSize, (int)Offset.RawTransactionN);
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
                        var offsetvalue = rawTrans.Min(x => x.Id);
                        rawTransactionServices.UpdateRawTransactionN(rawTransJson, offsetId, offsetvalue);
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
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
                        var offsetValue = rawTrans.Min(x => x.Id);

                        rawTransactionServices.UpdateRawTransaction(rawTransJson, retry, offsetId, offsetValue);
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }


        public void MergeTrans(int batchSize, int numberOfBatch)
        {
            var mergeTrans = transactionService.GetMergeTransaction(batchSize, 3);

            if (mergeTrans != null && mergeTrans.Any())
            {
                var mergeTransGroup = mergeTrans.Chunk2(numberOfBatch);
                var tasks = new List<Task>();

                foreach (var trans in mergeTransGroup)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var mergeTransJson = JsonConvert.SerializeObject(trans);

                        transactionService.MergeTransaction(mergeTransJson);
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        public void ForceMergeTrans(int batchSize, int numberOfBatch)
        {
            var mergeTrans = transactionService.GetForceMergeTransaction(batchSize, 3);

            if (mergeTrans != null && mergeTrans.Any())
            {
                var mergeTransGroup = mergeTrans.Chunk2(numberOfBatch);
                var tasks = new List<Task>();

                foreach (var trans in mergeTransGroup)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var mergeTransJson = JsonConvert.SerializeObject(mergeTrans);

                        transactionService.ForceMergeTransaction(mergeTransJson);
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}
