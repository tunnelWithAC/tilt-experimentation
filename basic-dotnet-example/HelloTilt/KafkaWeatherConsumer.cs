using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class KafkaWeatherConsumer : BackgroundService
{
    private readonly ILogger<KafkaWeatherConsumer> _logger;
    private readonly string _broker;
    private readonly string _topic;
    private readonly string _groupId;

    public KafkaWeatherConsumer(ILogger<KafkaWeatherConsumer> logger)
    {
        _logger = logger;
        _broker = Environment.GetEnvironmentVariable("KAFKA_BROKER") ?? "localhost:9092";
        _topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC") ?? "weather_updates";
        _groupId = Environment.GetEnvironmentVariable("KAFKA_GROUP_ID") ?? "weather-consumer-group";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _broker,
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_topic);

        _logger.LogInformation($"Kafka consumer started. Listening to topic: {_topic}");

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