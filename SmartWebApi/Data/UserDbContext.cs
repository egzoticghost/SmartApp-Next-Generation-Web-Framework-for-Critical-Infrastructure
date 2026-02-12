using Microsoft.EntityFrameworkCore;

namespace SmartWebApi.Data
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
