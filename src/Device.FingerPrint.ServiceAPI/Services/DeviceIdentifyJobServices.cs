using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Fanex.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IDeviceIdentifyJobServices
    {
        public void DeviceIdentify(int batchSize, int numberOfBatch);
    }

    public class DeviceIdentifyJobServices : IDeviceIdentifyJobServices
    {
        private readonly ITransactionService transactionService;

        public DeviceIdentifyJobServices(ITransactionService transactionService)
        {
            this.transactionService = transactionService;
        }

        public void DeviceIdentify(int batchSize, int numberOfBatch)
        {
            var listRawTrans = new List<Task>();

            var rawTrans = transactionService.GetDeviceIdentify(batchSize, numberOfBatch);

            listRawTrans.Add(Task.Run(() =>
            {
                try
                {
                    SetDeviceIdentify(rawTrans.Item1, 1);
                }
                catch (Exception ex)
                {
                    var rawTransIds = string.Join(",", rawTrans.Item1);
                    LogService.Error(ex.Message + "\n" + rawTransIds, ex);
                }
            }));

            listRawTrans.Add(Task.Run(() =>
            {
                try
                {
                    SetDeviceIdentify(rawTrans.Item2, 0);
                }
                catch (Exception ex)
                {
                    var rawTransIds = string.Join(",", rawTrans.Item2);
                    LogService.Error(ex.Message + "\n" + rawTransIds, ex);
                }
            }));

            listRawTrans.Add(Task.Run(() =>
            {
                if (rawTrans.Item3 != null && rawTrans.Item3.Any())
                {
                    var rawTransIds = string.Join(",", rawTrans.Item3);
                    try
                    {
                        transactionService.DeviceIdentifyNoFingerprint(rawTransIds);
                    }
                    catch (Exception ex)
                    {
                        LogService.Error(ex.Message + "\n" + rawTransIds, ex);
                    }
                }
            }));

            Task.WaitAll(listRawTrans.ToArray());
        }

        private void SetDeviceIdentify(IEnumerable<DeviceIdentifyEntity> trans, int groupType)
        {
            if (trans != null && trans.Any())
            {
                var groupRawTrans = trans.GroupBy(x => x.BatchNumber);

                foreach (var group in groupRawTrans)
                {
                    ProcessDeviceIdentify(group, groupType);
                }
            }
        }

        private void ProcessDeviceIdentify(IEnumerable<DeviceIdentifyEntity> rawTrans, int groupType)
        {
            var rawTransJson = JsonConvert.SerializeObject(rawTrans);

            if (groupType == 1)
            {
                transactionService.DeviceIdentify(rawTransJson);
            }
            else if (groupType == 0)
            {
                transactionService.DeviceIdentifyNoGroup(rawTransJson);
            }
        }
    }
}
