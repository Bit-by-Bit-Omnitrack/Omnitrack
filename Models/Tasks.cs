using System;
using System.Collections.Generic; // Add this using directive
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserRoles.Models
{
    public class Tasks
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("AssignedToUser")]
        public string? AssignedToUserId { get; set; }
        public Users? AssignedToUser { get; set; }

        [ForeignKey("CreatedByUser")]
        public string? CreatedById { get; set; }
        public Users? CreatedByUser { get; set; }

        public string Details { get; set; }

        public DateTime DueDate { get; set; }

        public string AssignedTo { get; internal set; }

        // New: Navigation property for Tickets associated with this Task
        public ICollection<Ticket>? Tickets { get; set; }

    }
}