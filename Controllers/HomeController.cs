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

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
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

            ViewBag.FullName = currentUser?.FullName ?? "User";
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
