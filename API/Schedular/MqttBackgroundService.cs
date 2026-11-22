
using API.MQTT;
using MQTTnet;

namespace API.Schedular;

public sealed class MqttBackgroundService(
    ILogger<MqttBackgroundService> logger,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    MqttMessageDispatcher dispatcher,
    IMqttClient client,
    MqttPublisher publisher
    )
    : BackgroundService
{

    private readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = Environment.GetEnvironmentVariable("MQTT__HOST") ?? throw new ArgumentException("Mqtt:Host is missing.");
        var port = int.Parse(Environment.GetEnvironmentVariable("MQTT__PORT") ?? "1883");

        var username = Environment.GetEnvironmentVariable("MQTT__USERNAME");
        var password = Environment.GetEnvironmentVariable("MQTT__PASSWORD");

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            throw new ArgumentException("MQTT__USERNAME or MQTT__PASSWORD is missing.");

        var clientId = Environment.GetEnvironmentVariable("MQTT__CLIENTID") ?? throw new ArgumentException("Mqtt:ClientId is missing.");

        var topics = GetSubscribedTopics();

        if (topics.Length == 0)
            throw new ArgumentException("No MQTT topics configured.");

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithClientId(clientId)
            .WithCredentials(username, password)
            .WithCleanStart()
            // .WithKeepAlivePeriod(TimeSpan.FromSeconds(20))
            .Build();

        // Handle incoming messages.
        client.ApplicationMessageReceivedAsync += async args =>
        {
            var topic = args.ApplicationMessage.Topic;
            var payload = args.ApplicationMessage.ConvertPayloadToString();
            // logger.LogInformation("Received [{topic}]: {payload}", topic, payload);

            try
            {
                var message = new MqttMessage(topic, payload);
                await dispatcher.DispatchAsync(message, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Application is shutting down.
            }
        };

        // Log disconnections. Reconnection is handled
        // separately by the background loop.
        client.DisconnectedAsync += args =>
        {
            logger.LogWarning("MQTT disconnected. Reason: {Reason}", args.Reason);
            return Task.CompletedTask;
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!client.IsConnected)
                {
                    await publisher.PauseAsync();
                    logger.LogInformation("Connecting to MQTT broker {Host}:{Port}", host, port);

                    // Avoid hanging indefinitely during connection.
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    timeout.CancelAfter(TimeSpan.FromSeconds(10));
                    var result = await client.ConnectAsync(options, timeout.Token);
                    if (!client.IsConnected)
                        throw new InvalidOperationException($"MQTT connection refused: {result.ResultCode}");

                    publisher.Resume();
                    logger.LogInformation("MQTT connected");
                    var builder = new MqttClientSubscribeOptionsBuilder();

                    foreach (var topic in topics)
                    {
                        builder.WithTopicFilter(filter => filter.WithTopic(topic).WithAtLeastOnceQoS());
                    }

                    var subscription = await client.SubscribeAsync(
                        builder.Build(), timeout.Token);

                    foreach (var item in subscription.Items)
                    {
                        if ((int)item.ResultCode >= 0x80)
                            throw new InvalidOperationException($"Subscription rejected: {item.TopicFilter.Topic}");

                        logger.LogInformation("Subscribed to {Topic}", item.TopicFilter.Topic);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "MQTT connection/subscription failed. Retrying in 5 seconds.");

                // Force a new connection and resubscription
                // if subscribing failed after connecting.
                await publisher.PauseAsync();
                if (client.IsConnected)
                {
                    try
                    {
                        await client.DisconnectAsync(cancellationToken: stoppingToken);
                    }
                    catch (Exception disconnectEx)
                    {
                        logger.LogWarning(disconnectEx, "Disconnect failed");
                    }
                }
            }

            try
            {
                await Task.Delay(_retryInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        // Graceful shutdown.
        await publisher.PauseAsync();
        if (client.IsConnected)
        {
            try
            {
                await client.DisconnectAsync(cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error during MQTT shutdown.");
            }
        }

        logger.LogInformation("MQTT background service stopped.");
    }

    private string[] GetSubscribedTopics()
    {
        using var scope = scopeFactory.CreateScope();

        return scope.ServiceProvider
            .GetServices<IMqttMessageHandler>()
            .Select(handler => handler.TopicFilter)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}
