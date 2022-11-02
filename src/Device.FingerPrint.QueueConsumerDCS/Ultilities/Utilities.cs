using Device.FingerPrint.QueueConsumerDCS.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Text;

namespace Device.FingerPrint.QueueConsumerDCS.Ultilities
{
    public static class Utilities
    {
        public static string GetURLFromDeviceCode(string di)
        {
            if (string.IsNullOrWhiteSpace(di))
            {
                return string.Empty;
            }

            var requestData = di.Split(new[] { "###" }, StringSplitOptions.None);

            var url = requestData.Length >= 8 ? requestData[7] : string.Empty;

            return url;
        }

        public static string GetUserAgentFromDeviceCode(string di)
        {
            if (string.IsNullOrWhiteSpace(di))
            {
                return string.Empty;
            }

            var deviceSplited = di.Split(new[] { "###" }, StringSplitOptions.None);

            return deviceSplited.Length > 3 ? deviceSplited[3] : string.Empty;
        }

        public static bool IsGuidDeviceId(string deviceCode)
        {
            return Guid.TryParse(deviceCode, out _);
        }

        public static string GetDeviceCodeFromCapturedData(string di)
        {
            string[] splitingDI = di.Split(new[] { "###" }, StringSplitOptions.None);
            return splitingDI.Length >= 7 ? splitingDI[6] : string.Empty;
        }

        public static string GetAnalysis(string di)
        {
            string[] splitedDI = di.Split(new[] { "###" }, StringSplitOptions.None);

            return splitedDI.Length > 8 ? splitedDI[8] : string.Empty;
        }

        public static int ConvertBitArray2Int(BitArray bitArray)
        {
            var binString = ToBitString(bitArray);

            return Convert.ToInt32(binString, 2);
        }

        public static string ToBitString(BitArray bits)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bits.Count; i++)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }

        public static IpModel Ip2Long(this string ipAddress)
        {
            var isValid = IPAddress.TryParse(ipAddress, out IPAddress ipAd);

            if (!isValid)
            {
                return new IpModel
                {
                    IpNumber = -1,
                    IsIpV6 = false
                };
            }

            BigInteger ipnum = 0;

            byte[] addrBytes = ipAd.GetAddressBytes();

            if (BitConverter.IsLittleEndian)
            {
                var byteList = new List<byte>(addrBytes);
                byteList.Reverse();
                addrBytes = byteList.ToArray();
            }

            if (addrBytes.Length > 8)
            {
                //IPv6
                ipnum = BitConverter.ToUInt64(addrBytes, 8);
                ipnum <<= 64;
                ipnum += BitConverter.ToUInt64(addrBytes, 0);

                return new IpModel
                {
                    IpNumber = ipnum,
                    IsIpV6 = true
                };
            }
            else
            {
                //IPv4
                ipnum = BitConverter.ToUInt32(addrBytes, 0);

                return new IpModel
                {
                    IpNumber = ipnum,
                    IsIpV6 = false
                };
            }
        }
    }
}
