namespace UserRoles.Models.Dtos
{
    public class CalendarEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string EventType { get; set; }
        public int? TicketId { get; set; }
        public int? TasksId { get; set; }
        public int? ProjectId { get; set; }
    }
}
