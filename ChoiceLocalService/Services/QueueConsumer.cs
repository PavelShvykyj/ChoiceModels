using Azure.Messaging.ServiceBus;

namespace ChoiceLocalService.Services;

public class QueueConsumer : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IConfiguration _config;
    private readonly ILogger<QueueConsumer> _logger;

    private ServiceBusProcessor? _processor;
    private CancellationTokenSource? _ctsQueueProcessing;

    public event Func<string, Task>? OnFailure;
    public event Func<string, Task<bool>>? OnProcess;

    public bool IsRunning => _processor?.IsProcessing == true;

    public QueueConsumer(ServiceBusClient client, IConfiguration config, ILogger<QueueConsumer> logger)
    {
        _client = client;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QueueConsumer background service started.");

        await StartListeningAsync();

        // блокируем завершение BackgroundService, пока не будет отменён stoppingToken
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("BackgroundService stopping...");
        }

        await StopListeningAsync();
    }

    public async Task<bool> StartListeningAsync()
    {
        if (IsRunning)
        {
            _logger.LogWarning("Queue listener is already running.");
            return true;
        }

        try
        {
            string queueName = _config["ServiceBus:QueueName"]
                ?? throw new InvalidOperationException("Queue name not configured.");

            _ctsQueueProcessing?.Dispose();
            _ctsQueueProcessing = new CancellationTokenSource();

            _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

            _processor.ProcessMessageAsync += ProcessMessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            await _processor.StartProcessingAsync(_ctsQueueProcessing.Token);
            _logger.LogInformation("Queue listening started for {queue}.", queueName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start listening queue.");
            return false;
        }
    }

    public async Task<bool> StopListeningAsync()
    {
        if (_processor == null)
        {
            _logger.LogWarning("Queue processor is not initialized or already stopped.");
            return false;
        }

        _logger.LogInformation("Stopping queue listener...");
        try
        {
            _ctsQueueProcessing?.Cancel();

            await _processor.StopProcessingAsync();
            await _processor.DisposeAsync();

            _processor = null;
            _logger.LogInformation("Queue listener stopped successfully.");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop queue listener.");
            return false;
        }
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        _logger.LogInformation("Received message: {id}", args.Message.MessageId);

        if (OnProcess == null)
        {
            await args.AbandonMessageAsync(args.Message);
            _logger.LogWarning("Abandon message {id}: no OnProcess handlers registered.", args.Message.MessageId);

            // безопасная остановка без дедлока
            _ = Task.Run(StopListeningAsync);
            return;
        }

        bool handled = true;
        foreach (Func<string, Task<bool>> handler in OnProcess.GetInvocationList())
        {
            handled = handled && await handler(body);
        }

        if (handled)
        {
            await args.CompleteMessageAsync(args.Message);
            _logger.LogInformation("Completed message {id}", args.Message.MessageId);
        }
        else
        {
            await args.AbandonMessageAsync(args.Message);
            _logger.LogWarning("Abandon message {id}", args.Message.MessageId);
            _ = Task.Run(StopListeningAsync); // асинхронная остановка
        }
    }

    private async Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Queue listener crashed.");

        // безопасно останавливаем из отдельной задачи
        _ = Task.Run(StopListeningAsync);

        if (OnFailure != null)
        {
            try
            {
                await OnFailure.Invoke(args.Exception.Message ?? "Unknown error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OnFailure callback.");
            }
        }
    }
}
