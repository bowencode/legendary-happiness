using Microsoft.AspNetCore.Authorization;

namespace Demo.Tokens.Api.Host;

public class UserRequirement : IAuthorizationRequirement
{
    public string RouteParameter { get; }

    public UserRequirement(string routeParameter)
    {
        RouteParameter = routeParameter;
    }
}

public class UserAuthorizationHandler : AuthorizationHandler<UserRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRequirement requirement)
    {
        var routeData = (context.Resource as HttpContext)?.Request;
        var userId = routeData?.RouteValues[requirement.RouteParameter]?.ToString();

        var sub = context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        var userMatches = string.Equals(sub, userId);
        if (userMatches)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}