using Azure.Messaging.ServiceBus;
using ChoiceEventsListener.Services;
using ChoiceModels.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;

namespace AzureFunctionPublisher.Functions;

public class PublishMessageFunction
{
    private readonly IEventProcessor _processor;

    public PublishMessageFunction(IEventProcessor processor)
    {
        _processor = processor;
    }

    [Function("PublishMessage")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        string body;
        using (var reader = new StreamReader(req.Body))
        {
            body = await reader.ReadToEndAsync();
        }

        var result = await _processor.ProcessAsync(body);

        var response = req.CreateResponse((System.Net.HttpStatusCode)result.StatusCode);
        await response.WriteStringAsync(result.Message);
        return response;
    }
}
