
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChoiceLocalService.Services;

public class TelegramBotService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly string _chatId;
    private readonly RuntimeSupervisor _supervisor;

    public TelegramBotService(IConfiguration config, RuntimeSupervisor supervisor)
    {
        
        _chatId = config["Telegram:BotChatId"]!;
        _bot = new TelegramBotClient(config["Telegram:BotToken"]!);
        _supervisor = supervisor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
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
                else {
                    await bot.SendMessage(_chatId, "⏹️ Fail to start, API delegate disabled.");
                }                
                break;
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken token)
    {
        Console.WriteLine($"[TelegramBotService] Error: {ex.Message}");
        return Task.CompletedTask;
    }

    public async Task NotifyFailureAsync(string reason)
    {
        await _bot.SendMessage(_chatId,
            $"⚠️ Queue listener stopped due to error: {reason}\nUse /startlistener to restart.");
    }
}

