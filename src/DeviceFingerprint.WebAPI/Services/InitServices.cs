using Device.FingerPrint.Features.FingerPrint.Services;
using Device.FingerPrint.Features.RawTransaction.Services;
using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Device.Fingerprint.WebAPI.Services
{
    public static class InitServices
    {
        public static void RegisterService(this IServiceCollection services)
        {
            RegisterFingerPrintService(services);
            RegisterTransactionService(services);
            RegisterRawTransactionService(services);
            RegisterSubscriberService(services);
            RegisterKafkaService(services);
        }

        private static void RegisterFingerPrintService(this IServiceCollection services)
        {
            services.AddSingleton<IFingerPrintDataAccess, FingerPrintDataAccess>();
            services.AddSingleton<IFingerPrintService, FingerPrintService>();
        }

        private static void RegisterTransactionService(this IServiceCollection services)
        {
            services.AddSingleton<ITransactionDataAccess, TransactionDataAccess>();
            services.AddSingleton<ITransactionService, TransactionService>();
        }

        private static void RegisterRawTransactionService(this IServiceCollection services)
        {
            services.AddSingleton<IRawTransactionDataAccess, RawTransactionDataAccess>();
            services.AddSingleton<IRawTransactionService, RawTransactionService>();
        }

        private static void RegisterSubscriberService(this IServiceCollection services)
        {
            services.AddSingleton<ISubscriberDataAccess, SubscriberDataAccess>();
            services.AddSingleton<ISubscriberServices, SubscriberServices>();
        }

        private static void RegisterKafkaService(this IServiceCollection services)
        {
            services.AddSingleton<IKafkaServices, KafkaServices>();
        }
    }
}
