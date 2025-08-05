namespace UserRoles.ViewModels
{
    public class TaskReportViewModel
    {
        public string TaskTitle { get; set; }
        public string AssignedTo { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string ProjectName { get; set; }
        public int TicketCount { get; set; }

    }
}
