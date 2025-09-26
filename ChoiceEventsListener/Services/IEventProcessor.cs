
using ChoiceEventsListener.Models;


namespace ChoiceEventsListener.Services
{
    public interface IEventProcessor
    {
        Task<ProcessingResult> ProcessAsync(string rawJson);
    }
}
