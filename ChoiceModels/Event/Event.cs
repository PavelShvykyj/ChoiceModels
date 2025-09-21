using ChoiceModels.Order;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChoiceModels.Events
{

    /// <summary>
    /// Базовое событие из шины сообщений.
    /// </summary>
    public sealed record EventEnvelope(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("langCode")] string LangCode,
        [property: JsonPropertyName("data")] JsonElement Data,
        [property: JsonPropertyName("varSymbol")] string VarSymbol,
        [property: JsonPropertyName("timestamp")] long Timestamp
    )
    {
        /// <summary>
        /// Проверяет, что событие относится к заказу и это именно "order.created".
        /// </summary>
        public bool IsOrderCreated => Type.Equals("order.created", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Десериализует поле data как OrderDto, если событие "order.created".
        /// Если тип не совпадает, вернёт null.
        /// </summary>
        public OrderDto? TryGetOrder(JsonSerializerOptions? options = null)
        {
            if (!IsOrderCreated) return null;

            return Data.Deserialize<OrderDto>(options ?? new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}

