using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Device.Fingerprint.WebAPI.Services
{
    public static class InitServices
    {
        public static void RegisterService(this IServiceCollection services)
        {
            RegisterTransactionService(services);
            RegisterSubscriberService(services);
            RegisterQueueService(services);
            RegisterExceptionService(services);
        }

        private static void RegisterTransactionService(this IServiceCollection services)
        {
            services.AddSingleton<ITransactionDataAccess, TransactionDataAccess>();
            services.AddSingleton<ITransactionService, TransactionService>();
        }

        private static void RegisterSubscriberService(this IServiceCollection services)
        {
            services.AddSingleton<ISubscriberDataAccess, SubscriberDataAccess>();
            services.AddSingleton<ISubscriberServices, SubscriberServices>();
        }

        private static void RegisterQueueService(this IServiceCollection services)
        {
            services.AddSingleton<IEasyNetQService, EasyNetQService>();
        }

        private static void RegisterExceptionService(this IServiceCollection services)
        {
            services.AddSingleton<ITransactionDataAccess, TransactionDataAccess>();
            services.AddSingleton<ITransactionService, TransactionService>();
        }
    }
}
