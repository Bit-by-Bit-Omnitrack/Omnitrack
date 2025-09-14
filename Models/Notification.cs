using System;

namespace UserRoles.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }   // The recipient of the notification
        public string Message { get; set; }
        public string Type { get; set; }     // Task, Project, Ticket
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
