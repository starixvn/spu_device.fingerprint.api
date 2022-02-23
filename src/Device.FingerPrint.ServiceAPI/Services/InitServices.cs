using Device.FingerPrint.Features.Offset.Services;
using Device.FingerPrint.Features.RawTransaction.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public static class InitServices
    {
        public static void RegisterService(this IServiceCollection services)
        {   
            RegisterTransactionService(services);            
            RegisterRawTransactionService(services);            
            RegisterOffsetService(services);            
        }

        private static void RegisterTransactionService(this IServiceCollection services)
        {
            services.AddSingleton<ITransactionDataAccess, TransactionDataAccess>();
            services.AddSingleton<ITransactionService, TransactionService>();
            services.AddSingleton<IDeviceIdentifyServices, DeviceIdentifyServices>();
        }

        private static void RegisterRawTransactionService(this IServiceCollection services)
        {
            services.AddSingleton<IRawTransactionDataAccess, RawTransactionDataAccess>();
            services.AddSingleton<IRawTransactionService, RawTransactionService>();
        }

        private static void RegisterOffsetService(this IServiceCollection services)
        {
            services.AddSingleton<IOffsetDataAccess, OffsetDataAccess>();
            services.AddSingleton<IOffsetService, OffsetService>();
        }
    }
}
