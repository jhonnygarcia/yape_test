using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Application.Components.Messaging.Impl
{
    public class KafkaMessagingService : IMessagingService
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaMessagingService> _logger;
        private readonly MessagingOptions _options;
        public KafkaMessagingService(IOptions<MessagingOptions> options, ILogger<KafkaMessagingService> logger)
        {
            _logger = logger;
            _options = options.Value;
            var config = new ProducerConfig { BootstrapServers = _options.BootstrapServers };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync<T>(T message) where T : class, IMessage
        {
            var topickName = typeof(T).Name;
            await EnsureTopicExistsAsync(topickName);

            var messageValue = JsonSerializer.Serialize(message);
            await _producer.ProduceAsync(topickName, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = messageValue
            });

            _logger.LogInformation($"Message publish in {topickName}: {messageValue}");
        }

        private async Task EnsureTopicExistsAsync(string topic)
        {
            var adminConfig = new AdminClientConfig { BootstrapServers = _options.BootstrapServers };

            using var adminClient = new AdminClientBuilder(adminConfig).Build();
            try
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
                if (metadata.Topics.Any(t => t.Topic == topic))
                {
                    _logger.LogInformation($"Kafka topic '{topic}' already exists.");
                    return;
                }

                var topicSpecification = new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                };

                await adminClient.CreateTopicsAsync(new[] { topicSpecification });
                _logger.LogInformation($"Kafka topic '{topic}' created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error ensuring Kafka topic '{topic}' exists.");
            }
        }
    }
}
