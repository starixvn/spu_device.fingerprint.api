using Device.FingerPrint.Features.FPRawTransaction.Services;
using Device.FingerPrint.QueueConsumerDCS.Model;
using Fanex.DeviceDetector.Factory;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Device.FingerPrint.QueueConsumerDCS.Services
{

    public interface IFPCapturingService
    {
        FPDeviceInfoModel GetDeviceInfo(string deviceCode);

        int InsertTransaction(List<FPRawTransactionModel> rawTrans);
    }

    public class FPCapturingService: IFPCapturingService
    {
        private readonly FPRawTransactionDataAccess capturingData;

        public FPCapturingService()
        {
            capturingData = new FPRawTransactionDataAccess();
        }

        public int InsertTransaction(List<FPRawTransactionModel> rawTrans)
        {
            var model = JsonConvert.SerializeObject(rawTrans);

            return capturingData.InsertTransaction(model);
        }

        public FPDeviceInfoModel GetDeviceInfo(string deviceCode)
        {
            if (string.IsNullOrWhiteSpace(deviceCode))
            {
                return TransformEmptyDeviceInfo();
            }

            var deviceInfo = DeviceFactory.GetDeviceInfo(deviceCode);

            if (deviceInfo != null)
            {
                return TransformDeviceInfo(deviceInfo);
            }

            return TransformEmptyDeviceInfo();
        }

        private FPDeviceInfoModel TransformEmptyDeviceInfo()
        {
            return new FPDeviceInfoModel
            {
                AppVersion = string.Empty,
                CapturedDate = 0,
                DeviceFPCode = string.Empty,
                DeviceNo = string.Empty,
                HRef = string.Empty,
                NA = string.Empty,
                Screen = new FPScreenInfoModel
                {
                    ColorDepth = 0,
                    Height = 0,
                    Width = 0
                },
                TimeZone = string.Empty,
                UserAgent = string.Empty,
                InvalidDeviceId = true
            };
        }

        private FPDeviceInfoModel TransformDeviceInfo(Fanex.DeviceDetector.Models.DeviceInfo deviceInfo)
        {
            return new FPDeviceInfoModel
            {
                AppVersion = deviceInfo.AppVersion,
                CapturedDate = deviceInfo.CapturedDate,
                DeviceFPCode = deviceInfo.DeviceCode,
                DeviceNo = deviceInfo.DeviceId,
                HRef = deviceInfo.HRef,
                NA = deviceInfo.NA,
                TimeZone = deviceInfo.TimeZone,
                UserAgent = deviceInfo.UserAgent,
                Screen = deviceInfo.Screen == null
                    ? new FPScreenInfoModel
                    {
                        ColorDepth = 0,
                        Height = 0,
                        Width = 0
                    }
                    : new FPScreenInfoModel
                    {
                        ColorDepth = deviceInfo.Screen.ColorDepth,
                        Height = deviceInfo.Screen.Height,
                        Width = deviceInfo.Screen.Width
                    }
            };
        }
    }
}
