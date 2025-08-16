using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserRoles.Models;

namespace UserRoles.Controllers
{
    [AllowAnonymous] // safe to show even if user is logged out
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            var model = new AboutViewModel
            {
                AppName = "OmniTrack",
                Version = "3.2.0",
                Mission = "Empower teams with a unified platform for tracking projects, tasks, and bugs—enabling transparency, accountability, and faster delivery.",
                Vision = "Become the go-to collaborative tracking solution that adapts to any industry, making teamwork effortless and goals achievable.",
                TeamName = "Bit by Bit",
                TeamMembers = new List<string>
                {
                    "Tshenolo Millicent Mogane",
                    "Azola Jokweni",
                    "Esther Mkhabela",
                    "Tlotlo Molefe"
                }
            };

            return View(model);
        }
    }
}
