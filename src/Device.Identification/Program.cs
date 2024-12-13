using Device.Identification;
using Device.Identification.ActivatorScript;
using Device.Identification.Optimization;
using Device.Identification.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseKestrel()
//    .ConfigureAppConfiguration((hostingContext, config) =>
//    {
//        var env = hostingContext.HostingEnvironment;

//        config.SetBasePath(env.ContentRootPath)
//              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//              .AddEnvironmentVariables();
//    });

var appSettings = new AppSettings(builder.Configuration);
builder.Services.AddSingleton<IAppSettings>(appSettings);
builder.Services.AddTransient<JwtToken>();

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<HttpCachingWrapper>();
builder.Services.AddSingleton<NumberGenerator>();
builder.Services.AddTransient<ScriptLoader>();
builder.Services.AddSingleton<BundleConfig>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();
builder.Environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "ScriptsDetecas");
builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(builder.Environment.WebRootPath));

var app = builder.Build();

//app.UseAshxMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var memoryCache = app.Services.GetRequiredService<IMemoryCache>();
var fileProvider = app.Services.GetRequiredService<IFileProvider>();
BundleTable.Initialize(memoryCache, fileProvider);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
