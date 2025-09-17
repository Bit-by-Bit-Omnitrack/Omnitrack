using Microsoft.AspNetCore.Mvc;

namespace UserRoles.Controllers
{
    public class CalendarController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
