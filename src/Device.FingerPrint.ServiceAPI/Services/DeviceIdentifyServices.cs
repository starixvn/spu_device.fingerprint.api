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
        public void ServiceDeviceIdentify(int batchSize);

        public void MergeTransaction(int batchSize);

        public void ServiceUpdateTransaction(int batchSize);

        public void ServiceUpdateTransactionN(int batchSize);
        
    }

    public class DeviceIdentifyServices: IDeviceIdentifyServices
    {
        private readonly IAppSettings appSettings;
        private readonly ITransactionService transactionService;
        private readonly IRawTransactionService rawTransactionServices;
        private readonly int delayTime = 100;
        private readonly int numberOfBatch = 5;

        public DeviceIdentifyServices(ITransactionService transactionService, IAppSettings appSettings, IRawTransactionService rawTransactionServices)
        {
            this.transactionService = transactionService;
            this.appSettings = appSettings;
            this.rawTransactionServices = rawTransactionServices;
        }

        public void ServiceDeviceIdentify(int batchSize)
        {
            var offsetName = "DeviceIndetify";
            var rawTrans = transactionService.GetDeviceIdentify(batchSize, offsetName);
            if (rawTrans != null && rawTrans.Any())
            {
                DeviceIdentify(rawTrans, offsetName);
            }
        }

        private async void DeviceIdentify(IEnumerable<DeviceTransactionEntity> rawTrans, string offsetName)
        {
            var rawTransHaveFingerPrint = rawTrans.Where(x => x.FingerPrintCode != null).ToList();
            var rawTransNoFingerPrint = rawTrans.Where(x => x.FingerPrintCode == null).ToList();

            var tasks = new List<Task>();
            // Add Task1
            if (rawTransHaveFingerPrint != null && rawTransHaveFingerPrint.Any())
            {
                var task1 = Task.Run(() =>
                {
                    var json = JsonConvert.SerializeObject(rawTransHaveFingerPrint);
                    transactionService.DeviceIdentify(JsonConvert.SerializeObject(rawTransHaveFingerPrint), offsetName);
                });
                await Task.Delay(delayTime);
                tasks.Add(task1);
            }

            // Add Task2
            if (rawTransNoFingerPrint != null && rawTransNoFingerPrint.Any())
            {
                var task2 = Task.Run(() =>
                {
                    var json = JsonConvert.SerializeObject(rawTransNoFingerPrint);
                    transactionService.DeviceIdentifyNoFingerprint(JsonConvert.SerializeObject(rawTransNoFingerPrint));
                });
                await Task.Delay(delayTime);
                tasks.Add(task2);
            }

            // Run Tasks
            if (tasks.Count > 0)
            {
                var taskCompleted = Task.WhenAll(tasks.ToArray());
                await taskCompleted;
            }
        }

        public void MergeTransaction(int batchSize)
        {
            transactionService.MergeTransaction(batchSize);
        }

        public void ServiceUpdateTransaction(int batchSize)
        {
            var offsetName = "RawTransaction";
            var retry = appSettings.RawTransactionRetry;
            RawTransactionConfigModel config = new()
            {
                Size = batchSize,
                Retry = retry,
                OffsetName = offsetName
            };
            var rawTrans = rawTransactionServices.GetRawTrans(config);

            if (rawTrans != null && rawTrans.Any())
            {
                UpdateTransaction(rawTrans, batchSize, retry, offsetName);
            }
        }

        private async void UpdateTransaction(IEnumerable<RawTransactionModel> rawTrans, int batchSize, int retry, string offsetName)
        {
            var result = new List<RawTransactionModel>();
            var tasks = new List<Task>();
            var chunkRawTrans = rawTrans.Chunk2(batchSize / numberOfBatch);

            foreach (var item in chunkRawTrans)
            {
                var records = item.Distinct();
                var task = Task.Run(() =>
                {
                    var json = JsonConvert.SerializeObject(records);
                    var model = new TransactionUpdateConfigModel
                    {
                        Json = json,
                        Retry = retry,
                        OffsetName = offsetName,
                        Offsetvalue = rawTrans.First().Id
                    };
                    transactionService.UpdateTransaction(model);
                });
                await Task.Delay(delayTime);
                tasks.Add(task);
            }
            var taskCompleted = Task.WhenAll(tasks.ToArray());
            await taskCompleted;
        }

        public void ServiceUpdateTransactionN(int batchSize)
        {
            var offsetName = "RawTransactionN";
            RawTransactionConfigModel config = new()
            {
                Size = batchSize,
                OffsetName = offsetName
            };
            var rawTrans = rawTransactionServices.GetRawTransN(config);

            if (rawTrans != null && rawTrans.Any())
            {
                UpdateTransactionN(rawTrans, batchSize, offsetName);
            }
        }

        public async void UpdateTransactionN(IEnumerable<RawTransactionModel> rawTrans, int batchSize, string offsetName)
        {
            var result = new List<RawTransactionModel>();
            var tasks = new List<Task>();
            if (rawTrans != null && rawTrans.Any())
            {
                var chunkRawTrans = rawTrans.Chunk2(batchSize / numberOfBatch);

                foreach (var item in chunkRawTrans)
                {
                    var records = item.Distinct();
                    var task = Task.Run(() =>
                    {
                        var json = JsonConvert.SerializeObject(records);
                        var model = new TransactionUpdateConfigModel
                        {
                            Json = json,
                            OffsetName = offsetName,
                            Offsetvalue = rawTrans.First().Id
                        };
                        transactionService.UpdateTransactionN(model);
                    });
                    await Task.Delay(delayTime);
                    tasks.Add(task);
                }
                var taskCompleted = Task.WhenAll(tasks.ToArray());
                await taskCompleted;
            }
        }
    }
}
