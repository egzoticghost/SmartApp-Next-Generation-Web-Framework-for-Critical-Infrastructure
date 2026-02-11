using Microsoft.EntityFrameworkCore;

namespace SmartWebApp.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        // Add your DbSet<T> properties here
        // public DbSet<User> Users { get; set; }
    }
}
