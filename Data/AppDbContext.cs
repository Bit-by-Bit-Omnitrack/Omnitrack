using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;
//using UserRoles.ViewModels;

namespace UserRoles.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        //public AppDbContext(DbContextOptions options) : base(options)

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) // Changed constructor to use DbContextOptions<AppDbContext> to fix scaffolding and build error
        {
        }
        // public DbSet<UserRoles.Models.User> User { get; set; } = default!;
        // public DbSet<RegisterViewModel> AspNetUsers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AppTask> AppTasks { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TicketAssignment> TicketAssignments { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
    }
}
