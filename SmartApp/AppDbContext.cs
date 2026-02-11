using Microsoft.EntityFrameworkCore;
using SmartApp.Data.Models; // Added using directive for AppUser

namespace SmartApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppUser> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
