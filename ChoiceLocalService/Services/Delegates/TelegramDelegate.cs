using ChoiceModels.Order;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChoiceLocalService.Services.Delegates;

public class TelegramDelegate : IMessageDelegate
{
    private readonly HttpClient _httpClient;
    private readonly string _botToken;
    private readonly string _chatId;
    private readonly ILogger<TelegramDelegate> _logger;

    public TelegramDelegate(HttpClient httpClient, IConfiguration config, ILogger<TelegramDelegate> logger)
    {
        _httpClient = new HttpClient();
        _logger = logger;
        _botToken = config["Telegram:BotToken"]!;
        _chatId = config["Telegram:ChatId"]!;
    }

    public async Task<bool> HandleAsync(string messageBody)
    {
        try
        {
            var order = JsonSerializer.Deserialize<OrderDto>(messageBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (order == null)
            {
                _logger.LogWarning("[TelegramDelegate] Failed to deserialize message.");
                return false;
            }

            var text = $"🆕 Нове замовлення #{order.Id}\n" +
                       $"📍 Адреса: {order.Delivery?.Customer?.Address?.Prediction ?? "N/A"}\n" +
                       $"📞 Телефон: {order.Delivery?.Customer?.Phone ?? "N/A"}\n" +
                       $"💰 Сума: {order.Total / 100.0:F2} {order.Currency}";

            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new { chat_id = _chatId, text };
            var body = JsonSerializer.Serialize(payload);

            var response = await _httpClient.PostAsJsonAsync(url, payload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[TelegramDelegate] Sent order {OrderId} to Telegram.", order.Id);
                return true;
            }

            _logger.LogWarning("[TelegramDelegate] Failed to send order {OrderId}. StatusCode: {StatusCode}", order.Id, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TelegramDelegate] Error sending message.");
            return false;
        }
    }
}

