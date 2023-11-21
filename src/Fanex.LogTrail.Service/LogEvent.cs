namespace Fanex.LogTrail.Service
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Serilog;

    public class LogEvent : ILogEvent
    {
        private readonly ILogger logger;

        public LogEvent(string url)
        {
            logger = LoggerFactory.CreateLogger(url);
        }

        public void WriteEventInfo(string message) => Information(message);


        public void WriteEventInfo<T>(T message)
        {
            var json = JsonConvert.SerializeObject(message);
            Information(json);
        }

        public void WriteMultipleEventInfo<T>(IEnumerable<T> messages)
        {
            var json = JsonConvert.SerializeObject(messages);
            Information(json);
        }

        private void Information(string message) => logger.Information(message);
    }
}
