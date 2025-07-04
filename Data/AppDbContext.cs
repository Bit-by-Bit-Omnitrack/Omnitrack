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
       
    }
}



    public class AppDbContext : IdentityDbContext<Users>
{
        public DbSet<UserRoles.Models.Priority> Priorities { get; set; }
        public DbSet<UserRoles.Models.TaskItem> TaskItems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<UserRoles.Models.Checklists> Checklists { get; set; } = default!;

        public DbSet<UserRoles.Models.Comments> Comments { get; set; } = default!;
    }
