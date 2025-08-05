namespace UserRoles.ViewModels
{
    public class ProjectReportViewModel
    {
        public string ProjectName { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OpenTickets { get; set; }
        public string Status { get; set; }

    }
}
