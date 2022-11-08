using Device.FingerPrint.DCS.QueueConsumer.Handlers;
using Device.FingerPrint.DCS.QueueConsumer.Services;
using Fanex.Data;
using Fanex.Data.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace Device.FingerPrint.DCS.QueueConsumer
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

            services.AddSingleton<ICapturingService, CapturingService>();
            services.AddSingleton<IDeviceDetectorServices, DeviceDetectorServices>();

            services.AddHealthChecks();

            FanexDataConfig(appSettings);

            services.AddHostedService<Consumer>();
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
        }
    }
}
