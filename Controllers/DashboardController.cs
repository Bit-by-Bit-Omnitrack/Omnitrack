using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserRoles.Controllers
{
    [Authorize] // Only logged-in users can access
    public class DashboardController : Controller
    {
        public IActionResult AdminDashboard()
        {
            return View();
        }

        public IActionResult LeaderDashboard()
        {
            return View();
        }

        public IActionResult DeveloperDashboard()
        {
            return View();
        }

        public IActionResult TesterDashboard()
        {
            return View();
        }
    }
}
