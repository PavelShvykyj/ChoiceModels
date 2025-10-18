using Azure.Messaging.ServiceBus;
using ChoiceLocalService.Services;
using ChoiceLocalService.Services.Delegates;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build())
    .Enrich.FromLogContext()
    .CreateLogger();

Log.Information("Starting ChoiceLocalService...");

var builder = Host.CreateApplicationBuilder(args);



builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "ChoiceLocalService";
});

builder.Logging.ClearProviders();


builder.Logging.AddSerilog(Log.Logger);


// Service Bus client
builder.Services.AddSingleton(sp =>
{
    var cs = builder.Configuration["ServiceBus:ConnectionString"];
    return new ServiceBusClient(cs);
});



builder.Services.AddHttpClient<TelegramDelegate>(); 
builder.Services.AddHttpClient<HttpApiDelegate>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseCookies = false 
    });


builder.Services.AddSingleton<IMessageDelegate>(sp => sp.GetRequiredService<HttpApiDelegate>());
builder.Services.AddSingleton<QueueConsumer>();

// Core services
builder.Services.AddSingleton<RuntimeSupervisor>();
builder.Services.AddSingleton<TelegramBotService>();


// Hosted services
builder.Services.AddHostedService(sp => sp.GetRequiredService<TelegramBotService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<QueueConsumer>());


Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var host = builder.Build();
host.Run();
