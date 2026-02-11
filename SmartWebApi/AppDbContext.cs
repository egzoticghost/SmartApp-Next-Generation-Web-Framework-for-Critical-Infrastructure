using Microsoft.EntityFrameworkCore;
using SmartWebApp.Models; // <-- Add this using directive

namespace SmartWebApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppUser> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
