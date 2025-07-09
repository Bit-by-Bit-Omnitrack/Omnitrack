using System.ComponentModel.DataAnnotations;

namespace UserRoles.Models
{
    public class TicketAssignment
    {
        public int Id { get; set; }
        public int AssignmentID { get; set; }

        // Foreign Keys
        public int TicketID { get; set; }
        //   public int UserID { get; set; }

        // Navigation Properties
        public Ticket Ticket { get; set; }
        public Users User { get; set; }
    }
}