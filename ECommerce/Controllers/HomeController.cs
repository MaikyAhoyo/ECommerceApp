using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/")]
        public IActionResult Index()
        {
            return RedirectToAction("Login", "Auth");
        }
    }
}
