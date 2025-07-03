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
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        Complete,
        Blocked
    }
}
