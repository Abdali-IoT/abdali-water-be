using API.MQTT;
using Common.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Serilog;

namespace API.Endpoints;

internal static class MqttBrokerEndpoints
{
    public static void MapMqttBrokerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/mqtt");
        auth.MapPost("/publish", PublishMessageToBroker).RequireAuthorization();
    }

    private static async Task<Results<Ok, BadRequest>> PublishMessageToBroker(
        PublishMessageRequest request,
        MqttPublisher publisher,
        HttpContext context
    )
    {
        var user = context.Features.Get<UserDto>() ?? throw new ArgumentException("User not found");
        Log.Information("Publish message received: {request}", request);
        var pubMsg = new MqttMessage(request.Topic, request.Payload);
        
        await publisher.PublishAsync(pubMsg);
        return TypedResults.Ok();
    }
}

internal record PublishMessageRequest(string Topic, string Payload);