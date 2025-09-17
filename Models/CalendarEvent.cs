using UserRoles.Models;

public class CalendarEvent
{
    public int Id { get; set; } 
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string EventType { get; set; }
    public string UserId { get; set; }
    public int? TicketId { get; set; }
    public Ticket Ticket { get; set; }
    public int? TasksId { get; set; }
    public Tasks Tasks { get; set; }
    public int? ProjectId { get; set; }
    public Project Project { get; set; }
}