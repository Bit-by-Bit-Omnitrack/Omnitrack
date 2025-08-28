using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserRoles.Controllers
{
    [Authorize] // Only logged-in users can access
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            if (User.IsInRole("System administrator"))
                return RedirectToAction("AdminDashboard");
            if (User.IsInRole("Project leader"))
                return RedirectToAction("LeaderDashboard");
            if (User.IsInRole("Developer"))
                return RedirectToAction("DeveloperDashboard");
            if (User.IsInRole("Tester"))
                return RedirectToAction("TesterDashboard");

            // Fallback if no role matches
            return RedirectToAction("AccessDenied", "Account");
        }

        [Authorize(Roles = "System Administrator")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Project Leader")]
        public IActionResult LeaderDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Developer")]
        public IActionResult DeveloperDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Tester")]
        public IActionResult TesterDashboard()
        {
            return View();
        }
    }
}
