using Azure.Messaging.ServiceBus;
using ChoiceEventsListener.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<ServiceBusClient>(sp =>
{
    var cs = builder.Configuration["ServiceBusConnectionString"];

    var options = new ServiceBusClientOptions
    {
        RetryOptions = new ServiceBusRetryOptions
        {
            Mode = ServiceBusRetryMode.Exponential,
            MaxRetries = 5,
            Delay = TimeSpan.FromSeconds(0.5),
            MaxDelay = TimeSpan.FromSeconds(5),
            TryTimeout = TimeSpan.FromSeconds(30)
        }
    };

    return new ServiceBusClient(cs, options);
});


// Регистрируем сервис-паблишер
builder.Services.AddSingleton<ServiceBusPublisher>();
builder.Services.AddSingleton<IEventProcessor, EventProcessor>();

builder.Build().Run();
