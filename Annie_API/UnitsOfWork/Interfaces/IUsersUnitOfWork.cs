using Annie_API.DTOs;
using Annie_API.Models;
using Microsoft.AspNetCore.Identity;

namespace Annie_API.UnitsOfWork.Interfaces
{
    public interface IUsersUnitOfWork
    {
        Task<User> GetUserAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task CheckRoleAsync(string roleName);

        Task AddUserToRoleASync(User user, string roleName);

        Task<bool> IsUserInRoleAsync(User user, string roleName);

        Task<SignInResult> LoginAsync(LoginRequest request);
        
        Task LogoutAsync();
    }
}
