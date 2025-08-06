using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HelloTilt;

public class KafkaWeatherConsumer : BackgroundService
{
    private readonly ILogger<KafkaWeatherConsumer> _logger;
    private readonly KafkaConfig _kafkaConfig;

    public KafkaWeatherConsumer(ILogger<KafkaWeatherConsumer> logger, KafkaConfig kafkaConfig)
    {
        _logger = logger;
        _kafkaConfig = kafkaConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaConfig.Broker,
            GroupId = _kafkaConfig.GroupId,
            AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_kafkaConfig.AutoOffsetReset)
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_kafkaConfig.Topic);

        _logger.LogInformation($"Kafka consumer started. Listening to topic: {_kafkaConfig.Topic}");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(stoppingToken);
                    _logger.LogInformation($"Received message: {cr.Message.Value}");
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        finally
        {
            consumer.Close();
        }
    }
} 