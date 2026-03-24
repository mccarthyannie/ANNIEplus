using Annie_API.Authorization;
using Microsoft.Build.Experimental.BuildCheck;
using Microsoft.EntityFrameworkCore;
using System.Composition;

namespace Annie_API.Models
{
    public class SeedData
    {
        private readonly DataContext _context;
        private readonly Authorizator _authorizator = new Authorizator();
        public SeedData(DataContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();
            await CheckUsersAsync();
        }

        private async Task CheckUsersAsync()
        {
            if (!_context.Users.Any())
            {
                User user = new User
                {
                    Name = "AnniePlus",
                    Password = _authorizator.HashPassword("admin"),
                    Email = "agustin.egui@gmail.com",
                    Role = UserRole.Admin
                };
                _context.Users.Add(user);

                await _context.SaveChangesAsync();
            }
        }
    }
}
