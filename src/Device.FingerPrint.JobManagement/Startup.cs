using Fanex.Logging;
using Fanex.Logging.Sentry;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Sentry;
using SilkierQuartz;
using System.Collections.Generic;

namespace Device.FingerPrint.JobManagement
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = new AppSettings(this.Configuration);

            services.AddRazorPages();

            services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.IgnoreDuplicates = true; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            });

            services.AddSilkierQuartz(options =>
            {
                options.VirtualPathRoot = "/jobs";
                options.UseLocalTime = true;
                options.DefaultDateFormat = "yyyy-MM-dd";
                options.DefaultTimeFormat = "HH:mm:ss";
                options.CronExpressionOptions = new CronExpressionDescriptor.Options()
                {
                    DayOfWeekStartIndexZero = false //Quartz uses 1-7 as the range
                };
                options.ProductName = "Device FingerPrint";               
            },
            authenticationOptions =>
            {
                authenticationOptions.AuthScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                authenticationOptions.SilkierQuartzClaim = "Silkier";
                authenticationOptions.SilkierQuartzClaimValue = "Quartz";
                authenticationOptions.UserName = "admin";
                authenticationOptions.UserPassword = "admin";
                authenticationOptions.AccessRequirement = SilkierQuartzAuthenticationOptions.SimpleAccessRequirement.AllowOnlyUsersWithClaim;
            });

            RegisterLogging(appSettings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSilkierQuartz();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", context =>
                {
                    context.Response.Redirect("/jobs");
                    return System.Threading.Tasks.Task.CompletedTask;
                });
            });
        }

        private void RegisterLogging(IAppSettings appSettings)
        {
            LogManager
                .SetDefaultLogCategory("DeviceFingerPrintJobManagement")
                .Use(new List<ILogEngine>
                {
                     new SentryLogging(new SentryEngineOptions {
                        Dsn = new Dsn(appSettings.SentryDsn),
                        Environment = appSettings.Environment
                     })
                });
        }
    }
}
