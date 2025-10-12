using Azure.Messaging.ServiceBus;
using ChoiceLocalService.Services.Delegates;

namespace ChoiceLocalService.Services;

public class QueueConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<QueueConsumer> _logger;
    public event Func<string, Task>? OnFailure;
    public event Func<string, Task<bool>>? OnProcess;
    public event Func<string, Task>? MessageFailure;

    public bool IsRunning => _processor.IsProcessing;

    public QueueConsumer(ServiceBusClient client,
                         IConfiguration config,
                         ILogger<QueueConsumer> logger)
    {

        _logger = logger;

        var queueName = config["ServiceBus:QueueName"];
        _processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        await StartListeningAsync();
    }

    public async Task<bool> StartListeningAsync()
    {
        if (IsRunning) return true;

        try
        {
            await _processor.StartProcessingAsync();
            _logger.LogInformation("Queue listening started: {Status}", IsRunning);
            return IsRunning;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start listening");
            return false;
        }
    }

    public async Task<bool> StopListeningAsync()
    {
        if (!IsRunning) return true;

        try
        {
            await _processor.StopProcessingAsync();
            _logger.LogInformation("Queue listening stopped.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop listening");
            return false;
        }
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        _logger.LogInformation("Received message: {id}", args.Message.SessionId);
        if (OnProcess == null)
        {
            await args.AbandonMessageAsync(args.Message);
            return;
        }

        var delegates = OnProcess.GetInvocationList();
        bool handled = true;
        foreach (Func<string, Task<bool>> handler in delegates)
        {
            handled = handled & await handler(body);
        }

        if (handled)
        {
            await args.CompleteMessageAsync(args.Message);
        }
        else {
            await args.AbandonMessageAsync(args.Message);
            await MessageFailure?.Invoke(args.Message.CorrelationId);        
        }



    }

    private async Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Queue listener crashed");
        await StopListeningAsync();

        if (OnFailure != null)
        {
            await OnFailure.Invoke(args.Exception.Message ?? "Unknown error");
        }
    }

}

