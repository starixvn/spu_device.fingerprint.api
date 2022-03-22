using Device.FingerPrint.Features.Monitor.Services;
using Device.FingerPrint.Features.ServerTransaction.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public static class InitServices
    {
        public static void RegisterService(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterTransactionService(services);
            RegisterRawTransactionService(services);
            RegisterMonitorService(services);
        }

        private static void RegisterTransactionService(this IServiceCollection services)
        {
            services.AddSingleton<ITransactionDataAccess, TransactionDataAccess>();
            services.AddSingleton<ITransactionService, TransactionService>();

            services.AddSingleton<IDeviceIdentifyJobServices, DeviceIdentifyJobServices>();
            services.AddSingleton<ITransactionJobServices, TransactionJobServices>();
        }

        private static void RegisterRawTransactionService(this IServiceCollection services)
        {
            services.AddSingleton<IRawTransactionDataAccess, RawTransactionDataAccess>();
            services.AddSingleton<IRawTransactionService, RawTransactionService>();
        }

        private static void RegisterMonitorService(this IServiceCollection services)
        {
            services.AddSingleton<IMonitorJobServices, MonitorJobServices>();
            services.AddSingleton<IServerTransDailySumDataAccess, ServerTransDailySumDataAccess>();
            services.AddSingleton<IServerTransDailySumServices, ServerTransDailySumServices>();
            services.AddSingleton<IClientTransDailySumDataAccess, ClientTransDailySumDataAccess>();
            services.AddSingleton<IClientTransDailySumServices, ClientTransDailySumServices>();
        }
    }
}
