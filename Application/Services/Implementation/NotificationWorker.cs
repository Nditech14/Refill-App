// Application/Services/Implementation/NotificationWorker.cs
using Application.Messaging;
using Application.Services.Abstraction;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Services.Implementation
{
    public class NotificationWorker : BackgroundService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _adminProcessor;
        private readonly ServiceBusProcessor _userProcessor;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationWorker> _logger;
        private readonly IConfiguration _configuration;

        public NotificationWorker(IConfiguration configuration, IEmailService emailService, ILogger<NotificationWorker> logger)
        {
            _configuration = configuration;
            var connectionString = configuration["AzureServiceBus:ConnectionString"];
            var adminQueue = configuration["AzureServiceBus:AdminQueueName"];
            var userQueue = configuration["AzureServiceBus:UserQueueName"];

            _client = new ServiceBusClient(connectionString);
            _adminProcessor = _client.CreateProcessor(adminQueue, new ServiceBusProcessorOptions());
            _userProcessor = _client.CreateProcessor(userQueue, new ServiceBusProcessorOptions());

            _emailService = emailService;
            _logger = logger;

            // Subscribe to message handlers
            _adminProcessor.ProcessMessageAsync += ProcessAdminMessageAsync;
            _adminProcessor.ProcessErrorAsync += ProcessErrorAsync;

            _userProcessor.ProcessMessageAsync += ProcessUserMessageAsync;
            _userProcessor.ProcessErrorAsync += ProcessErrorAsync;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationWorker is starting.");

            // Start processing messages
            await _adminProcessor.StartProcessingAsync(stoppingToken);
            await _userProcessor.StartProcessingAsync(stoppingToken);

            _logger.LogInformation("NotificationWorker is running.");
        }

        private async Task ProcessAdminMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var body = args.Message.Body.ToString();
                var adminMessage = JsonSerializer.Deserialize<AdminNotificationMessage>(body);

                if (adminMessage != null)
                {
                    var emailSent = await _emailService.SendEmailAsync(
                        adminMessage.AdminEmail,
                        adminMessage.Subject,
                        adminMessage.Body,
                        adminMessage.Body // Assuming plain text is same as HTML content
                    );

                    if (emailSent)
                    {
                        _logger.LogInformation($"Admin notification sent to {adminMessage.AdminEmail}");
                        await args.CompleteMessageAsync(args.Message);
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to send admin notification to {adminMessage.AdminEmail}");
                        // Optionally, abandon the message to retry later
                        await args.AbandonMessageAsync(args.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("Received null AdminNotificationMessage.");
                    await args.CompleteMessageAsync(args.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing admin message: {ex.Message}");
                // Optionally, abandon the message to retry later
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private async Task ProcessUserMessageAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var body = args.Message.Body.ToString();
                var userMessage = JsonSerializer.Deserialize<UserNotificationMessage>(body);

                if (userMessage != null)
                {
                    var emailSent = await _emailService.SendEmailAsync(
                        userMessage.RecipientEmail,
                        userMessage.Subject,
                        userMessage.Body,
                        userMessage.Body // Assuming plain text is same as HTML content
                    );

                    if (emailSent)
                    {
                        _logger.LogInformation($"User notification sent to {userMessage.RecipientEmail}");
                        await args.CompleteMessageAsync(args.Message);
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to send user notification to {userMessage.RecipientEmail}");
                        // Optionally, abandon the message to retry later
                        await args.AbandonMessageAsync(args.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("Received null UserNotificationMessage.");
                    await args.CompleteMessageAsync(args.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing user message: {ex.Message}");
                // Optionally, abandon the message to retry later
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError($"Message handler encountered an exception {args.Exception}.");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("NotificationWorker is stopping.");

            // Stop processing messages
            await _adminProcessor.StopProcessingAsync(cancellationToken);
            await _userProcessor.StopProcessingAsync(cancellationToken);

            _logger.LogInformation("NotificationWorker has stopped processing messages.");

            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("NotificationWorker is disposing resources.");

            // Dispose of the processors and client synchronously
            _adminProcessor?.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _userProcessor?.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _client?.DisposeAsync().AsTask().GetAwaiter().GetResult();

            base.Dispose();

            _logger.LogInformation("NotificationWorker has disposed resources.");
        }
    }
}
