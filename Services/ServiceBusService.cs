using Azure.Messaging.ServiceBus;
using CreditoAPI.DTOs;
using System.Text.Json;

namespace CreditoAPI.Services
{
    public class ServiceBusService : IServiceBusService, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ServiceBusReceiver _receiver;
        private readonly ServiceBusSender _auditSender;
        private readonly ILogger<ServiceBusService> _logger;
        private readonly string _topicName;
        private readonly string _auditTopicName;

        public ServiceBusService(IConfiguration configuration, ILogger<ServiceBusService> logger)
        {
            _logger = logger;
            var connectionString = configuration["ServiceBus:ConnectionString"];
            _topicName = configuration["ServiceBus:TopicName"] ?? "integrar-credito-constituido-entry";
            _auditTopicName = configuration["ServiceBus:AuditTopicName"] ?? "consulta-credito-log";
            var subscriptionName = configuration["ServiceBus:SubscriptionName"] ?? "credito-processor";

            if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("your-namespace"))
            {
                _logger.LogWarning("Service Bus connection string not configured. Using mock implementation.");
                _client = null!;
                _sender = null!;
                _receiver = null!;
            }
            else
            {
                _client = new ServiceBusClient(connectionString);
                _sender = _client.CreateSender(_topicName);
                _auditSender = _client.CreateSender(_auditTopicName);
                _receiver = _client.CreateReceiver(_topicName, subscriptionName, new ServiceBusReceiverOptions
                {
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                });
            }
        }

        public async Task SendMessageAsync(CreditoDto credito)
        {
            try
            {
                if (_sender == null)
                {
                    _logger.LogWarning("Service Bus sender not initialized. Message not sent: {NumeroCredito}", credito.NumeroCredito);
                    return;
                }

                var messageBody = JsonSerializer.Serialize(credito);
                var message = new ServiceBusMessage(messageBody)
                {
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString()
                };

                await _sender.SendMessageAsync(message);
                _logger.LogInformation("Message sent to topic {TopicName}: {NumeroCredito}", _topicName, credito.NumeroCredito);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to Service Bus for credito: {NumeroCredito}", credito.NumeroCredito);
                throw;
            }
        }

        public async Task<List<CreditoDto>> ReceiveMessagesAsync()
        {
            var creditos = new List<CreditoDto>();

            try
            {
                if (_receiver == null)
                {
                    _logger.LogWarning("Service Bus receiver not initialized. No messages received.");
                    return creditos;
                }

                var messages = await _receiver.ReceiveMessagesAsync(maxMessages: 10, maxWaitTime: TimeSpan.FromMilliseconds(100));

                foreach (var message in messages)
                {
                    try
                    {
                        var body = message.Body.ToString();
                        var credito = JsonSerializer.Deserialize<CreditoDto>(body);

                        if (credito != null)
                        {
                            creditos.Add(credito);
                            await _receiver.CompleteMessageAsync(message);
                            _logger.LogInformation("Message received and completed: {NumeroCredito}", credito.NumeroCredito);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message: {MessageId}", message.MessageId);
                        await _receiver.AbandonMessageAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages from Service Bus");
            }

            return creditos;
        }

        public async Task SendAuditMessageAsync(string eventType, string key)
        {
            try
            {
                if (_auditSender == null)
                {
                    _logger.LogWarning("Service Bus audit sender not initialized. Audit message not sent: {EventType} - {Key}", eventType, key);
                    return;
                }

                var payload = JsonSerializer.Serialize(new
                {
                    eventType,
                    key,
                    timestamp = DateTime.UtcNow
                });

                var message = new ServiceBusMessage(payload)
                {
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString()
                };

                await _auditSender.SendMessageAsync(message);
                _logger.LogInformation("Audit message sent to topic {TopicName}: {EventType} - {Key}", _auditTopicName, eventType, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending audit message to Service Bus: {EventType} - {Key}", eventType, key);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_sender != null)
                await _sender.DisposeAsync();
            
            if (_auditSender != null)
                await _auditSender.DisposeAsync();
            
            if (_receiver != null)
                await _receiver.DisposeAsync();
            
            if (_client != null)
                await _client.DisposeAsync();
        }
    }
}
