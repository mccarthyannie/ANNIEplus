using Microsoft.EntityFrameworkCore;

namespace Annie_API.Models
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
    }
}
