using Microsoft.Build.Experimental.BuildCheck;
using Microsoft.EntityFrameworkCore;
using System.Composition;

namespace Annie_API.Models
{
    public class SeedData
    {
        private readonly DataContext _context;
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
                _context.Users.Add(new User { Name="AnniePlus", Password = "admin", Email = "agustin.egui@gmail.com", Role = UserRole.Admin });

                await _context.SaveChangesAsync();
            }
        }
    }
}
