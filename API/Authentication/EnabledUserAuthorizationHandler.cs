using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Service.Interface;

namespace API.Authentication;

internal sealed class EnabledUserRequirement : IAuthorizationRequirement;

internal sealed class EnabledUserAuthorizationHandler(IUserService users)
    : AuthorizationHandler<EnabledUserRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EnabledUserRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true
            || context.Resource is not HttpContext httpContext
            || !Guid.TryParseExact(context.User.FindFirstValue(JwtRegisteredClaimNames.Sub), "D", out var userId))
        {
            return;
        }

        var user = await users.FindByIdAsync(userId, httpContext.RequestAborted);
        if (user is { GoogleSignInEnabled: true })
        {
            httpContext.Features.Set(user);
            context.Succeed(requirement);
        }
    }
}