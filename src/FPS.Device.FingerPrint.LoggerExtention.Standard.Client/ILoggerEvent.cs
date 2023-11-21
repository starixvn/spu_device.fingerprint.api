namespace FPS.Device.FingerPrint.LoggerExtention.Standard.Client
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ILoggerEvent
    {
        void WriteEventInfo(string message);

        void WriteEventInfo<T>(T message);

        void WriteMultipleEventInfo<T>(IEnumerable<T> messages);

        Task WriteEventInfoAysnc(string message);

        Task WriteEventInfoAysnc<T>(T message);

        Task WriteMultipleEventInfoAsync<T>(IEnumerable<T> messages);
    }
}
