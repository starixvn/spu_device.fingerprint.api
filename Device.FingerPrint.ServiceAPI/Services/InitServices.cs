using Fps.Device.FingerPrint.Features.Transaction.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public static class InitServices
    {
        public static void RegisterService(this IServiceCollection services)
        {   
            RegisterTransactionService(services);            
        }

        private static void RegisterTransactionService(this IServiceCollection services)
        {
            services.AddSingleton<ITransactionDataAccess, TransactionDataAccess>();
            services.AddSingleton<ITransactionService, TransactionService>();
            services.AddSingleton<IRecoverDeviceServices, RecoverDeviceServices>();
        }
    }
}
