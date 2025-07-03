using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;
using UserRoles.ViewModels;

namespace UserRoles.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
      // public DbSet<UserRoles.Models.User> User { get; set; } = default!;
        // public DbSet<RegisterViewModel> AspNetUsers { get; set; }
     
    }
}

public class AppDbContext : DbContext
{
    public DbSet<Priority> Priorities { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
}
