
﻿using System.Data;

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

    public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        
    public DbSet<UserRoles.Models.Priority> Priorities { get; set; } = default!;
    public DbSet<UserRoles.Models.TaskItem> TaskItems { get; set; } = default!;
    public DbSet<UserRoles.Models.Checklists> Checklists { get; set; } = default!;

    public DbSet<UserRoles.Models.Comments> Comments { get; set; } = default!;
    // public DbSet<UserRoles.Models.User> User { get; set; } = default!;
    // public DbSet<RegisterViewModel> AspNetUsers { get; set; }
    public DbSet<Role> Roles { get; set; } = default!;
    public DbSet<AppTask> AppTask { get; set; } = default!;
    public DbSet<Ticket> Tickets { get; set; } = default!;
   
    public DbSet<TicketAssignment> TicketAssignments { get; set; } = default!;
    public DbSet<TicketStatus> TicketStatuses { get; set; } = default!;
    public DbSet<ChecklistItem> ChecklistItems { get; set; } = default!;

    public DbSet<Chats> Chats { get; set; } = default!;
    // In UserRoles.Data/AppDbContext.cs
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

        // Configure Ticket -> Users (AssignedToUser)
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.AssignedToUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull); // SetNull is appropriate here

       //  Configure Ticket -> Users (CreatedByUser)
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.CreatedByUser) // Reference the navigation property
            .WithMany()
            .HasForeignKey(t => t.CreatedByID) // Use the CreatedByID foreign key
          .OnDelete(DeleteBehavior.Restrict); // Restrict is good for the creator
    
        // Configure Ticket -> TicketStatus
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Status) // Assuming your Ticket model has a 'Status' navigation property
            .WithMany() // Or WithMany(s => s.Tickets) if you add a collection to TicketStatus
            .HasForeignKey(t => t.StatusID)
            .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict for safety

        // Ensure AppUser configuration (IdentityUser properties) are handled by base.OnModelCreating
        // If you had custom IdentityUser relationships, they would go here too.
    }

}

