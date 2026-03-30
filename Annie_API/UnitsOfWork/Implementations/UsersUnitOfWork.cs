using Annie_API.DTOs;
using Annie_API.Models;
using Annie_API.Repositories.Interfaces;
using Annie_API.UnitsOfWork.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Annie_API.UnitsOfWork.Implementations
{
    public class UsersUnitOfWork : IUsersUnitOfWork
    {
        private readonly IUsersRepository _usersRepository;

        public UsersUnitOfWork(IUsersRepository usersRepository) 
        {
            _usersRepository = usersRepository;
        }

        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _usersRepository.AddUserAsync(user, password);
        }

        public async Task AddUserToRoleASync(User user, string roleName)
        {
            await _usersRepository.AddUserToRoleASync(user, roleName);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            await _usersRepository.CheckRoleAsync(roleName);
        }

        public async Task<User> GetUserAsync(string email)
        {
            return await _usersRepository.GetUserAsync(email);
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _usersRepository.IsUserInRoleAsync(user, roleName);
        }
        public async Task<SignInResult> LoginAsync(LoginRequest request)
        {
            return await _usersRepository.LoginAsync(request);
        }

        public async Task LogoutAsync()
        {
            await _usersRepository.LogoutAsync();
        }

        public async Task<string> CreateConfirmationToken(User user)
        {
            return await _usersRepository.CreateConfirmationToken(user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _usersRepository.ConfirmEmailAsync(user, token);
        }
    }
}
