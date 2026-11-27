using System.Text.Json;
using API.MQTT;
using Service.Interface;

namespace API.Handlers;

public sealed class TelemetryHandler(ILogger<TelemetryHandler> logger, ITelemetryService telemetryService) : IMqttMessageHandler
{
    public string TopicFilter => "devices/telemetry";

    public async Task HandleAsync(MqttMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Telemetry message received= {message}", message);
        await telemetryService.IngestTelemetryAsync(message.Payload, cancellationToken);
    }
}