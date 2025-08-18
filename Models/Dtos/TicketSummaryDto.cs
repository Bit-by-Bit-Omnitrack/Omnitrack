namespace UserRoles.Models.Dtos
{
    public class TicketSummaryDto
    {
        public int TotalTickets { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; }
        public Dictionary<string, int> PriorityCounts { get; set; }
        public Dictionary<string, int> ProjectCounts { get; set; }
        public Dictionary<string, int> UserCounts { get; set; }

        public List<TicketTrendDto> TicketTrends { get; set; }  // For line chart
        public List<string> Projects { get; set; }              // For dropdown filter
        public List<string> Users { get; set; }                 
    }
}
