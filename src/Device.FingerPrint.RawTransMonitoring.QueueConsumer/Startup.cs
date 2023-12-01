namespace Device.FingerPrint.RawTransMonitoring.QueueConsumer
{
    using Fanex.Data;
    using Fanex.Logging.Extensions.AspNetCore;
    using Fanex.Logging.Extensions.Database;
    using Fanex.Logging.Extensions;
    using Fanex.Logging.Sentry;
    using Fanex.Logging;
    using MassTransit;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Sentry;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Hosting;
    using Fanex.Data.MySql;
    using Device.FingerPrint.RawTransMonitoring.QueueConsumer.Handlers;
    using Device.FingerPrint.RawTransMonitoring.QueueConsumer.Services;
    using Device.FingerPrint.Features.MonitorTransaction.Services;

    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = new AppSettings(this.Configuration);

            services.AddSingleton<IAppSettings>(appSettings);
            services.AddSingleton<ILogger, Logger>();

            services.AddHealthChecks();

            FanexDataConfig(appSettings);

            LogManager
               .SetDefaultLogCategory("Device.FingerPrint.RawTransMonitoring.QueueConsumer")
               .Use(new List<ILogEngine>
               {
                        new SentryLogging(new SentryEngineOptions
                        {
                            Dsn = new Dsn(appSettings.SentryDsn)
                        })
               });

            ExceptionHandler
                .UseConfig(new LoggingConfiguration())
                .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo);

            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

                busConfigurator.AddConsumer<Consumer>(cfg =>
                {
                    cfg.Options<BatchOptions>(options => options
                    .SetMessageLimit(appSettings.GetValue<int>("Consumer:Masstransit:BatchSizeLimit"))
                    .SetTimeLimit(s: appSettings.GetValue<int>("Consumer:Masstransit:BatchTimeLimit"))
                    .SetConcurrencyLimit(appSettings.GetValue<int>("Consumer:Masstransit:BatchConcurrencyLimit")));
                }).Endpoint(e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Name = appSettings.GetValue<string>("Consumer:Name");
                    e.PrefetchCount = appSettings.GetValue<int>("Consumer:Masstransit:PrefetchCount");
                });


                busConfigurator.UsingRabbitMq((context, rgx) =>
                {
                    rgx.ConfigureEndpoints(context);
                });
            });

            services.AddOptions<RabbitMqTransportOptions>()
            .Configure(options =>
            {
                options.Host = appSettings.GetValue<string>("RabbitMQ:Host");
                options.Port = appSettings.GetValue<ushort>("RabbitMQ:Port");
                options.User = appSettings.GetValue<string>("RabbitMQ:UserName");
                options.Pass = appSettings.GetValue<string>("RabbitMQ:Password");
                options.VHost = appSettings.GetValue<string>("RabbitMQ:VirtualHost");
            });

            services.AddSingleton<ITransactionMonitoringService, TransactionMonitoringService>();
            services.AddSingleton<ITransactionMonitoringDataAccess, TransactionMonitoringDataAccess>();
        }

        private void FanexDataConfig(IAppSettings appSettings)
        {
            var connections = Configuration
                                  .GetSection("AppConfig:Connections")
                                  .GetChildren()
                                  .AsEnumerable()
                                  .ToDictionary(
                                           x => x.GetValue<string>("Name"),
                                           y => new ConnectionConfiguration(y.GetValue<string>("Name"), y.GetValue<string>("ConnectionString"), y.GetValue<string>("ProviderName"))
                                   );

            DbSettingProviderManager
              .StartNewSession()
              .Use(connections)
              .WithMySql()
              .UseDefaultDbSettingProvider(resourcePath: appSettings.SPName)
              .Run();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true
            });

            app.UseRouting();
        }
    }
}
