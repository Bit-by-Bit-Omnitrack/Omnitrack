using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models.Dtos;
using System.Threading.Tasks;
using System.Linq;

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
        public async Task<IActionResult> GetSummaryReport()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .ToListAsync();

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
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return Json(summary);
        }

        // Remove unused scaffolded actions unless needed
        public IActionResult Details(int id) => View();
        public IActionResult Create() => View();
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(IFormCollection collection) => RedirectToAction(nameof(Index));
        public IActionResult Edit(int id) => View();
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(int id, IFormCollection collection) => RedirectToAction(nameof(Index));
        public IActionResult Delete(int id) => View();
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection collection) => RedirectToAction(nameof(Index));
    }
}