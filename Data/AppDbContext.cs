using System.Data;
using System.Net.Sockets;
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
        public DbSet<UserRoles.Models.Priority> Priorities { get; set; } = default!;
    public DbSet<UserRoles.Models.TaskItem> TaskItems { get; set; } = default!;

    public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<UserRoles.Models.Checklists> Checklists { get; set; } = default!;

        public DbSet<UserRoles.Models.Comments> Comments { get; set; } = default!;
    // public DbSet<UserRoles.Models.User> User { get; set; } = default!;
    // public DbSet<RegisterViewModel> AspNetUsers { get; set; }
    public DbSet<Role> Roles { get; set; } = default!;
    public DbSet<AppTask> AppTasks { get; set; } = default!;
    public DbSet<Ticket> Tickets { get; set; } = default!;
    public DbSet<Priority> Priorities { get; set; } = default!;
    public DbSet<TaskItem> TaskItems { get; set; } = default!;
    public DbSet<TicketAssignment> TicketAssignments { get; set; } = default!;
    public DbSet<TicketStatus> TicketStatuses { get; set; } = default!;
    public DbSet<ChecklistItem> ChecklistItems { get; set; } = default!;
}
