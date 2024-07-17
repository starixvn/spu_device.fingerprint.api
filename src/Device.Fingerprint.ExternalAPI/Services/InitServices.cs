using Device.Fingerprint.ExternalAPI.Filters;
using Device.Fingerprint.ExternalAPI.Services;
using Device.FingerPrint.Features.RiskLevel.Services;
using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Device.Fingerprint.WebAPI.Services
{
    public static class InitServices
    {
        public static void RegisterService(this IServiceCollection services)
        {
            RegisterTransactionService(services);
            RegisterSubscriberService(services);
            RegisterRiskLevelService(services);
            RegisterRequestCounterFilter(services);
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

        private static void RegisterRiskLevelService(this IServiceCollection services)
        {
            services.AddSingleton<IRiskLevelDataAccess, RiskLevelDataAccess>();
            services.AddSingleton<IRiskLevelService, RiskLevelService>();
        }

        private static void RegisterRequestCounterFilter(this IServiceCollection services)
        {
            services.AddSingleton<RequestCounterService>();

            services.AddScoped<RequestCounterFilter>();            
        }
    }
}
