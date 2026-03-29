using Microsoft.AspNetCore.Authorization;

namespace AnniePlus.AuthenticationProviders
{
    public class CustomRoleRequirement : IAuthorizationRequirement
    {
        public string[] Groups { get; }

        public CustomRoleRequirement(string[] groups)
        {
            Groups = groups;
        }
    }
}