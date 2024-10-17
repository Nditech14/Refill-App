using Application.Messaging;
using Application.Services.Abstraction;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Application.Services.Implementation
{
    public class ServiceBusProducer : IServiceBusProducer, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _adminSender;
        private readonly ServiceBusSender _userSender;

        public ServiceBusProducer(IConfiguration configuration)
        {
            var connectionString = configuration["AzureServiceBus:ConnectionString"];
            var adminQueue = configuration["AzureServiceBus:AdminQueueName"];
            var userQueue = configuration["AzureServiceBus:UserQueueName"];

            _client = new ServiceBusClient(connectionString);
            _adminSender = _client.CreateSender(adminQueue);
            _userSender = _client.CreateSender(userQueue);
        }

        public async Task SendAdminNotificationAsync(AdminNotificationMessage message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(jsonMessage)
            {
                ContentType = "application/json"
            };
            await _adminSender.SendMessageAsync(serviceBusMessage);
        }

        public async Task SendUserNotificationAsync(UserNotificationMessage message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(jsonMessage)
            {
                ContentType = "application/json"
            };
            await _userSender.SendMessageAsync(serviceBusMessage);
        }

        public async ValueTask DisposeAsync()
        {
            await _adminSender.DisposeAsync();
            await _userSender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
