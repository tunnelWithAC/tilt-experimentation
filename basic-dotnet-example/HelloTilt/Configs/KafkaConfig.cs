namespace HelloTilt.Configs;

public class KafkaConfig
{
    public string Broker { get; set; } = "localhost:9092";
    public string Topic { get; set; } = "weather_updates";
    public string GroupId { get; set; } = "weather-consumer-group";
    public string AutoOffsetReset { get; set; } = "Earliest";
} 