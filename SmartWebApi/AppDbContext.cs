using Microsoft.EntityFrameworkCore;
using SmartWebApi.Models; // <-- Add this using directive

namespace SmartWebApi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppUser> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}

