using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UserRoles.Data; // Gives access to AppDbContext and other data-related classes
using UserRoles.Models;




namespace UserRoles.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        // This sets up both logging and database access
        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        public IActionResult Index()
        {
            // If the user is a system administrator
            if (User.IsInRole("System Administrator"))
            {
                return RedirectToAction("AdminLanding", "Home");
            }

            // If the user is a regular user (Developer, Tester, Project Leader are inside this role)
            if (User.IsInRole("User"))
            {
                return RedirectToAction("UserDashboard", "Home");
            }

            // Default fallback
            return RedirectToAction("Login", "Account");
        }



        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }


        [Authorize(Roles = "User")]
        public IActionResult UserDashboard()
        {
            return View("User"); // Loads Views/Home/User.cshtml
        }


        [Authorize(Roles = "System Administrator")]
        public IActionResult AdminLanding()
        {
            // Count users by approval status
            var approved = _context.Users.Count(u => u.ApprovalStatus == UserApprovalStatus.Approved);
            var pending = _context.Users.Count(u => u.ApprovalStatus == UserApprovalStatus.Pending);
            var rejected = _context.Users.Count(u => u.ApprovalStatus == UserApprovalStatus.Rejected);

            // Count all tickets in the system
            var totalTickets = _context.Tickets.Count();

            // Pass data to view
            ViewBag.ApprovedUsers = approved;
            ViewBag.PendingUsers = pending;
            ViewBag.RejectedUsers = rejected;
            ViewBag.TotalTickets = totalTickets;

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
