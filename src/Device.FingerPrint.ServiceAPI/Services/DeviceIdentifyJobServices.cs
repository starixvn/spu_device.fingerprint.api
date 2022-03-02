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

            var rawTrans = transactionService.GetDeviceIdentify(batchSize, numberOfBatch);

            listRawTrans.Add(Task.Run(() =>
            {
                SetDeviceIdentify(rawTrans.Item1, 0);
            }));

            listRawTrans.Add(Task.Run(() =>
            {
                SetDeviceIdentify(rawTrans.Item2, 1);
            }));

            listRawTrans.Add(Task.Run(() =>
            {
                if (rawTrans.Item3 != null && rawTrans.Item3.Any())
                {
                    var rawTransIds = string.Join(",", rawTrans.Item3);

                    transactionService.DeviceIdentifyNoFingerprint(rawTransIds);
                }
            }));

            Task.WaitAll(listRawTrans.ToArray());
        }

        private void SetDeviceIdentify(IEnumerable<DeviceIdentifyEntity> trans, int groupType)
        {
            if (trans != null && trans.Any())
            {
                var groupRawTrans = trans.GroupBy(x => x.BatchNumber);

                var tasks = new List<Task>();

                foreach (var group in groupRawTrans)
                {
                    tasks.Add(ProcessDeviceIdentify(group, groupType));
                }

                if (tasks.Any())
                {
                    Task.WaitAll(tasks.ToArray());
                }
            }
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
    }
}
