using Annie_API.Authorization;
using Annie_API.Models;
using Annie_API.UnitsOfWork.Interfaces;
using Microsoft.Build.Experimental.BuildCheck;
using Microsoft.EntityFrameworkCore;
using System.Composition;

namespace Annie_API.Data
{
    public class SeedData
    {
        private readonly DataContext _context;
        private readonly IUsersUnitOfWork _usersUnitOfWork;
        private readonly IConfiguration _configuration;
        private readonly Authorizator _authorizator = new Authorizator();

        public SeedData(DataContext context, IUsersUnitOfWork usersUnitOfWork, IConfiguration configuration)
        {
            _context = context;
            _usersUnitOfWork = usersUnitOfWork;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();
            await CheckRolesAsync();
            await CheckUsersAsync("AnniePlus", _configuration["Admin:Password"], _configuration["Admin:Email"], UserRole.Admin);
        }

        private async Task CheckRolesAsync()
        {
            await _usersUnitOfWork.CheckRoleAsync(UserRole.Admin.ToString());
            await _usersUnitOfWork.CheckRoleAsync(UserRole.User.ToString());
            await _usersUnitOfWork.CheckRoleAsync(UserRole.Instructor.ToString());
        }

        private async Task<User> CheckUsersAsync(string name, string password, string email, UserRole role)
        {
            var user = await _usersUnitOfWork.GetUserAsync(email);
            if (user == null)
            {
                user = new()
                {
                    Name = name,
                    Email = email,
                    Role = role,
                    UserName = email
                };

                var result = await _usersUnitOfWork.AddUserAsync(user, password);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException("Database could not be seeded.");
                }

                await _usersUnitOfWork.AddUserToRoleASync(user, role.ToString());

                // manually confirm the email of this user
                var token = await _usersUnitOfWork.CreateConfirmationToken(user);
                await _usersUnitOfWork.ConfirmEmailAsync(user, token);

            }
            
            return user;
        }
    }
}
