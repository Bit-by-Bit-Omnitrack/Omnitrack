using System;

namespace UserRoles.ViewModels
{
    public class ReportViewModel
    {
        
        public string TicketTitle { get; set; }
        public string TicketDescription { get; set; }
        public string TicketStatus { get; set; }
        public string TicketPriority { get; set; }
        public DateTime? TicketDueDate { get; set; }
        public DateTime? TicketCreatedDate { get; set; }

       
        public string TaskName { get; set; }
        public string TaskDetails { get; set; }
        public string TaskStatus { get; set; }
        public DateTime? TaskDueDate { get; set; }
        public DateTime? TaskCreatedDate { get; set; }

        
        public string AssignedToUser { get; set; }
        public string AssignedToUserEmail { get; set; }
        public string CreatedByUser { get; set; }
        public string CreatedByUserEmail { get; set; }
        public string AssignedToUserRole { get; set; }

       
        public string AssignedToUserTicket { get; set; }
        public string CreatedByTicket { get; set; }

 
        public string ProjectName { get; set; }
    }
}