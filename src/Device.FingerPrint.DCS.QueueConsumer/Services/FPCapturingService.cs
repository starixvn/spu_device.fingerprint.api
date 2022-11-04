using Device.FingerPrint.Features.FPRawTransaction.Services;
using Device.FingerPrint.DCS.QueueConsumer.Model;
using Device.FingerPrint.DCS.QueueConsumer.Services.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Device.FingerPrint.DCS.QueueConsumer.Services
{

    public interface IFPCapturingService
    {
        FPDeviceInfoModel GetDeviceInfo(string deviceCode);

        int InsertTransaction(List<FPRawTransactionModel> rawTrans);
    }

    public class FPCapturingService : IFPCapturingService
    {
        private readonly FPRawTransactionDataAccess capturingData;
        private readonly IDeviceDetectorServices deviceDetectorServices;

        public FPCapturingService(IDeviceDetectorServices deviceDetectorServices)
        {
            capturingData = new FPRawTransactionDataAccess();
            this.deviceDetectorServices = deviceDetectorServices;
        }

        public int InsertTransaction(List<FPRawTransactionModel> rawTrans)
        {
            var model = JsonConvert.SerializeObject(rawTrans);

            return capturingData.InsertTransaction(model);
        }

        public FPDeviceInfoModel GetDeviceInfo(string deviceCode)
        {
            //deviceCode = "eyJEZXZpY2VDb2RlIjoiNjU1MWY5OTdkZmY4MDBkNTI5MGI5NjdlYjhjZDZlOWEzZTA5NDJiNDs0ZWY3ZTY2YWZkMTcyZDBlOGY5OGI1MDdhNjgzYTU3NyIsIkFwcFZlcnNpb24iOiIxMjMiLCJUaW1lWm9uZSI6IlVUQyIsIlVzZXJBZ2VudCI6Ik1vemlsbGEvNS4wIChMaW51eDsgQW5kcm9pZCAxMDsgU00tQTc1MEdOKSBBcHBsZVdlYktpdC81MzcuMzYgKEtIVE1MLCBsaWtlIEdlY2tvKSBDaHJvbWUvMTA2LjAuMC4wIE1vYmlsZSBTYWZhcmkvNTM3LjM2IiwiRGV2aWNlSWQiOiJkOTk5YWU0ODU2MDM0YTdlYjhhODliZjhlNmZiYTY3MCJ9";
            if (string.IsNullOrWhiteSpace(deviceCode))
            {
                return TransformEmptyDeviceInfo();
            }

            var deviceInfo = deviceDetectorServices.GetDeviceInfo(deviceCode);

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

        private FPDeviceInfoModel TransformDeviceInfo(DeviceInfo deviceInfo)
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
