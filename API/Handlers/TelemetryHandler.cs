using API.MQTT;

namespace API.Handlers;

public sealed class TelemetryHandler(ILogger<TelemetryHandler> logger) : IMqttMessageHandler
{
    public string TopicFilter => "devices/telemetry";
    
    public Task HandleAsync(MqttMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Telemetry message received= {message}", message);
        return Task.CompletedTask;
    }
}