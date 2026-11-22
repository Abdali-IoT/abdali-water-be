using Microsoft.AspNetCore.Diagnostics;
using Npgsql;

namespace API.Authentication;

internal sealed class AuthenticationDependencyExceptionHandler(
    ILogger<AuthenticationDependencyExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not (HttpRequestException or TaskCanceledException or TimeoutException or NpgsqlException { IsTransient: true }))
        {
            return false;
        }

        logger.LogWarning("An API dependency was unavailable ({ExceptionType}).", exception.GetType().Name);
        context.Response.Headers.CacheControl = "no-store";
        await Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "A required service is temporarily unavailable.").ExecuteAsync(context);
        return true;
    }
}