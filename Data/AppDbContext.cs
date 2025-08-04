using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserRoles.Models;
using UserRoles.ViewModels;

namespace UserRoles.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSet collections
        public DbSet<Priority> Priorities { get; set; } = default!;
        public DbSet<Tasks> Tasks { get; set; } = default!;
        public DbSet<Checklists> Checklists { get; set; } = default!;
        public DbSet<ChecklistItem> ChecklistItems { get; set; } = default!;
        public DbSet<Comments> Comments { get; set; } = default!;
        public DbSet<Users> UsersTable { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<Ticket> Tickets { get; set; } = default!;
        public DbSet<TicketAssignment> TicketAssignments { get; set; } = default!;
        public DbSet<TicketStatus> TicketStatuses { get; set; } = default!;
        public DbSet<Chats> Chats { get; set; } = default!;
        public DbSet<EmailLog> EmailLogs { get; set; } = default!;
        public DbSet<Project> Projects { get; set; } = default!;
        public DbSet<ProjectMember> ProjectMembers { get; set; } = default!;
        public DbSet<SystemAdmin> SystemAdmins { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tasks relationships
            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .IsRequired(false);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.AssignedToUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedToUserId)
                .IsRequired(false);

            //  Ticket relationships
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Tasks)
                .WithMany(t => t.Tickets)
                .HasForeignKey(t => t.TasksId)
                .IsRequired(false);

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

            //  ProjectMember composite key and relationships
            modelBuilder.Entity<ProjectMember>()
                .HasKey(pm => new { pm.ProjectId, pm.UserId });

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMemberships)
                .HasForeignKey(pm => pm.UserId);

            //  Seeding Priorities
            modelBuilder.Entity<Priority>().HasData(
                new Priority { Id = 1, Name = "Low", Color = "#28a745" },
                new Priority { Id = 2, Name = "Medium", Color = "#ffc107" },
                new Priority { Id = 3, Name = "High", Color = "#fd7e14" },
                new Priority { Id = 4, Name = "Critical", Color = "#dc3545" }
            );

            // Seeding Ticket Statuses
            modelBuilder.Entity<TicketStatus>().HasData(
                new TicketStatus { Id = 1, StatusName = "To Do" },
                new TicketStatus { Id = 2, StatusName = "In Progress" },
                new TicketStatus { Id = 3, StatusName = "Blocker" },
                new TicketStatus { Id = 4, StatusName = "Done" }
            );
        }
        public DbSet<UserRoles.ViewModels.TicketSummaryViewModel> TicketSummaryViewModel { get; set; } = default!;
    }
}