using Flurl.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Spectre;
using Spectre.Console;
using TabDownloader.Service;

const string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
var logsPath = Path.Combine("logs");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Spectre(outputTemplate)
    .WriteTo.File($"{logsPath}/.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate, restrictedToMinimumLevel: LogEventLevel.Error)
    .CreateLogger();

if (!Directory.Exists("Tabs"))
{
    Directory.CreateDirectory("Tabs");
}

try
{
    var builder = Host.CreateApplicationBuilder();

    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));

    builder.Services.AddSerilog();
    builder.Services.AddSingleton<Style>(_ => new Style(Color.MediumOrchid3));
    builder.Services.AddSingleton<CookieJar>();
    builder.Services.AddSingleton<SongsterParser>();
    builder.Services.AddSingleton<MenuHandler>();
    builder.Services.AddHostedService<ConsoleMenu>();
    
    var app = builder.Build();
    
    await app.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "The application cannot be loaded");
}
finally
{
    await Log.CloseAndFlushAsync();
}