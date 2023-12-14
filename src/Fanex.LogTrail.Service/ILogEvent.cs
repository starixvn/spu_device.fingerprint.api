namespace Fanex.LogTrail.Service
{
    using System.Threading.Tasks;

    public interface ILogEvent
    {
        void WriteEventInfo(string message);

        void WriteEventInfo<T>(T message);

        Task WriteEventInfoAsync<T>(T message);
    }
}
