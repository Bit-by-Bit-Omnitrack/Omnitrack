using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace UserRoles.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ReportsController
        public IActionResult Index()
        {
            return View();
        }

        // GET: ReportsController/Summary
        public async Task<IActionResult> Summary()
        {
            return View();
        }

        // GET: ReportsController/GetSummaryReport
        [HttpGet]
        public async Task<IActionResult> GetSummaryReport(DateTime? startDate, DateTime? endDate, string project, string user)
        {
            var ticketsQuery = _context.Tickets
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .AsQueryable();

            // 🗓️ Default to last 30 days if no date range is provided
            if (!startDate.HasValue && !endDate.HasValue)
            {
                var today = DateTime.Today;
                startDate = today.AddDays(-30);
                endDate = today;
            }

            if (startDate.HasValue)
                ticketsQuery = ticketsQuery.Where(t => t.CreatedDate >= startDate.Value);

            if (endDate.HasValue)
                ticketsQuery = ticketsQuery.Where(t => t.CreatedDate <= endDate.Value);

            if (!string.IsNullOrEmpty(project))
                ticketsQuery = ticketsQuery.Where(t => t.Project.ProjectName == project);

            if (!string.IsNullOrEmpty(user))
                ticketsQuery = ticketsQuery.Where(t => t.AssignedToUser.FullName == user);

            var tickets = await ticketsQuery.ToListAsync();

            var summary = new TicketSummaryDto
            {
                TotalTickets = tickets.Count,

                StatusCounts = tickets
                    .GroupBy(t => t.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),

                PriorityCounts = tickets
                    .GroupBy(t => t.Priority)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),

                ProjectCounts = tickets
                    .GroupBy(t => t.Project.ProjectName)
                    .ToDictionary(g => g.Key, g => g.Count()),

                UserCounts = tickets
                    .GroupBy(t => t.AssignedToUser.FullName)
                    .ToDictionary(g => g.Key, g => g.Count()),

                TicketTrends = tickets
                    .GroupBy(t => t.CreatedDate.Date)
                    .Select(g => new TicketTrendDto
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(t => t.Date)
                    .ToList(),

                Projects = tickets
                    .Select(t => t.Project.ProjectName)
                    .Distinct()
                    .ToList(),

                Users = tickets
                    .Select(t => t.AssignedToUser.FullName)
                    .Distinct()
                    .ToList()
            };

            return Json(summary);
        }
    }
}