using Microsoft.AspNetCore.Mvc;

namespace UserRoles.Controllers
{
    public class ContactController : Controller
    {
        // GET: /Contact/
        public IActionResult Index()
        {
            return View();
        }
    }
}
