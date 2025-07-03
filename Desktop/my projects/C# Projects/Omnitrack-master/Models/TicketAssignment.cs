using System.ComponentModel.DataAnnotations;

public class TicketAssignment
{
    public int AssignmentID { get; set; }

    // Foreign Keys
    public int TicketID { get; set; }
    public int UserID { get; set; }

    // Navigation Properties
   // public Ticket Ticket { get; set; }
   // public User User { get; set; }
}
