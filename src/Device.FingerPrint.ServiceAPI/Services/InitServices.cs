namespace Device.FingerPrint.ServiceAPI.Services
{
    using Device.FingerPrint.Features.Archive.Services;
    using Device.FingerPrint.Features.BrowserInfo.Services;
    using Device.FingerPrint.Features.FPDevices.Services;
    using Device.FingerPrint.Features.FPTransactions.Services;
    using Device.FingerPrint.Features.Monitor.Services;
    using Device.FingerPrint.Features.Offset.Services;
    using Device.FingerPrint.Features.RawUserAgent.Services;
    using Device.FingerPrint.Features.ServerTransaction.Services;
    using Device.FingerPrint.Features.SummaryTransaction.Services;
    using Device.FingerPrint.Features.Transaction.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class InitServices
    {
        public static void RegisterService(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterTransactionService(services);
            RegisterRawTransactionService(services);
            RegisterMonitorService(services);
            RegisterSummaryTransactionService(services);
            RegisterRawUserAgentService(services);
            RegisterFPDevicesService(services);
            RegisterFPTransformTransaction(services);
            RegisterDetectBots(services);
            RegisterArchiveService(services);
            RegisterOffset(services);

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

        private static void RegisterSummaryTransactionService(this IServiceCollection services)
        {
            services.AddSingleton<ISummaryJobServices, SummaryJobServices>();
            services.AddSingleton<ISummaryTransactionDataAccess, SummaryTransactionDataAccess>();
            services.AddSingleton<ISummaryTransactionService, SummaryTransactionService>();
        }

        private static void RegisterRawUserAgentService(this IServiceCollection services)
        {
            services.AddSingleton<IRawUserAgentJobServices, RawUserAgentJobServices>();
            services.AddSingleton<IRawUserAgentDataAccess, RawUserAgentDataAccess>();
            services.AddSingleton<IRawUserAgentService, RawUserAgentService>();
        }

        private static void RegisterFPDevicesService(this IServiceCollection services)
        {
            services.AddSingleton<IFPDeviceJobService, FPDeviceJobService>();
            services.AddSingleton<IFPDeviceDataAccess, FPDeviceDataAccess>();
            services.AddSingleton<IFPDeviceServices, FPDeviceServices>();
        }

        private static void RegisterFPTransformTransaction(this IServiceCollection services)
        {
            services.AddSingleton<IFPTransactionDataAccess, FPTransactionDataAccess>();
            services.AddSingleton<IFPTransactionServices, FPTransactionServices>();
            services.AddSingleton<IFPTransactionJobService, FPTransactionJobService>();
        }

        private static void RegisterArchiveService(this IServiceCollection services)
        {
            services.AddSingleton<IArchiveDataAccess, ArchiveDataAccess>();
            services.AddSingleton<IArchiveService, ArchiveService>();
            services.AddSingleton<IArchiveJobServices, ArchiveJobServices>();
        }

        private static void RegisterDetectBots(this IServiceCollection services)
        {
            services.AddSingleton<IBrowserInfoDataAccess, BrowserInfoDataAccess>();
            services.AddSingleton<IBrowserInfoService, BrowserInfoService>();
            services.AddSingleton<IBrowserInfoJobServices, BrowserInfoJobServices>();
        }

        private static void RegisterOffset(this IServiceCollection services)
        {
            services.AddSingleton<IOffsetDataAccess, OffsetDataAccess>();
            services.AddSingleton<IOffsetService, OffsetService>();
        }

    }
}
