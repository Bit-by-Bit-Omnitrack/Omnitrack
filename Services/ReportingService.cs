using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;
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

        // 🔹 Project Report
        public async Task<List<ProjectReportViewModel>> GetProjectReportsAsync()
        {
            return await _context.Projects
                .Include(p => p.Tasks)
                .ThenInclude(t => t.Tickets)
                .Select(p => new ProjectReportViewModel
                {
                    ProjectName = p.ProjectName,
                    TotalTasks = p.Tasks.Count(),
                    CompletedTasks = p.Tasks.Count(t => t.Status == "Done"),
                    OpenTickets = p.Tasks.SelectMany(t => t.Tickets).Count(),
                    Status = p.Tasks.All(t => t.Status == "Done") ? "Completed" : "Active"
                }).ToListAsync();
        }

        // 🔹 Task Report
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
                    ProjectName = t.Project.ProjectName,
                    TicketCount = t.Tickets.Count
                }).ToListAsync();
        }

        // 🔹 Ticket Summary Report
        public async Task<ReportViewModel> GetTicketSummaryAsync()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Status)
                .ToListAsync();

            return new ReportViewModel
            {
                TotalTickets = tickets.Count,
                OpenTickets = tickets.Count(t => t.Status.Name == "Open"),
                ClosedTickets = tickets.Count(t => t.Status.Name == "Closed")
            };
        }

        // 🔹 Optional: Task Summary (used in TicketReportViewModel)
        public async Task<int> GetTotalTasksAsync()
        {
            return await _context.Tasks.CountAsync();
        }

        public async Task<int> GetCompletedTasksAsync()
        {
            return await _context.Tasks.CountAsync(t => t.Status == "Done");
        }

        public async Task<int> GetOpenTasksAsync()
        {
            return await _context.Tasks.CountAsync(t => t.Status != "Done");
        }
    }
}