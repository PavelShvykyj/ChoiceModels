using Azure.Messaging.ServiceBus;
using ChoiceLocalService.Services.Delegates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceLocalService.Services
{
    public class RuntimeSupervisor
    {

        private readonly HttpApiDelegate _apiManager;
        private readonly QueueConsumer _queue;
        private readonly ILogger<RuntimeSupervisor> _logger;

        public RuntimeSupervisor(
            IEnumerable<IMessageDelegate> delegates,
            HttpApiDelegate apiManager,
            QueueConsumer queue,
            ILogger<RuntimeSupervisor> logger
            ) { 
        
            _apiManager = apiManager;
            _queue = queue;
            _logger = logger;

            foreach (var item in delegates)
            {
                _queue.OnProcess += item.HandleAsync;

            }

            _queue.OnFailure += OnQueueFailure;
            _apiManager.SessionStateChanged += OnSessionStateChanged;
        }



        public bool IsQueueRunning { get => _queue.IsRunning;  }
        public bool IsApiEnabled { get => _apiManager.IsEnabled; }


        public async Task<bool> StopProcessQueueAsync() {
            return await _queue.StopListeningAsync();
        }


        public async Task<bool>  StartProcessQueueAsync() {
           return await _queue.StartListeningAsync();
        
        }

        public async Task<bool> StopAPI() {
           return await _apiManager.DisableAsync();
        }

        public async Task<bool> StartAPI()
        {
           return await _apiManager.EnableAsync();
        }



        private async Task OnQueueFailure(string arg)
        {
            _logger.LogError($"Queue crushed with error: {arg}");

        }

        private async Task OnSessionStateChanged( bool state)
        {
            if (state & !_queue.IsRunning)
            {
                await _queue.StartListeningAsync();
            }
            else if (state & _queue.IsRunning)
            {
                return;
            }
            else
            {
                await _queue.StopListeningAsync();
            }
        }

    }
}
