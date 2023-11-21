namespace FPS.Device.FingerPrint.LoggerExtention.Standard.Client
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Serilog;

    public class LoggerEvent : ILoggerEvent
    {
        private readonly ILogger logger;

        public LoggerEvent(string url)
        {
            logger = LoggerFactory.CreateLogger(url);
        }

        public void WriteEventInfo(string message) => Information(message);


        public void WriteEventInfo<T>(T message)
        {
            var json = JsonSerializer.Serialize(message);
            Information(json);
        }

        public void WriteMultipleEventInfo<T>(IEnumerable<T> messages)
        {
            var json = JsonSerializer.Serialize(messages);
            Information(json);
        }

        public async Task WriteEventInfoAysnc(string message) => Information(message);


        public async Task WriteEventInfoAysnc<T>(T message)
        {
            var json = JsonSerializer.Serialize(message);
            Information(json);
        }

        public async Task WriteMultipleEventInfoAsync<T>(IEnumerable<T> messages)
        {
            var json = JsonSerializer.Serialize(messages);
            Information(json);
        }

        private void Information(string message) => logger.Information(message);
    }
}
