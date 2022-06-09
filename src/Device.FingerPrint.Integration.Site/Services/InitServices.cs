using Device.FingerPrint.Features.Subscriber.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Device.FingerPrint.Integration.Site.Services
{
    public static class InitServices
    {
        public static void RegisterService(this IServiceCollection services)
        {
            RegisterSubscriberService(services);
        }

        private static void RegisterSubscriberService(this IServiceCollection services)
        {
            services.AddSingleton<ISubscriberDataAccess, SubscriberDataAccess>();
            services.AddSingleton<ISubscriberServices, SubscriberServices>();
        }
    }
}
