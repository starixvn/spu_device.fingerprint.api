namespace Fanex.LogTrail.Service
{
    using System.Threading.Tasks;
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

        public async Task WriteEventInfoAsync<T>(T message)
        {
            var json = JsonConvert.SerializeObject(message);
            Information(json);
        }

        private void Information(string message) => logger.Information(message);
    }
}
