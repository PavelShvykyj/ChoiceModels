using Azure.Messaging.ServiceBus;
using ChoiceEventsListener.Models;
using ChoiceModels.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;


namespace ChoiceEventsListener.Services
{
    public class EventProcessor : IEventProcessor
    {
        private readonly ServiceBusPublisher _publisher;
        private readonly ILogger<EventProcessor> _logger;

        public EventProcessor(ServiceBusPublisher publisher, ILogger<EventProcessor> logger)
        {
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<ProcessingResult> ProcessAsync(string rawJson)
        {
            EventEnvelope? evt;
            try
            {
                evt = JsonSerializer.Deserialize<EventEnvelope>(rawJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON");
                return new ProcessingResult(400, "Invalid JSON format.");
            }

            if (evt is null)
                return new ProcessingResult(400, "Invalid event structure.");

            if (!evt.IsOrderCreated)
                return new ProcessingResult(200, "Event ignored (not order.created).");

            if (string.IsNullOrWhiteSpace(evt.VarSymbol))
                return new ProcessingResult(400, "Missing or empty varSymbol (queue name).");

            var message = new ServiceBusMessage(evt.Data.GetRawText())
            {
                MessageId = evt.Id,
                ContentType = "application/json"
            };
            message.ApplicationProperties["eventType"] = evt.Type;

            var success = await _publisher.TrySendAsync(evt.VarSymbol, message);
            if (!success)
                return new ProcessingResult(502, $"Failed to send to queue '{evt.VarSymbol}'.");

            return new ProcessingResult(200, $"Order message sent to '{evt.VarSymbol}'.");
        }
    }

}





