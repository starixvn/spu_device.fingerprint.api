using Device.FingerPrint.Features.RawTransaction.Services;
using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IDeviceIdentifyServices
    {
        public void DeviceIdentify(int batchSize);

        public void MergeTransaction(int batchSize);

        public void UpdateTransaction(int batchSize, int numberOfBatch);

        public void UpdateTransactionN(int batchSize, int numberOfBatch);
    }

    public class DeviceIdentifyServices: IDeviceIdentifyServices
    {
        private readonly IAppSettings appSettings;
        private readonly ITransactionService transactionService;
        private readonly IRawTransactionService rawTransactionServices;

        public DeviceIdentifyServices(ITransactionService transactionService, IAppSettings appSettings, IRawTransactionService rawTransactionServices)
        {
            this.transactionService = transactionService;
            this.appSettings = appSettings;
            this.rawTransactionServices = rawTransactionServices;
        }

        public void DeviceIdentify(int batchSize)
        {
            var offsetName = "DeviceIndetify";
            var rawTrans = transactionService.GetDeviceIdentify(batchSize, offsetName);
            if (rawTrans != null && rawTrans.Any())
            {
                ProcessDeviceIdentify(rawTrans, offsetName);
            }
        }

        private async void ProcessDeviceIdentify(IEnumerable<DeviceTransactionEntity> rawTrans, string offsetName)
        {
            var groupRawTrans = rawTrans.Select((v, i) => new { v, checkDevice = v.FingerPrintCode == null ? 0 : 1 })
                                        .GroupBy(x => x.checkDevice)
                                        .Select(g => g.Select(x => x.v));

            if (groupRawTrans != null && groupRawTrans.Any())
            {
                var tasks = new List<Task>();
                foreach (var item in groupRawTrans)
                {
                    var records = item.Distinct();
                    if (records != null && records.Any())
                    {
                        var firstItem = records.First();
                        if (firstItem.FingerPrintCode == null)
                        {
                            var task = Task.Run(() =>
                            {
                                transactionService.DeviceIdentifyNoFingerprint(JsonConvert.SerializeObject(records));
                            });
                            tasks.Add(task);
                        }
                        else
                        {
                            var task = Task.Run(() =>
                            {
                                transactionService.DeviceIdentify(JsonConvert.SerializeObject(records), offsetName);
                            });
                            tasks.Add(task);
                        }
                    }
                }

                // Run Tasks
                if (tasks.Count > 0)
                {
                    var taskCompleted = Task.WhenAll(tasks.ToArray());
                    await taskCompleted;
                }
            }
        }

        public void MergeTransaction(int batchSize)
        {
            transactionService.MergeTransaction(batchSize);
        }

        public void UpdateTransaction(int batchSize, int numberOfBatch)
        {
            var offsetName = "RawTransaction";
            var retry = appSettings.RawTransactionRetry;
            var rawTrans = rawTransactionServices.GetRawTrans(batchSize * numberOfBatch, retry, offsetName);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransaction(rawTrans, batchSize, retry, offsetName);
            }
        }

        private async void ProcessUpdateTransaction(IEnumerable<RawTransactionModel> rawTrans, int batchSize, int retry, string offsetName)
        {
            var result = new List<RawTransactionModel>();
            var tasks = new List<Task>();
            var chunkRawTrans = rawTrans.Chunk2(batchSize);

            foreach (var item in chunkRawTrans)
            {
                var records = item.Distinct();
                var task = Task.Run(() =>
                {
                    var rawTransJson = JsonConvert.SerializeObject(records);
                    var offsetvalue = rawTrans.Min(x => x.Id);
                    transactionService.UpdateTransaction(rawTransJson, retry, offsetName, offsetvalue);
                });
                tasks.Add(task);
            }
            var taskCompleted = Task.WhenAll(tasks.ToArray());
            await taskCompleted;
        }

        public void UpdateTransactionN(int batchSize, int numberOfBatch)
        {
            var offsetName = "RawTransactionN";
            var rawTrans = rawTransactionServices.GetRawTransN(batchSize * numberOfBatch, offsetName);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransactionN(rawTrans, batchSize, offsetName);
            }
        }

        private async void ProcessUpdateTransactionN(IEnumerable<RawTransactionModel> rawTrans, int batchSize, string offsetName)
        {
            var result = new List<RawTransactionModel>();
            var tasks = new List<Task>();
            if (rawTrans != null && rawTrans.Any())
            {
                var chunkRawTrans = rawTrans.Chunk2(batchSize);

                foreach (var item in chunkRawTrans)
                {
                    var records = item.Distinct();
                    var task = Task.Run(() =>
                    {
                        var rawTransJson = JsonConvert.SerializeObject(records);
                        var offsetvalue = rawTrans.Min(x => x.Id);
                        transactionService.UpdateTransactionN(rawTransJson, offsetName, offsetvalue);
                    });
                    tasks.Add(task);
                }
                var taskCompleted = Task.WhenAll(tasks.ToArray());
                await taskCompleted;
            }
        }
    }
}
