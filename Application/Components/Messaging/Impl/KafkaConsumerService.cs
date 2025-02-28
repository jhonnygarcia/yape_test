using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Application.Components.Messaging.Impl
{
    public class KafkaConsumerService<THandler, TMessage> : BackgroundService
        where THandler : class, IMessageHandler<TMessage>
        where TMessage : class, IMessage
    {
        private readonly string _bootstrapServers;
        private readonly string _topic;
        private readonly string _subscriberName;
        private readonly ILogger<KafkaConsumerService<THandler, TMessage>> _logger;
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumerService(IOptions<MessagingOptions> options,
            ILogger<KafkaConsumerService<THandler, TMessage>> logger, IServiceProvider serviceProvider)
        {
            _bootstrapServers = options.Value.BootstrapServers;
            _topic = typeof(TMessage).Name;
            _subscriberName = $"{typeof(THandler).FullName ?? typeof(THandler).Name}-{typeof(TMessage).Name}";
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        private async Task EnsureTopicExistsAsync()
        {
            var adminConfig = new AdminClientConfig { BootstrapServers = _bootstrapServers };

            using var adminClient = new AdminClientBuilder(adminConfig).Build();
            try
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
                if (metadata.Topics.Any(t => t.Topic == _topic))
                {
                    _logger.LogInformation($"Kafka topic '{_topic}' already exists.");
                    return;
                }

                var topicSpecification = new TopicSpecification
                {
                    Name = _topic,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                };

                await adminClient.CreateTopicsAsync(new[] { topicSpecification });
                _logger.LogInformation($"Kafka topic '{_topic}' created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error ensuring Kafka topic '{_topic}' exists.");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await EnsureTopicExistsAsync();
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = _subscriberName,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(_topic);

            _logger.LogInformation($"[{_subscriberName}] Listening to topic: {_topic}");
            await Task.Run(async () =>
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(stoppingToken);
                            var message = consumeResult.Message.Value;

                            _logger.LogInformation($"[{_subscriberName}] Message received: {message}");

                            await ProcessMessageAsync(message, stoppingToken);
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInformation("Consumer cancelled.");
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"[{_subscriberName}] Error processing message: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    consumer.Close();
                }
            }, stoppingToken);
        }

        private async Task ProcessMessageAsync(string message, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetService<THandler>();

            if (handler == null)
            {
                _logger.LogWarning($"[{_subscriberName}] No handler registered for message type: {typeof(TMessage).Name}");
                return;
            }

            var deserializedMessage = JsonSerializer.Deserialize<TMessage>(message);
            if (deserializedMessage == null)
            {
                _logger.LogError($"[{_subscriberName}] Failed to deserialize message");
                return;
            }

            await handler.HandleAsync(deserializedMessage, stoppingToken);
        }
    }
}
