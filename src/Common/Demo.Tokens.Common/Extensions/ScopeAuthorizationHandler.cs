using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Demo.Tokens.Common.Extensions
{
    public class ScopeRequirement : IAuthorizationRequirement
    {
        public ScopeRequirement(string requiredScope)
        {
            RequiredScope = requiredScope;
        }

        public string RequiredScope { get; }
    }
    public class ScopeAuthorizationHandler : AuthorizationHandler<ScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
        {
            var hasScope = context.User.HasClaim(c => c.Type == "scope" && c.Value.Split(' ').Contains(requirement.RequiredScope));
            if (hasScope)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}