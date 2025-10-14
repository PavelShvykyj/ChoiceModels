
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ChoiceLocalService.Services;

public class TelegramBotService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly string _chatId;
    private readonly RuntimeSupervisor _supervisor;
    private readonly ILogger<TelegramBotService> _logger;

    public TelegramBotService(IConfiguration config, RuntimeSupervisor supervisor, ILogger<TelegramBotService> logger)
    {
        
        _chatId = config["Telegram:BotChatId"]!;
        _bot = new TelegramBotClient(config["Telegram:BotToken"]!);
        _supervisor = supervisor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message },
            DropPendingUpdates = true // отбросить старые апдейты при запуске
        };

        _bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: stoppingToken
        );
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Message?.Text is null) return;
 
            switch (update.Message.Text.ToLower())
            {
                case "/startlistener":
                    var started = await _supervisor.StartProcessQueueAsync();
                    await bot.SendMessage(_chatId, started ? "✅ Listener started" : "❌ Failed to start", cancellationToken: token);
                    break;

                case "/stoplistener":
                    var stopped = await _supervisor.StopProcessQueueAsync();
                    await bot.SendMessage(_chatId, stopped ? "⏹️ Listener stopped" : "❌ Failed to stop", cancellationToken: token);
                    break;

                case "/status":
                    var status = _supervisor.IsQueueRunning ? "🟢 Running" : "🔴 Stopped";
                    await bot.SendMessage(_chatId, $"Listener status: {status}");
                    break;

                case "/statusapi":
                    var apiStatus = _supervisor.IsApiEnabled ? "🟢 API Enabled" : "🔴 API Disabled";
                    await bot.SendMessage(_chatId, $"API status: {apiStatus}");
                    break;

                case "/stopapi":
                    await _supervisor.StopAPI();
                    await bot.SendMessage(_chatId, "⏹️ API delegate disabled.");
                    break;

                case "/startapi":
                    await _supervisor.StartAPI();
                    if (_supervisor.IsApiEnabled)
                    {
                        await bot.SendMessage(_chatId, "✅ API delegate enabled.");
                    }
                    else
                    {
                        await bot.SendMessage(_chatId, "⏹️ Fail to start, API delegate disabled.");
                    }
                    break;
            }
    }

    private async Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {

        switch (exception)
        {
            case ApiRequestException apiEx:
                _logger.LogWarning($"Telegram API ошибка [{apiEx.ErrorCode}]: {apiEx.Message}");
                break;
            default:
                _logger.LogError(exception, "Неизвестная ошибка в Telegram Polling");
                break;
        }

        // 🔁 Попробуем подождать и переподключиться
        await Task.Delay(TimeSpan.FromSeconds(5), token);

        try
        {
            _logger.LogInformation("Пробуем восстановить соединение...");
            // Попробуем получить информацию о боте, чтобы убедиться, что связь восстановлена
            var me = await _bot.GetMe();
            _logger.LogInformation($"Соединение восстановлено. Бот: @{me.Username}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось восстановить соединение, повтор через 10 секунд...");
            await Task.Delay(TimeSpan.FromSeconds(10), token);
        }
    }

}

