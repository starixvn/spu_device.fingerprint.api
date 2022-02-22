using Device.FingerPrint.Features.RawTransaction.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IUpdateTransactionServices
    {
        void UpdateTransaction(IEnumerable<RawTransactionModel> rawTrans, int batchSize);
    }

    public class UpdateTransactionServices : IUpdateTransactionServices
    {
        private readonly IRawTransactionService rawTransactionServices;
        private readonly ITransactionService transactionServices;
        private int delayTime = 100;

        public UpdateTransactionServices (ITransactionService transactionServices, IRawTransactionService rawTransactionServices)
        {
            this.transactionServices = transactionServices;
            this.rawTransactionServices = rawTransactionServices;
        }

        public async void UpdateTransaction(IEnumerable<RawTransactionModel> rawTrans, int batchSize)
        {
            var result = new List<RawTransactionModel>();
            var tasks = new List<Task>();
            int retry = 5;
            int numberOfBatch = 5;
            try
            {
                if (rawTrans != null && rawTrans.Any())
                {
                    var chunkRawTrans = rawTrans.Chunk2(batchSize/numberOfBatch);

                    foreach (var item in chunkRawTrans)
                    {
                        var records = item.Distinct();
                        var task = Task.Run(() =>
                        {
                            var json = JsonConvert.SerializeObject(records);
                            ExecuteSP(json, retry);
                        });
                        await Task.Delay(delayTime);
                        tasks.Add(task);
                    }
                    var taskCompleted = Task.WhenAll(tasks.ToArray());
                    await taskCompleted;
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message.ToString();
            }
        }

        private void ExecuteSP(string json, int retry)
        {
            transactionServices.UpdateTransaction(json, retry);
        }
    }
}
