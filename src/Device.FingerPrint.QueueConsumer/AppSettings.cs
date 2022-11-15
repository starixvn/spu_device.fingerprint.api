using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace Device.FingerPrint.QueueConsumer
{
    public interface IAppSettings
    {
        string ApiUrl { get; }

        string SPName { get; }

        string SentryDsn { get; }

        string Environment { get; }

        int TransBatchSize { get; }

        int ClientTransBatchSize { get; }

        int ClientTransLogsBatchSize { get; }

        int ServerTransBatchSize { get; }

        int FPAttributeBatchSize { get; }

        int BrowserInfoBatchSize { get; }

        IConfiguration Configuration { get; }

    }
    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;
        private readonly string spName;
        private readonly string apiUrl;
        private readonly string sentryDsn;
        private readonly string environment;
        private readonly int transBatchSize;
        private readonly int clientTransBatchSize;
        private readonly int clientTransLogsBatchSize;
        private readonly int serverTransBatchSize;
        private readonly int fpAttributeBatchSize;
        private readonly int browserInfoBatchSize;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            apiUrl = GetValue<string>("AppConfig:ApiUrl");

            spName = GetValue<string>("AppConfig:SPName");

            sentryDsn = GetValue<string>("Sentry:Dsn");

            environment = GetValue<string>("AppConfig:Environment");

            transBatchSize = GetValue<int>("Consumer:TransBatchSize");

            clientTransBatchSize = GetValue<int>("Consumer:ClientTrans");

            clientTransLogsBatchSize = GetValue<int>("Consumer:ClientTransLogs");

            serverTransBatchSize = GetValue<int>("Consumer:ServerTrans");

            fpAttributeBatchSize = GetValue<int>("Consumer:FPAttribute");

            browserInfoBatchSize = GetValue<int>("Consumer:BrowserInfo");
        }

        public string ApiUrl => apiUrl;

        public string SPName => spName;

        public string SentryDsn => sentryDsn;

        public string Environment => environment;

        public int TransBatchSize => transBatchSize;

        public int ClientTransBatchSize => clientTransBatchSize;

        public IConfiguration Configuration => configuration;

        public int ServerTransBatchSize => serverTransBatchSize;

        public int FPAttributeBatchSize => fpAttributeBatchSize;

        public int BrowserInfoBatchSize => browserInfoBatchSize;

        public int ClientTransLogsBatchSize => clientTransLogsBatchSize;

        public T GetValue<T>(string key)
        {
            try
            {
                var value = configuration[key];

                var typeConverter = TypeDescriptor.GetConverter(typeof(T));

                return (T)typeConverter.ConvertFromString(value);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Key: {key}", ex);
            }
        }
    }
}
