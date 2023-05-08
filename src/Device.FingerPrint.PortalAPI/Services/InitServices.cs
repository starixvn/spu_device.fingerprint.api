using Device.FingerPrint.Core.Services;
using Device.FingerPrint.Features.ClientTransaction.Services;
using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.PortalAPI.Ultilities;
using Microsoft.Extensions.DependencyInjection;

namespace Device.FingerPrint.PortalAPI.Services
{
    public static class InitServices
    {
        public static void RegisterService(this IServiceCollection services)
        {
            InitService(services);
            RegisterQueueService(services);
            RegisterSubscriberService(services);
            RegisterLogExeption(services);
        }

        private static void InitService(this IServiceCollection services)
        {
            services.AddSingleton<ITransactionService, TransactionService>();
            services.AddSingleton<ITransactionAttributeService, TransactionAttributeService>();
            services.AddSingleton<IBrowserInfoService, BrowserInfoService>();
            services.AddSingleton<ICrypto, Crypto>();
            services.AddSingleton<ISubtleCryptService, SubtleCryptService>();
        }

        private static void RegisterQueueService(this IServiceCollection services)
        {
            services.AddSingleton<IEasyNetQService, EasyNetQService>();
        }

        private static void RegisterSubscriberService(this IServiceCollection services)
        {
            services.AddSingleton<ISubscriberDataAccess, SubscriberDataAccess>();
            services.AddSingleton<ISubscriberServices, SubscriberServices>();
        }

        private static void RegisterLogExeption(this IServiceCollection services)
        {
            services.AddSingleton<IClientTransactionDataAccess, ClientTransactionDataAccess>();
            services.AddSingleton<IClientTransactionService, ClientTransactionService>();
        }
    }
}
