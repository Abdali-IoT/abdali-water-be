using API.Authentication;
using Common.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Service.Authentication;
using Service.Interface;

namespace API.Endpoints;

internal static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/auth");
        auth.MapPost("/google", SignInWithGoogleAsync).AllowAnonymous();
        auth.MapGet("/me", GetCurrentUser).RequireAuthorization();
    }

    private static async Task<Results<Ok<AccessTokenResponse>, ProblemHttpResult>> SignInWithGoogleAsync(
        GoogleSignInRequest? request,
        GoogleCredentialVerifier google,
        IUserService users,
        ApiAccessTokens tokens,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (string.IsNullOrWhiteSpace(request?.IdToken))
            return TypedResults.Problem(statusCode: StatusCodes.Status400BadRequest, title: "A Google ID token is required.");

        var identity = await google.VerifyAsync(request.IdToken, cancellationToken);
        if (identity is null)
            return TypedResults.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid Google ID token.");
        
        var user = await users.ResolveGoogleUserAsync(identity.Subject, identity.FirstSignInEmail!, cancellationToken);
        if (user is not { GoogleSignInEnabled: true })
            return TypedResults.Problem(statusCode: StatusCodes.Status403Forbidden, title: "Google sign-in is unavailable for this account.");
        
        return TypedResults.Ok(tokens.Issue(user.Id));
    }

    private static Results<Ok<CurrentUserResponse>, UnauthorizedHttpResult> GetCurrentUser(HttpContext context)
    {
        var user = context.Features.Get<UserDto>()!;
        context.Response.Headers.CacheControl = "no-store";
        return TypedResults.Ok(new CurrentUserResponse(user.Id, user.Email, user.Devices));
    }

    private sealed record GoogleSignInRequest(string? IdToken);

    private sealed record CurrentUserResponse(Guid Id, string? Email, IReadOnlyList<DeviceDto> Devices);
}