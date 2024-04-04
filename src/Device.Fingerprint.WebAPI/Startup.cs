namespace Device.Fingerprint.WebAPI
{
    using Device.Fingerprint.WebAPI.Middleware;
    using Device.Fingerprint.WebAPI.Services;
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Core.Services;
    using Fanex.Data;
    using Fanex.Data.MySql;
    using Fanex.Logging;
    using Fanex.Logging.Extensions;
    using Fanex.Logging.Extensions.AspNetCore;
    using Fanex.Logging.Extensions.Database;
    using Fanex.Logging.Sentry;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;
    using Prometheus;
    using Sentry;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = new AppSettings(this.Configuration);
            var JWTService = new JWTService(appSettings);

            services.AddSingleton<IAppSettings>(appSettings);
            services.AddSingleton<ILogger, Logger>();

            services.AddCors(opt =>
            {
                opt.AddDefaultPolicy(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            services.AddResponseCaching();

            services.AddControllers(options =>
            {
                var jsonInputFormatter = options.InputFormatters
                    .OfType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonInputFormatter>()
                    .Single();
                jsonInputFormatter.SupportedMediaTypes.Add("application/json");
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddHttpContextAccessor();

            services.AddHealthChecks();

            #region Swagger Configuration
            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "Device.Fingerprint.WebAPI", Version = "v1" });               

            });
            #endregion


            #region Authentication
            //services.AddAuthentication(option =>
            //{
            //    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            //}).AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuer = false,
            //        ValidateAudience = false,
            //        ValidateLifetime = false,
            //        ValidateIssuerSigningKey = false,
            //        IssuerSigningKey = new SymmetricSecurityKey(JWTService.keyJWT)
            //    };
            //});
            #endregion

            services.RegisterService();
            FanexDataConfig(appSettings);

            LogManager
           .SetDefaultLogCategory("Device.Fingerprint.WebAPI")
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

            LogService.Info("WebAPI Start!");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }

            // Custom Metrics to count requests for each endpoint and the method
            var counter = Metrics.CreateCounter("fingerprintapi_path_counter", "Counts requests to the Transaction API endpoints", new CounterConfiguration
            {
                LabelNames = new[] { "method", "endpoint" }
            });
            app.Use((context, next) =>
            {
                counter.WithLabels(context.Request.Method, context.Request.Path).Inc();
                return next();
            });
            // Use the Prometheus middleware
            app.UseMetricServer();
            app.UseHttpMetrics();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Device.Fingerprint.WebAPI v1");                
            });

            app.UseStaticFiles();

            app.UseHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true
            });

            app.UseForwardedHeaders();

            app.UseRouting();

            app.UseCors();

            app.UseResponseCaching();

            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromMinutes(60)
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new string[] { "Accept-Encoding", "version" };

                await next();
            });

            app.UseAuthorization();

            app.UseMiddleware<ExceptionMiddleware>();

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
    }
}
