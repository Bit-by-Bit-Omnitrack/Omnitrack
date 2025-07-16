
using System.Data;

using System.Net.Sockets;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;
using UserRoles.ViewModels;

namespace UserRoles.Data
{

   // public class AppDbContext : IdentityDbContext<Users>
   // {
   //     public AppDbContext(DbContextOptions options) : base(options)
   //     {
    //    }

  //  }
}

/*  public AppDbContext(DbContextOptions<AppDbContext> options)
          : base(options) { 


  public DbSet<UserRoles.Models.Priority> Priorities { get; set; } = default!;
  public DbSet<UserRoles.Models.TaskItem> TaskItems { get; set; } = default!;
  public DbSet<UserRoles.Models.Checklists> Checklists { get; set; } = default!;
*/
public class AppDbContext : IdentityDbContext<Users>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Priority> Priorities { get; set; } = default!;
    public DbSet<TaskItem> TaskItems { get; set; } = default!;
    public DbSet<Checklists> Checklists { get; set; } = default!;
    public DbSet<Comments> Comments { get; set; } = default!;
    public DbSet<Users> UsersTable { get; set; } = default!;
    public DbSet<Role> Roles { get; set; } = default!;
    public DbSet<AppTask> AppTask { get; set; } = default!;
    public DbSet<Ticket> Tickets { get; set; } = default!;
    public DbSet<TicketAssignment> TicketAssignments { get; set; } = default!;
    public DbSet<TicketStatus> TicketStatuses { get; set; } = default!;
    public DbSet<ChecklistItem> ChecklistItems { get; set; } = default!;
    public DbSet<Chats> Chats { get; set; } = default!;
    public DbSet<EmailLog> EmailLogs { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Priority>().HasData(
            new Priority { Id = 1, Name = "Low", Color = "#28a745" },      // Green
            new Priority { Id = 2, Name = "Medium", Color = "#ffc107" },   // Yellow
            new Priority { Id = 3, Name = "High", Color = "#fd7e14" },     // Orange
            new Priority { Id = 4, Name = "Critical", Color = "#dc3545" }  // Red
        );

        modelBuilder.Entity<TicketStatus>().HasData(
            new TicketStatus { Id = 1, StatusName = "To Do" },
            new TicketStatus { Id = 2, StatusName = "In Progress" },
            new TicketStatus { Id = 3, StatusName = "Blocker" },
            new TicketStatus { Id = 4, StatusName = "Done" }
        );

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.AssignedToUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Status)
            .WithMany()
            .HasForeignKey(t => t.StatusID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

