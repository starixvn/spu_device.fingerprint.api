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
        void UpdateRawTransaction(IEnumerable<RawTransactionModel> rawTrans, int batchSize, int retry);
        void UpdateRawTransactionN(IEnumerable<RawTransactionModel> rawTrans, int batchSize);
    }

    public class UpdateTransactionServices : IUpdateTransactionServices
    {
        private readonly ITransactionService transactionServices;
        private readonly int delayTime = 100;

        public UpdateTransactionServices (ITransactionService transactionServices)
        {
            this.transactionServices = transactionServices;
        }

        public async void UpdateRawTransaction(IEnumerable<RawTransactionModel> rawTrans, int batchSize, int retry)
        {
            var result = new List<RawTransactionModel>();
            var tasks = new List<Task>();
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
                            ExecuteRawTransaction(json, retry);
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

        private void ExecuteRawTransaction(string json, int retry)
        {
            transactionServices.UpdateTransaction(json, retry);
        }

        public async void UpdateRawTransactionN(IEnumerable<RawTransactionModel> rawTrans, int batchSize)
        {
            var result = new List<RawTransactionModel>();
            var tasks = new List<Task>();
            int numberOfBatch = 5;
            try
            {
                if (rawTrans != null && rawTrans.Any())
                {
                    var chunkRawTrans = rawTrans.Chunk2(batchSize / numberOfBatch);

                    foreach (var item in chunkRawTrans)
                    {
                        var records = item.Distinct();
                        var task = Task.Run(() =>
                        {
                            var json = JsonConvert.SerializeObject(records);
                            ExecuteRawTransactionN(json);
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

        private void ExecuteRawTransactionN(string json)
        {
            transactionServices.UpdateTransactionN(json);
        }
    }
}
