using Device.Identification.Criterias;
using Device.Identification.Encoders;
using Device.Identification.Factory;
using Device.Identification.Model;
using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.Identification.Services
{
    public interface IRecoverDeviceService
    {
        RecoveredDevice GetRecoveredDevice(IList<string> candidateIds, string deviceCode);
    }

    public class RecoverDeviceService : IRecoverDeviceService
    {
        private readonly IDynamicRepository _repository;

        public RecoverDeviceService()
        {
            _repository = new DynamicRepository();
        }

        public RecoveredDevice GetRecoveredDevice(IList<string> candidateIds, string deviceCode)
        {
            var searchingMultiDeviceResult = SearchingMultiDevice(candidateIds, deviceCode);
            var recoveredDevice = TryRecoverDeviceId(candidateIds, searchingMultiDeviceResult);
            recoveredDevice.IsNewDeviceId = searchingMultiDeviceResult == null;

            return recoveredDevice;
        }

        private static SearchingMultiDeviceResult SearchingMultiDevice(IList<string> candidateIds, string deviceCode)
        {
            var criteria = new SearchingMultiDeviceCriteria
            {
                DeviceIds = string.Join(",", candidateIds),
                DeviceCode = !string.IsNullOrEmpty(deviceCode) ? deviceCode : string.Empty
            };

            using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
            return db.QueryEntity<SearchingMultiDeviceResult>(criteria);
        }

        private static RecoveredDevice TryRecoverDeviceId(IList<string> candidateIds, SearchingMultiDeviceResult searchingMultiDeviceResult)
        {
            var recoveredDevice = new RecoveredDevice();

            if (searchingMultiDeviceResult != null)
            {
                recoveredDevice.DeviceId = DeviceEncoder.Encrypt(searchingMultiDeviceResult.RecoverId);
            }
            else
            {
                if (candidateIds.Count > 0)
                {
                    try
                    {
                        recoveredDevice.DeviceId = DeviceEncoder.Encrypt(candidateIds[0]);
                    }
                    catch
                    {
                        recoveredDevice.DeviceId = DeviceFactory.GenerateDeviceId();
                    }
                }
                else
                {
                    recoveredDevice.DeviceId = DeviceFactory.GenerateDeviceId();
                }
            }

            return recoveredDevice;
        }
    }
}
