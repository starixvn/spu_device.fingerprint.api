using Device.FingerPrint.Features.RawTransaction.Services;
using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface ITransactionJobServices
    {
        public void UpdateTransaction(int batchSize, int numberOfBatch);

        public void UpdateTransactionN(int batchSize, int numberOfBatch);
    }

    public class TransactionJobServices : ITransactionJobServices
    {
        private readonly ITransactionService transactionService;
        private readonly IRawTransactionService rawTransactionServices;
        private readonly IAppSettings appSettings;

        public TransactionJobServices(IAppSettings appSettings, ITransactionService transactionService, IRawTransactionService rawTransactionServices)
        {
            this.transactionService = transactionService;
            this.rawTransactionServices = rawTransactionServices;
            this.appSettings = appSettings;
        }

        public void UpdateTransaction(int batchSize, int numberOfBatch)
        {
            var rawTrans = rawTransactionServices.GetRawTransaction(batchSize * numberOfBatch, appSettings.RawTransactionRetry, (int)Offset.RawTransaction);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransaction(rawTrans, batchSize, appSettings.RawTransactionRetry, (int)Offset.RawTransaction);
            }
        }

        public void UpdateTransactionN(int batchSize, int numberOfBatch)
        {
            var rawTrans = rawTransactionServices.GetRawTransactionN(batchSize * numberOfBatch, (int)Offset.RawTransactionN);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransactionN(rawTrans, batchSize, (int)Offset.RawTransactionN);
            }
        }

        private void ProcessUpdateTransactionN(IEnumerable<RawTransactionModel> rawTrans, int batchSize, int offsetId)
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

                Task.WhenAll(tasks.ToArray());
            }
        }

        private void ProcessUpdateTransaction(IEnumerable<RawTransactionModel> rawTrans, int batchSize, int retry, int offsetId)
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

                Task.WhenAll(tasks.ToArray());
            }
        }
    }
}
