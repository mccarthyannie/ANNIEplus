using Microsoft.EntityFrameworkCore;

namespace Annie_API.Models
{
    public class SessionContext: DbContext
    {
        public SessionContext(DbContextOptions<SessionContext> options) : base(options) { }

        public DbSet<Session> Sessions { get; set; } = null!;
    }
}
