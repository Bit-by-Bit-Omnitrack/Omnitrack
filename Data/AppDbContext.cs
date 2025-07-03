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
       public DbSet<UserRoles.Models.Checklists> Checklists { get; set; } = default!;
        // public DbSet<RegisterViewModel> AspNetUsers { get; set; }
        public DbSet<UserRoles.Models.Comments> comments { get; set; } = default!;
    }
}

public class AppDbContext : DbContext
{
    public DbSet<Priority> Priorities { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
}
