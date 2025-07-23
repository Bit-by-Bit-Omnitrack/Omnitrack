using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserRoles.Models
{
    public class Tasks
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("AssignedToUser")] // This links to the navigation property name
        public string? AssignedToUserId { get; set; } // This is the actual foreign key column

        public Users? AssignedToUser { get; set; } // Navigation property to the User

        public string? CreatedBy { get; set; } // This currently stores the CreatedBy user's ID string

        [NotMapped] // If you want to store the username directly when saving, consider this, but it's less ideal.
        public string? CreatedByUserName { get; set; } // To hold the username for display in scenarios where CreatedBy is the ID.


        public string Details { get; set; }


        public DateTime DueDate { get; set; }
        public string AssignedTo { get; internal set; }
    }
}
