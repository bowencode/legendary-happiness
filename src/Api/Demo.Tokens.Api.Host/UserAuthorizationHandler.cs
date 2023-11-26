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
        // get route parameter for userId from http context
        var routeData = context.Resource as RouteData;
        var userId = routeData?.Values[requirement.RouteParameter]?.ToString();

        // get userId from claims
        var sub = context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        // compare userId from route parameter to userId from claims
        var userMatches = string.Equals(sub, userId);
        if (userMatches)
        {
            context.Succeed(requirement);
        }

        context.Fail(new AuthorizationFailureReason(this, "User is not authorized to access data for other users"));
        return Task.CompletedTask;
    }
}