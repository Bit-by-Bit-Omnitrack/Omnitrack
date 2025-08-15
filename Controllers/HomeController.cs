using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserRoles.Models;
using UserRoles.Data; // Gives access to AppDbContext and other data-related classes
using Microsoft.AspNetCore.Identity;


namespace UserRoles.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Users> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<Users> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }


        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "System Administrator")]
        public IActionResult SystemAdmin()
        {
            return View();
        }

        [Authorize(Roles = "User")]
        public IActionResult User()
        {
            return View();
        }

        //  System Admin landing page
        [Authorize(Roles = "System Administrator")]
        public IActionResult AdminLanding()
        {
            return View(); 
        }

        //  Welcome Landing Page
        [Authorize]
        public async Task<IActionResult> Welcome()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (currentUser == null)
            {
                return RedirectToAction("Index"); // or a login page
            }

            // Get the user's roles
            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains("System Administrator"))
                return RedirectToAction("AdminDashboard", "Home");

            if (roles.Contains("Project Leader"))
                return RedirectToAction("LeaderDashboard", "Home");

            if (roles.Contains("Developer"))
                return RedirectToAction("DeveloperDashboard", "Home");

            if (roles.Contains("Tester"))
                return RedirectToAction("TesterDashboard", "Home");

            // Default fallback if no matching role
            ViewBag.FullName = currentUser.FullName ?? "User";
            return View();
        }

        [Authorize(Roles = "System Administrator")]
        public IActionResult SystemAdminDashboard() => View();

        [Authorize(Roles = "Project Leader")]
        public IActionResult LeaderDashboard() => View();

        [Authorize(Roles = "Developer")]
        public IActionResult DeveloperDashboard() => View();

        [Authorize(Roles = "Tester")]
        public IActionResult TesterDashboard() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}