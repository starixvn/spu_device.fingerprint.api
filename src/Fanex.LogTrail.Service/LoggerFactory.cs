namespace Fanex.LogTrail.Service
{
    using System;
    using Serilog;
    using Serilog.Core;

    public class LoggerFactory
    {
        public static Logger CreateLogger(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException("Log Trail API URL is not set.");
            }

            var logger = new LoggerConfiguration()
                .WriteTo.Http(
                    requestUri: url,
                    queueLimitBytes: null,
                    httpClient: new CustomHttpClients(),
                    configuration: null)
                .CreateLogger();

            return logger;
        }
    }
}
