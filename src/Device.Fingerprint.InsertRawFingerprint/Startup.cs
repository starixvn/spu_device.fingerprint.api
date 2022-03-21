using Confluent.Kafka;
using Device.Fingerprint.InsertRawFingerprint.Handlers;
using Device.FingerPrint.Features.ClientTransaction.Services;
using Device.FingerPrint.Kafka;
using Device.FingerPrint.Kafka.Handlers.Kafka.Handlers;
using Device.FingerPrint.Model;
using Fanex.Data;
using Fanex.Data.MySql;
using Fanex.Logging;
using Fanex.Logging.Sentry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sentry;
using System.Collections.Generic;
using System.Linq;

namespace Device.Fingerprint.InsertRawFingerprint
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
            services.AddSingleton<IAppSettings>(appSettings);

            services.AddSingleton<IClientTransactionDataAccess, ClientTransactionDataAccess>();
            services.AddSingleton<IClientTransactionService, ClientTransactionService>();

            var clientConfig = new ClientConfig()
            {
                BootstrapServers = Configuration["Kafka:ClientConfigs:BootstrapServers"]
            };

            var consumerConfig = new ConsumerConfig(clientConfig)
            {
                GroupId = Configuration["Kafka:ClientConfigs:GroupId"],
                EnableAutoCommit = true,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000
            };
            services.AddSingleton(consumerConfig);

            services.AddScoped<IKafkaHandler<string, ClientTransactionModel>, KafkaHandler>();
            services.AddSingleton(typeof(IKafkaConsumer<,>), typeof(KafkaConsumer<,>));
            services.AddHostedService<KafkaConsumer>();

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
                .SetDefaultLogCategory("Device.Fingerprint.InsertRawFingerprint")
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

            app.UseRouting();

            app.UseHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
