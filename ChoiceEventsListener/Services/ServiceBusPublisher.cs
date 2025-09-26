using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace ChoiceEventsListener.Services;

public class ServiceBusPublisher
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusPublisher> _logger;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

    public ServiceBusPublisher(ServiceBusClient client, ILogger<ServiceBusPublisher> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> TrySendAsync(string queueName, ServiceBusMessage message)
    {
        try
        {
            var sender = _senders.GetOrAdd(queueName, name => _client.CreateSender(name));
            await sender.SendMessageAsync(message);
            _logger.LogInformation("Message sent to queue {Queue}", queueName);
            return true;
        }
        catch (ServiceBusException sbEx) when (!sbEx.IsTransient)
        {
            // Non-transient error → сразу выходим
            _logger.LogError(sbEx, "Non-transient ServiceBus error for {Queue}: {Reason}", queueName, sbEx.Reason);

            // Можно дополнительно различать типы ошибок
            // Например, EntityNotFound → очередь не существует
            if (sbEx.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
            {
                _logger.LogCritical("Queue {Queue} does not exist!", queueName);
            }

            return false;
        }
        catch (ServiceBusException sbEx)
        {
            // Transient error → SDK уже сделал retry, здесь просто финальный фэйл
            _logger.LogError(sbEx, "Transient ServiceBus error after retries for {Queue}", queueName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when sending to {Queue}", queueName);
            return false;
        }
    }
}

