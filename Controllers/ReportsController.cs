using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UserRoles.Services;
using UserRoles.ViewModels;

namespace UserRoles.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ReportingService _reportingService;

        public ReportsController(ReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        public ActionResult Index() => View();
        public ActionResult Details(int id) => View();
        public ActionResult Create() => View();
        public ActionResult Edit(int id) => View();
        public ActionResult Delete(int id) => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try { return RedirectToAction(nameof(Index)); }
            catch { return View(); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try { return RedirectToAction(nameof(Index)); }
            catch { return View(); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try { return RedirectToAction(nameof(Index)); }
            catch { return View(); }
        }

        //  Project Report
        public async Task<IActionResult> ProjectReport()
        {
            var reports = await _reportingService.GetProjectReportsAsync();
            return View(reports);
        }

        //  Task Report
        public async Task<IActionResult> TaskReport()
        {
            var reports = await _reportingService.GetTaskReportsAsync();
            return View(reports);
        }

        //  Ticket Report
        public async Task<IActionResult> TicketReport()
        {
            var tickets = await _reportingService.GetTicketSummaryAsync(); // You need to implement this method

            var report = new ReportViewModel
            {
                TotalTickets = tickets.Count(),
                OpenTickets = tickets.Count(t => t.Status.Name == "Open"),
                ClosedTickets = tickets.Count(t => t.Status.Name == "Closed")
            };

            return View(report);
        }
    }
}