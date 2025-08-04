namespace UserRoles.ViewModels
{
    public class TicketSummaryViewModel
    {
        public int Id { get; set; }
        public string TicketID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string AssignedToUserId { get; set; }
        public string CreatedByID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int? TaskId { get; set; }
    }
}
