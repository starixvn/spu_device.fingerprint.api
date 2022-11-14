using Microsoft.Extensions.DependencyInjection;

namespace Device.FingerPrint.PortalAPI.Services
{
    public static class InitServices
    {
        public static void RegisterService(this IServiceCollection services)
        {
            InitService(services);
            RegisterQueueService(services);
        }

        private static void InitService(this IServiceCollection services)
        {
        }

        private static void RegisterQueueService(this IServiceCollection services)
        {
            services.AddSingleton<IEasyNetQService, EasyNetQService>();
        }
    }
}
