using Device.FingerPrint.Features.Monitor.Services;
using Device.FingerPrint.Features.ServerTransaction.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Features.TransformTransaction.Services;
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
            RegisterTransformTransactionService(services);
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
            services.AddSingleton<ITransactionReportDataAccess, TransactionReportDataAccess>();
            services.AddSingleton<ITransactionReportServices, TransactionReportServices>();
        }

        private static void RegisterTransformTransactionService(this IServiceCollection services)
        {
            services.AddSingleton<ITransformJobServices, TransformJobServices>();
            services.AddSingleton<ITransformTransactionDataAccess, TransformTransactionDataAccess>();
            services.AddSingleton<ITransformTransactionService, TransformTransactionService>();
        }
    }
}
