using Annie_API.Data;
using Annie_API.DTOs;
using Annie_API.Models;
using Annie_API.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices.Marshalling;

namespace Annie_API.Repositories.Implementations
{
    public class UsersRepository : IUsersRepository
    {

        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public UsersRepository(DataContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager) 
        { 
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task AddUserToRoleASync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists) {
                await _roleManager.CreateAsync(new IdentityRole{ Name = roleName});
            }
        }

        public async Task<User> GetUserAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            return user!;
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<SignInResult> LoginAsync(LoginRequest request) 
        {
            return await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, true);
        }

        public async Task LogoutAsync()
        { 
            await _signInManager.SignOutAsync();
        }

        public async Task<string> CreateConfirmationToken(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<string> CreateResetPasswordToken(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }
    }
}
