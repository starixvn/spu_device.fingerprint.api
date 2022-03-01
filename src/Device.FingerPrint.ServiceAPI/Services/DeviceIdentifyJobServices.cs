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
    public interface IDeviceIdentifyJobServices
    {
        public void DeviceIdentify(int batchSize, int numberOfBatch);

        public void MergeTransaction(int batchSize);
    }

    public class DeviceIdentifyJobServices : IDeviceIdentifyJobServices
    {

        private readonly ITransactionService transactionService;


        public DeviceIdentifyJobServices(ITransactionService transactionService, IAppSettings appSettings, IRawTransactionService rawTransactionServices)
        {
            this.transactionService = transactionService;
        }

        public void DeviceIdentify(int batchSize, int numberOfBatch)
        {
            var listRawTrans = new List<Task>();

            var rawTrans = transactionService.GetDeviceIdentify(batchSize);            

            listRawTrans.Add(Task.Run(() =>
            {
                if (rawTrans.Item1 != null && rawTrans.Item1.Any())
                {
                    var groupRawTrans = rawTrans.Item1.GroupBy(x => x.GroupId);

                    var tasks = new List<Task>();

                    foreach (var group in groupRawTrans)
                    {
                        if (group.Key != 0)
                        {
                            tasks.Add(ProcessDeviceIdentify(group, group.Key));
                        }
                        else
                        {
                            var chunkGroup = group.Chunk2(numberOfBatch);

                            foreach (var chunk in chunkGroup)
                            {
                                if (chunk != null && chunk.Any())
                                {
                                    tasks.Add(ProcessDeviceIdentify(chunk, 0));
                                }
                            }

                        }
                    }

                    if (tasks.Any())
                    {
                        Task.WaitAll(tasks.ToArray());
                    }
                }
            }));

            listRawTrans.Add(Task.Run(() =>
            {
                if (rawTrans.Item2 != null && rawTrans.Item2.Any())
                {
                    var rawTransIds = string.Join(",", rawTrans.Item2);

                    transactionService.DeviceIdentifyNoFingerprint(rawTransIds);
                }
            }));

            Task.WaitAll(listRawTrans.ToArray());
        }

        public void MergeTransaction(int batchSize)
        {
            transactionService.MergeTransaction(batchSize);
        }

        private Task ProcessDeviceIdentify(IEnumerable<DeviceIdentifyEntity> rawTrans, int groupId)
        {
            var rawTransJson = JsonConvert.SerializeObject(rawTrans);

            return Task.Run(() =>
            {
                transactionService.DeviceIdentify(rawTransJson, groupId);
            });
        }

        private void MergeTransactionNew(IEnumerable<DeviceIdentifyEntity> rawTrans)
        {
            var transJson = JsonConvert.SerializeObject(rawTrans);
            transactionService.MergeTransactionNew(transJson);
        }
    }
}
