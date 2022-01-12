using Fanex.Data;
using Fanex.Data.MySql;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using Fps.Device.FingerPrint.Features.Transaction.Services;
using KafkaConsumerCore;
using KafkaConsumerTransaction.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sentry;
using System.Collections.Generic;
using System.Linq;

namespace KafkaConsumerTransaction
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = new AppSettings(this.Configuration);
            services.AddSingleton<ITransactionDataAccess, TransactionDataAccess>();
            services.AddSingleton<ITransactionService, TransactionService>();
            services.AddSingleton<IAppSettings, AppSettings>();
            services.AddSingleton<IHostedService, KafkaConsumerHandler>();

            services.AddHealthChecks();
            FanexDataConfig(appSettings);
            RegisterLogging(appSettings);
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

        private void RegisterLogging(IAppSettings appSettings)
        {
            LogManager
                .SetDefaultLogCategory("DeviceFingerPrintWebAPI")
                .Use(new List<ILogEngine>
                {
                     new SentryLogging(new SentryEngineOptions {
                        Dsn = new Dsn(appSettings.SentryDsn),
                        Environment = appSettings.Environment
                     })
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
