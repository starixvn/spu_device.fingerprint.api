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
            services.AddSingleton<ITransactionService, TransactionService>();
            services.AddSingleton<ITransactionAttributeService, TransactionAttributeService>();
            services.AddSingleton<IBrowserInfoService, BrowserInfoService>();
        }

        private static void RegisterQueueService(this IServiceCollection services)
        {
            services.AddSingleton<IEasyNetQService, EasyNetQService>();
        }
    }
}
