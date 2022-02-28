using Device.FingerPrint.Features.RawTransaction.Services;
using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

    public class DeviceIdentifyServices : IDeviceIdentifyServices
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
            var rawTrans = transactionService.GetDeviceIdentify(batchSize, Offset.DeviceIndetify);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessDeviceIdentify(rawTrans, Offset.DeviceIndetify);
                MergeTransactionNew(rawTrans);
            }
        }

        public void MergeTransaction(int batchSize)
        {
            transactionService.MergeTransaction(batchSize);
        }

        public void UpdateTransaction(int batchSize, int numberOfBatch)
        {
            var rawTrans = rawTransactionServices.GetRawTrans(batchSize * numberOfBatch, appSettings.RawTransactionRetry, Offset.RawTransaction);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransaction(rawTrans, batchSize, appSettings.RawTransactionRetry, Offset.RawTransaction);
            }
        }
        public void UpdateTransactionN(int batchSize, int numberOfBatch)
        {
            var rawTrans = rawTransactionServices.GetRawTransN(batchSize * numberOfBatch, Offset.RawTransactionN);

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessUpdateTransactionN(rawTrans, batchSize, Offset.RawTransactionN);
            }
        }

        private void ProcessUpdateTransaction(IEnumerable<RawTransactionModel> rawTrans, int batchSize, int retry, string offsetName)
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

                        transactionService.UpdateTransaction(rawTransJson, retry, offsetName, offsetValue);
                    });

                    tasks.Add(task);
                }

                Task.WhenAll(tasks.ToArray());
            }
        }

        private void ProcessUpdateTransactionN(IEnumerable<RawTransactionModel> rawTrans, int batchSize, string offsetName)
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
                        transactionService.UpdateTransactionN(rawTransJson, offsetName, offsetvalue);
                    });

                    tasks.Add(task);
                }

                Task.WhenAll(tasks.ToArray());
            }
        }

        private void ProcessDeviceIdentify(IEnumerable<DeviceTransactionEntity> rawTrans, string offsetName)
        {

            if (rawTrans != null && rawTrans.Any())
            {
                var tasks = new List<Task>();

                var groupByRawTrans = rawTrans.GroupBy(x => x.FingerPrintCode != null);

                foreach (var group in groupByRawTrans)
                {
                    var existingFingerPrint = JsonConvert.SerializeObject(group);

                    if (group.Key)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            transactionService.DeviceIdentify(existingFingerPrint, offsetName);
                        }));
                    }
                    else
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            transactionService.DeviceIdentifyNoFingerprint(existingFingerPrint);
                        }));
                    }
                }

                Task.WhenAll(tasks.ToArray());
            }
        }

        private void MergeTransactionNew(IEnumerable<DeviceTransactionEntity> rawTrans)
        {
            var transJson = JsonConvert.SerializeObject(rawTrans);
            transactionService.MergeTransactionNew(transJson);
        }
    }
}
