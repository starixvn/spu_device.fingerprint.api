using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Device.Identification.Configuration
{
    public static class JsonSerializerSettingsFactory
    {
        private static volatile JsonSerializerSettings _instance;

        static JsonSerializerSettingsFactory()
        {
            _instance = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public static JsonSerializerSettings Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}
