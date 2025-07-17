using System;
using System.ComponentModel.DataAnnotations;

namespace UserRoles.Models
{
    public class Tasks
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Task Name")]
        public string Name { get; set; }

        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Task Details")]
        public string Details { get; set; }

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required]
        [Display(Name = "Status")]
        public TaskStatus Status { get; set; } // Enum to track task phase

        // Foreign key to Ticket
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        // Foreign Key to AspNetUsers table
        public string? UserId { get; set; }
        public Users? User { get; set; }    // Navigation property

        // Foreign Key
        public string? CreatedById { get; set; } // Foreign key

    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        Complete,
        Blocked
    }
}
