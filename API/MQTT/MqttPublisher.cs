using MQTTnet;
using MQTTnet.Protocol;

namespace API.MQTT;

public sealed class MqttPublisher(IMqttClient client)
{
    private readonly SemaphoreSlim _publishLock = new(1);
    private volatile bool _acceptingPublishes;

    public async Task PublishAsync(MqttMessage message, CancellationToken cancellationToken = default)
    {
        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(message.Topic)
            .WithPayload(message.Payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));

        await _publishLock.WaitAsync(timeout.Token);
        try
        {
            if (!_acceptingPublishes || !client.IsConnected)
                throw new InvalidOperationException("The MQTT broker connection is not ready.");

            var result = await client.PublishAsync(applicationMessage, timeout.Token);
            if (!result.IsSuccess)
                throw new InvalidOperationException($"MQTT publish to '{message.Topic}' failed: {result.ReasonCode}.");
        }
        finally
        {
            _publishLock.Release();
        }
    }

    internal async Task PauseAsync()
    {
        _acceptingPublishes = false;

        // Wait for any in-flight publish before the background service changes the connection.
        await _publishLock.WaitAsync();
        _publishLock.Release();
    }

    internal void Resume() => _acceptingPublishes = true;
}
