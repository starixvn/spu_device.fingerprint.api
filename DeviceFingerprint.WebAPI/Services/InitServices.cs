using Fps.Device.FingerPrint.Features.FingerPrint.Services;
using Fps.Device.FingerPrint.Features.Transaction.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceFingerprint.WebAPI.Services
{
    public static class InitServices
    {
        public static void RegisterService(this IServiceCollection services)
        {
            RegisterFingerPrintService(services);
            RegisterTransactionService(services);
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
    }
}
