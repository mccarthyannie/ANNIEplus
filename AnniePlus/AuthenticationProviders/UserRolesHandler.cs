using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AnniePlus.AuthenticationProviders
{
    public class UserRolesHandler : AuthorizationHandler<CustomRoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomRoleRequirement requirement)
        {
            var role = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (String.IsNullOrEmpty(role)) 
            {
                return Task.CompletedTask;
            }

            if (requirement.Groups.Contains(role))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
