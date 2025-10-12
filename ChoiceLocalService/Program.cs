using Azure.Messaging.ServiceBus;
using ChoiceLocalService.Services;
using ChoiceLocalService.Services.Delegates;
using TelegramLogger;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddTelegramLogger(opt =>
{
    opt.BotToken = builder.Configuration["Telegram:BotToken"]; 
    opt.ChatId = builder.Configuration["Telegram:ChatId"]; 
    
});

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "ChoiceLocalService";
});

// Service Bus client
builder.Services.AddSingleton(sp =>
{
    var cs = builder.Configuration["ServiceBus:ConnectionString"];
    return new ServiceBusClient(cs);
});



builder.Services.AddHttpClient<TelegramDelegate>(); 
builder.Services.AddSingleton<HttpApiDelegate>();   
builder.Services.AddSingleton<IMessageDelegate>(sp => sp.GetRequiredService<TelegramDelegate>());
builder.Services.AddSingleton<IMessageDelegate>(sp => sp.GetRequiredService<HttpApiDelegate>());
builder.Services.AddSingleton<QueueConsumer>();

// Core services
builder.Services.AddSingleton<RuntimeSupervisor>();
builder.Services.AddSingleton<TelegramBotService>();


// Hosted services
builder.Services.AddHostedService(sp => sp.GetRequiredService<TelegramBotService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<QueueConsumer>());




var host = builder.Build();
host.Run();
