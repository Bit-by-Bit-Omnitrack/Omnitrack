using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserRoles.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string TicketID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
       

        public DateTime? DueDate { get; set; }
        public int StatusID { get; set; } = 1;
        public string TaskID { get; set; }

        // Use this for the creator's User ID
        public string CreatedByID { get; set; }
        [ForeignKey("CreatedByID")]
       public Users CreatedByUser { get; set; } // Navigation property to the creator

        public DateTime CreatedDate { get; set; }
       
        public string? UpdatedBy { get; set; } // This should probably also be a UserId (nullable string)
        public DateTime? UpdatedDate { get; set; }

      //   This is for the user the ticket is assigned TO
        public string? AssignedToUserId { get; set; } // Make nullable

        [ForeignKey("AssignedToUserId")]
        public Users AssignedToUser { get; set; }

      //   Navigation property for Status
        [ForeignKey("StatusID")]
      public TicketStatus Status { get; set; }

        public int PriorityId { get; set; } // Foreign key reference
        public Priority Priority { get; set; } // Navigation property
    }

}
