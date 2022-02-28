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
        public void DeviceIdentify(int batchSize);

        public void MergeTransaction(int batchSize);
    }

    public class DeviceIdentifyJobServices : IDeviceIdentifyJobServices
    {
        
        private readonly ITransactionService transactionService;
        

        public DeviceIdentifyJobServices(ITransactionService transactionService, IAppSettings appSettings, IRawTransactionService rawTransactionServices)
        {
            this.transactionService = transactionService;
        }

        public void DeviceIdentify(int batchSize)
        {
            var rawTrans = transactionService.GetDeviceIdentify(batchSize, "");

            if (rawTrans != null && rawTrans.Any())
            {
                ProcessDeviceIdentify(rawTrans,"");

                //MergeTransactionNew(rawTrans);
            }
        }

        public void MergeTransaction(int batchSize)
        {
            transactionService.MergeTransaction(batchSize);
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
