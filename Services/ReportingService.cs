using UserRoles.Data;
using UserRoles.ViewModels;

namespace UserRoles.Services
{
    public class ReportingService
    {
        private readonly AppDbContext _context;

        public ReportingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectReportViewModel>> GetProjectReportsAsync()
        {
            return await _context.Projects
                .Select(p => new ProjectReportViewModel
                {
                    ProjectName = p.Name,
                    TotalTasks = p.Tasks.Count(),
                    CompletedTasks = p.Tasks.Count(t => t.Status == "Done"),
                    OpenTickets = p.Tasks.SelectMany(t => t.Tickets).Count(),
                    Status = p.Tasks.All(t => t.Status == "Done") ? "Completed" : "Active"
                }).ToListAsync();
        }

        public async Task<List<TaskReportViewModel>> GetTaskReportsAsync()
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Tickets)
                .Select(t => new TaskReportViewModel
                {
                    TaskTitle = t.Title,
                    AssignedTo = t.AssignedTo,
                    DueDate = t.DueDate,
                    Status = t.Status,
                    Priority = t.Priority,
                    ProjectName = t.Project.Name,
                    TicketCount = t.Tickets.Count
                }).ToListAsync();
        }
    }
}
