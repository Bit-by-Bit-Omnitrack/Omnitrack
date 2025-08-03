namespace UserRoles.DTOs
{
    public class TicketSummaryDto
    {
        public int Id { get; set; }
        public string TicketID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string Status { get; set; }
        public string Priority { get; set; }

        public string? AssignedTo { get; set; }
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }

        public string? TaskTitle { get; set; }
    }
}}
}
