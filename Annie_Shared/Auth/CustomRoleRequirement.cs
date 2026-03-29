using Microsoft.AspNetCore.Authorization;

namespace Annie_Shared.Auth
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