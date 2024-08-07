
namespace Device.FingerPrint.Gateway
{
    using Device.FingerPrint.Gateway.Model;
    using System.ComponentModel;

    public interface IAppSettings
    {     

        bool IsUM { get; }

        string SentryDsn { get; }      

        string GetSubscriberInfo { get; }

        List<SubscriberModel> ExtraSubscribers { get; }

        string LogstashUrl { get; }
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;      

        private readonly List<SubscriberModel> extraSubscribers;

        private readonly bool isUM;

        private readonly string sentryDsn;       

        private readonly string getSubscriberInfo;    
        
        private readonly string logstashUrl;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;          

            extraSubscribers = GetExtraSubscribers();

            isUM = GetValue<bool>("IsUM");

            sentryDsn = GetValue<string>("Sentry:Dsn");        

            getSubscriberInfo = GetValue<string>("GetSubscriberUrl");

            logstashUrl = GetValue<string>("Logstash:Url");
        }

        public List<SubscriberModel> ExtraSubscribers => extraSubscribers;

        public bool IsUM => isUM;

        public string SentryDsn => sentryDsn;       

        public string GetSubscriberInfo => getSubscriberInfo;

        public string LogstashUrl => logstashUrl;

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
            var extraSubscribers = GetValue<string>("DCSExtraSubscribers").Split(',');
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
