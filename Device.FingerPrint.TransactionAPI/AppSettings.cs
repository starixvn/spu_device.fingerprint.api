
namespace Device.FingerPrint.TransactionAPI
{
    using System.ComponentModel;
    using Device.FingerPrint.TransactionAPI.Model;

    public interface IAppSettings
    {
        string QueueName { get; }

        string ExtraQueueName { get; }

        List<SubscriberModel> ExtraSubscribers { get; }

        bool IsUM { get; }

        string SentryDsn { get; }

        string IgnoredSites { get; }

        string GetSubscriberInfo { get; }

        string FPJSPortialAPI { get; }

        bool LogTrailEnable { get; }

        string LogTrailBaseUrl { get; }

        string LogTrailSystemName { get; }

    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly string queueName;

        private readonly string extraQueueName;

        private readonly List<SubscriberModel> extraSubscribers;

        private readonly bool isUM;

        private readonly string sentryDsn;

        private readonly string ignoredSites;

        private readonly string getSubscriberInfo;

        private readonly string fPJSPortialAPI;

        private readonly bool logTrailEnable;

        private readonly string logTrailBaseUrl;

        private readonly string logTrailSystemName;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            queueName = GetValue<string>("RabbitMQ:Queue");

            extraQueueName = GetValue<string>("RabbitMQ:ExtraQueue");

            extraSubscribers = GetExtraSubscribers();

            isUM = GetValue<bool>("IsUM");

            sentryDsn = GetValue<string>("Sentry:Dsn");

            ignoredSites = GetValue<string>("IgnoredSites");

            getSubscriberInfo = GetValue<string>("GetSubscriberUrl");

            fPJSPortialAPI = GetValue<string>("FPJS-Portial-API");

            logTrailEnable = GetValue<bool>("LogTrail:Enable");

            logTrailBaseUrl = GetValue<string>("LogTrail:BaseUrl");

            logTrailSystemName = GetValue<string>("LogTrail:SystemName");
        }

        public string QueueName => queueName;

        public string ExtraQueueName => extraQueueName;

        public List<SubscriberModel> ExtraSubscribers => extraSubscribers;

        public bool IsUM => isUM;

        public string SentryDsn => sentryDsn;

        public string IgnoredSites => ignoredSites;

        public string GetSubscriberInfo => getSubscriberInfo;

        public string FPJSPortialAPI => fPJSPortialAPI;

        public bool LogTrailEnable => logTrailEnable;

        public string LogTrailBaseUrl => logTrailBaseUrl;

        public string LogTrailSystemName => logTrailSystemName;

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

        public List<SubscriberModel> GetExtraSubscribers()
        {
            var listExtraSubscribersModel = new List<SubscriberModel>();
            var extraSubscribers = GetValue<string>("DCS-ExtraSubscribers").Split(',');
            if (extraSubscribers != null && extraSubscribers.Any())
            {
                foreach (var subscriber in extraSubscribers)
                {
                    var subs = subscriber.Split(':');

                    listExtraSubscribersModel.Add(new SubscriberModel
                    {
                        SubscriberId = Convert.ToInt32(subs[0]),
                        SubscriberName = subs[1]
                    });
                }
            }

            return listExtraSubscribersModel;
        }

    }
}
