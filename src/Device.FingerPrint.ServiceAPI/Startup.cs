using Device.FingerPrint.ServiceAPI.Services;
using Fanex.Data;
using Fanex.Data.MySql;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Sentry;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Sentry;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Device.FingerPrint.ServiceAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = new AppSettings(this.Configuration);
            services.AddSingleton<IAppSettings>(appSettings);
            services.AddSingleton<ILogger, Logger>();

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Device.FingerPrint.ServiceAPI", Version = "v1" });
            });

            services.AddHealthChecks();

            services.RegisterService(Configuration);


            FanexDataConfig(appSettings);
            // RegisterLogging(appSettings);

            LogManager
           .SetDefaultLogCategory("Device.FingerPrint.ServiceAPI")
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

                busConfigurator.UsingRabbitMq((context, cfx) =>
                {
                    cfx.ConfigureEndpoints(context);
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            app.UsePathBase("/jobapi");
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "Device.FingerPrint.ServiceAPI v1"));

            app.UseHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
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

        //private void RegisterLogging(IAppSettings appSettings)
        //{
        //    LogManager
        //        .SetDefaultLogCategory("DeviceFingerPrintServiceAPI")
        //        .Use(new List<ILogEngine>
        //        {
        //             new SentryLogging(new SentryEngineOptions {
        //                Dsn = new Dsn(appSettings.SentryDsn),
        //                Environment = appSettings.Environment
        //             })
        //        });
        //}
    }
}
