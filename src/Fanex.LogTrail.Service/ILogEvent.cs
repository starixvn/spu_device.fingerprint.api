namespace Fanex.LogTrail.Service
{
    using System.Collections.Generic;

    public interface ILogEvent
    {
        void WriteEventInfo(string message);

        void WriteEventInfo<T>(T message);

        void WriteMultipleEventInfo<T>(IEnumerable<T> messages);
    }
}
