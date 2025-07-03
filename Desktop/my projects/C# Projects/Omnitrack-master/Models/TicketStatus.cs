using System.ComponentModel.DataAnnotations;

public class TicketStatus
{
    public int StatusID { get; set; }

    [Required, StringLength(50)]
    public string StatusName { get; set; }

    // Navigation Property
   // public ICollection<Ticket> Tickets { get; set; }
}
