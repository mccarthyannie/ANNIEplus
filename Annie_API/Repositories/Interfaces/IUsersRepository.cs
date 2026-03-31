using Annie_API.DTOs;
using Annie_API.Models;
using Microsoft.AspNetCore.Identity;

namespace Annie_API.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        Task<User> GetUserAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task CheckRoleAsync(string roleName);

        Task AddUserToRoleASync(User user, string roleName);

        Task<bool> IsUserInRoleAsync(User user, string roleName);

        Task<SignInResult> LoginAsync(LoginRequest request);

        Task LogoutAsync();

        Task<string> CreateConfirmationToken(User user);

        Task<IdentityResult> ConfirmEmailAsync(User user, string token);

        Task<string> CreateResetPasswordToken(User user);

        Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);

    }
}
