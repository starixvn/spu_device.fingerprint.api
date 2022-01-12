using Hangfire;
using Hangfire.MySql;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Transactions;

namespace Device.FingerPrint.JobManagement
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire((provider, configuration) => configuration
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseSimpleAssemblyNameTypeSerializer());

            services.AddHostedService<RecurringJobsService>();
            services.AddHangfireServer(options =>
            {
                options.StopTimeout = TimeSpan.FromSeconds(15);
                options.ShutdownTimeout = TimeSpan.FromSeconds(30);
            });

            GlobalConfiguration.Configuration.UseStorage(
                new MySqlStorage(
                "server=10.18.200.72;Port=3306;uid=fpsAPIAdmin;pwd=Welcome1;database=FPS_FingerPrint_Jobs;Allow User Variables=True",
                new MySqlStorageOptions
                {
                    TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),
                    PrepareSchemaIfNecessary = true,
                    DashboardJobListLimit = 50000,
                    TransactionTimeout = TimeSpan.FromMinutes(1),
                    TablesPrefix = "fp_"
                }));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                Authorization = new[] { new HangfireCustomBasicAuthenticationFilter { User = "admin", Pass = "admin" } }
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", context => {
                    context.Response.Redirect("/jobs");
                    return System.Threading.Tasks.Task.CompletedTask;
                });
            });
        }

        //private void RegisterLogging(IAppSettings appSettings)
        //{
        //    LogManager
        //        .SetDefaultLogCategory("DeviceFingerPrintWebAPI")
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
