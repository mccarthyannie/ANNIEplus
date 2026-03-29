using Annie_API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Annie_API.Data
{
    public class DataContext: IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
    }
}
