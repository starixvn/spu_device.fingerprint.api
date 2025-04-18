using Device.Identification.Criterias;
using Device.Identification.Encoders;
using Device.Identification.Factory;
using Device.Identification.Model;
using Fanex.Data;

namespace Device.Identification.Services
{
    public interface IRecoverDeviceService
    {
        RecoveredDevice GetRecoveredDevice(IList<string> candidateIds, string deviceCode);
    }

    public class RecoverDeviceService : IRecoverDeviceService
    {
        public RecoveredDevice GetRecoveredDevice(IList<string> candidateIds, string deviceCode)
        {
            var searchResult = SearchMultiDevice(candidateIds, deviceCode);
            var recoveredDevice = RecoverDeviceId(candidateIds, searchResult);
            recoveredDevice.IsNewDeviceId = searchResult is null;

            return recoveredDevice;
        }

        private static SearchingMultiDeviceResult? SearchMultiDevice(IList<string> candidateIds, string deviceCode)
        {
            var criteria = new SearchingMultiDeviceCriteria
            {
                DeviceIds = string.Join(",", candidateIds),
                DeviceCode = deviceCode ?? string.Empty
            };

            using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
            return db.QueryEntity<SearchingMultiDeviceResult>(criteria);
        }

        private static RecoveredDevice RecoverDeviceId(IList<string> candidateIds, SearchingMultiDeviceResult? result)
        {
            if (result is not null)
            {
                return new RecoveredDevice
                {
                    DeviceId = DeviceEncoder.Encrypt(result.RecoverId)
                };
            }

            var fallbackId = candidateIds.FirstOrDefault();

            try
            {
                var encryptedId = !string.IsNullOrEmpty(fallbackId)
                    ? DeviceEncoder.Encrypt(fallbackId)
                    : DeviceFactory.GenerateDeviceId();

                return new RecoveredDevice { DeviceId = encryptedId };
            }
            catch
            {
                return new RecoveredDevice { DeviceId = DeviceFactory.GenerateDeviceId() };
            }
        }
    }
}
